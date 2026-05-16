using Content.Server._CCM.Stats;
using Content.Server.NPC.HTN;
using Content.Shared.Actions.Components;
using Content.Shared._CCM.Stats;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Survivor;
using Content.Shared._RMC14.Synth;
using Content.Shared._RMC14.Vehicle;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Vehicle.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.KillTracking;

/// <summary>
/// This handles <see cref="KillTrackerComponent"/> and recording who is damaging and killing entities.
/// </summary>
public sealed class KillTrackingSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        // Add damage to LifetimeDamage before MobStateChangedEvent gets raised
        SubscribeLocalEvent<KillTrackerComponent, DamageChangedEvent>(OnDamageChanged, before: [typeof(MobThresholdSystem)]);
        SubscribeLocalEvent<KillTrackerComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public void EnsureKillTracker(EntityUid uid, MobState killState)
    {
        var tracker = EnsureComp<KillTrackerComponent>(uid);
        tracker.KillState = killState;
    }

    private void OnDamageChanged(EntityUid uid, KillTrackerComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
            return;

        if (!args.DamageIncreased)
        {
            foreach (var key in component.LifetimeDamage.Keys)
            {
                component.LifetimeDamage[key] -= args.DamageDelta.GetTotal();
            }

            return;
        }

        var source = GetKillSource(args.Origin, args.Tool);
        var damage = component.LifetimeDamage.GetValueOrDefault(source);
        component.LifetimeDamage[source] = damage + args.DamageDelta.GetTotal();
    }

    private void OnMobStateChanged(EntityUid uid, KillTrackerComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != component.KillState || args.OldMobState >= args.NewMobState)
            return;

        // impulse is the entity that did the finishing blow.
        var killImpulse = GetKillSource(args.Origin);

        // source is the kill tracker source with the most damage dealt.
        var largestSource = GetLargestSource(component.LifetimeDamage);
        largestSource ??= killImpulse;

        KillSource killSource;
        KillSource? assistSource = null;

        if (killImpulse is KillEnvironmentSource)
        {
            // if the kill was environmental, whatever did the most damage gets the kill.
            killSource = largestSource;
        }
        else if (killImpulse == largestSource)
        {
            // if the impulse and the source are the same, there's no assist
            killSource = killImpulse;
        }
        else
        {
            // the impulse gets the kill and the most damage gets the assist
            killSource = killImpulse;

            // no assist is given to environmental kills
            if (largestSource is not KillEnvironmentSource
                && component.LifetimeDamage.TryGetValue(largestSource, out var largestDamage))
            {
                var killDamage = component.LifetimeDamage.GetValueOrDefault(killSource);
                // you have to do at least twice as much damage as the killing source to get the assist.
                if (largestDamage >= killDamage / 2)
                    assistSource = largestSource;
            }
        }

        // it's a suicide if:
        // - you caused your own death
        // - the kill source was the entity that died
        // - the entity that died had an assist on themselves
        var suicide = args.Origin == uid ||
                      killSource is KillNpcSource npc && npc.NpcEnt == uid ||
                      killSource is KillPlayerSource player && player.PlayerId == CompOrNull<ActorComponent>(uid)?.PlayerSession.UserId ||
                      assistSource is KillNpcSource assistNpc && assistNpc.NpcEnt == uid ||
                      assistSource is KillPlayerSource assistPlayer && assistPlayer.PlayerId == CompOrNull<ActorComponent>(uid)?.PlayerSession.UserId;

        var ev = new KillReportedEvent(uid, killSource, assistSource, suicide);
        RaiseLocalEvent(uid, ref ev, true);
    }

    private KillSource GetKillSource(EntityUid? sourceEntity, EntityUid? toolEntity = null)
    {
        if (TryResolveKillSource(sourceEntity, out var source))
            return source;
        if (TryResolveKillSource(toolEntity, out source))
            return source;

        return new KillEnvironmentSource();
    }

    private bool TryResolveKillSource(EntityUid? sourceEntity, out KillSource source)
    {
        source = default!;
        if (sourceEntity == null)
            return false;

        var visited = new HashSet<EntityUid>();
        return TryResolveKillSource(sourceEntity.Value, visited, out source);
    }

    private bool TryResolveKillSource(EntityUid sourceEntity, HashSet<EntityUid> visited, out KillSource source)
    {
        source = default!;
        if (!visited.Add(sourceEntity))
            return false;

        if (TryComp<CCMStatsProjectileSourceComponent>(sourceEntity, out var projectileSource))
        {
            source = new KillPlayerSource(projectileSource.UserId, projectileSource.Side);
            return true;
        }

        var current = sourceEntity;
        var userId = default(NetUserId);
        var side = CCMStatsSide.None;

        for (var depth = 0; depth < 8; depth++)
        {
            if (userId == default &&
                TryComp<ActorComponent>(current, out var actor))
            {
                userId = actor.PlayerSession.UserId;
            }

            if (userId == default &&
                TryComp(current, out MindContainerComponent? mindContainer) &&
                mindContainer.Mind is { } mindId &&
                TryComp(mindId, out MindComponent? mind) &&
                mind.UserId is { } mindUserId)
            {
                userId = mindUserId;
            }

            if (side == CCMStatsSide.None)
                side = GetSide(current);

            if (userId == default &&
                TryComp(current, out VehicleWeaponsComponent? vehicleWeapons) &&
                vehicleWeapons.Operator is { } weaponOperator &&
                weaponOperator != current &&
                TryResolveKillSource(weaponOperator, visited, out source))
            {
                return true;
            }

            if (userId == default &&
                TryComp(current, out VehicleComponent? vehicle) &&
                vehicle.Operator is { } vehicleOperator &&
                vehicleOperator != current &&
                TryResolveKillSource(vehicleOperator, visited, out source))
            {
                return true;
            }

            if (userId == default &&
                TryComp(current, out ActionComponent? action))
            {
                if (action.AttachedEntity is { } attached &&
                    attached != current &&
                    TryResolveKillSource(attached, visited, out source))
                {
                    return true;
                }

                if (action.Container is { } container &&
                    container != current &&
                    TryResolveKillSource(container, visited, out source))
                {
                    return true;
                }
            }

            if (HasComp<HTNComponent>(current))
            {
                source = new KillNpcSource(current);
                return true;
            }

            if (userId != default && side != CCMStatsSide.None)
            {
                source = new KillPlayerSource(userId, side);
                return true;
            }

            if (!TryComp(current, out TransformComponent? xform) ||
                xform.ParentUid == EntityUid.Invalid ||
                xform.ParentUid == current)
            {
                break;
            }

            current = xform.ParentUid;
        }

        if (TryComp(sourceEntity, out ProjectileComponent? projectile))
        {
            if (projectile.Shooter is { } shooter &&
                shooter != sourceEntity &&
                TryResolveKillSource(shooter, visited, out source))
            {
                return true;
            }

            if (projectile.Weapon is { } weapon &&
                weapon != sourceEntity &&
                TryResolveKillSource(weapon, visited, out source))
            {
                return true;
            }
        }

        if (userId == default)
            return false;

        source = new KillPlayerSource(userId, side);
        return true;
    }

    private CCMStatsSide GetSide(EntityUid uid)
    {
        if (HasComp<MarineComponent>(uid))
            return CCMStatsSide.Marines;
        if (HasComp<RMCSurvivorComponent>(uid))
            return CCMStatsSide.Marines;
        if (HasComp<SynthComponent>(uid))
            return CCMStatsSide.Marines;
        if (HasComp<XenoComponent>(uid))
            return CCMStatsSide.Xenos;
        return CCMStatsSide.None;
    }

    private KillSource? GetLargestSource(Dictionary<KillSource, FixedPoint2> lifetimeDamages)
    {
        KillSource? maxSource = null;
        var maxDamage = FixedPoint2.Zero;
        foreach (var (source, damage) in lifetimeDamages)
        {
            if (damage < maxDamage)
                continue;
            maxSource = source;
            maxDamage = damage;
        }

        return maxSource;
    }
}

/// <summary>
/// Event broadcasted and raised by-ref on an entity with <see cref="KillTrackerComponent"/> when they are killed.
/// </summary>
/// <param name="Entity">The entity that was killed</param>
/// <param name="Primary">The primary source of the kill</param>
/// <param name="Assist">A secondary source of the kill. Can be null.</param>
/// <param name="Suicide">True if the entity that was killed caused their own death.</param>
[ByRefEvent]
public readonly record struct KillReportedEvent(EntityUid Entity, KillSource Primary, KillSource? Assist, bool Suicide);
