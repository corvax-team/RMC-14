using System.Numerics;
using Content.Shared._RMC14.Movement;
using Content.Shared._RMC14.Slow;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._CCM.Silencer;

public sealed class CCMSonicPulseSystem : EntitySystem
{
    [Dependency] private readonly CCMSonicCarbineSystem _carbine = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedRMCLagCompensationSystem _lagCompensation = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly RMCSizeStunSystem _sizeStun = default!;
    [Dependency] private readonly RMCSlowSystem _slow = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private const float LookupMargin = 1.5f;

    private const float SmallRadius = 0.45f;
    private const float LargeRadius = 0.9f;

    private readonly HashSet<Entity<XenoComponent>> _targets = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<CCMSonicPulseComponent, AmmoShotEvent>(OnAmmoShot);
        SubscribeLocalEvent<CCMSonicPulseComponent, GunShotEvent>(OnGunShot);
    }

    private void OnAmmoShot(Entity<CCMSonicPulseComponent> ent, ref AmmoShotEvent args)
    {
        foreach (var projectile in args.FiredProjectiles)
        {
            PredictedQueueDel(projectile);
        }
    }

    private void OnGunShot(Entity<CCMSonicPulseComponent> ent, ref GunShotEvent args)
    {
        // GunShotEvent is raised again on every client re-prediction.
        if (!_timing.IsFirstTimePredicted)
            return;

        var user = args.User;
        var from = _transform.GetMapCoordinates(user);
        var to = _transform.ToMapCoordinates(args.ToCoordinates);
        if (from.MapId != to.MapId)
            return;

        var aim = to.Position - from.Position;
        if (aim.LengthSquared() < 0.0001f)
            aim = _transform.GetWorldRotation(user).ToWorldVec();

        aim = Vector2.Normalize(aim);

        // The shooter draws the wave immediately; everyone else gets it from the server.
        if (_net.IsClient)
        {
            var predicted = new CCMSonicPulsePredictedWaveEvent(user, from, aim, ent.Comp.Range, ent.Comp.ConeAngle, ent.Comp.WaveDuration);
            RaiseLocalEvent(ref predicted);
            return;
        }

        var wave = new CCMSonicPulseWaveEvent(
            GetNetEntity(user),
            GetNetCoordinates(_transform.ToCoordinates(from)),
            aim,
            ent.Comp.Range,
            ent.Comp.ConeAngle,
            ent.Comp.WaveDuration);
        RaiseNetworkEvent(wave, Filter.Pvs(user, entityManager: EntityManager));

        PulseCone(ent, user, from, aim);
    }

    public void PulseCone(Entity<CCMSonicPulseComponent> pulse, EntityUid user, MapCoordinates origin, Vector2 aim)
    {
        var comp = pulse.Comp;
        var aimAngle = aim.ToWorldAngle();
        var halfCone = comp.ConeAngle.Theta / 2;

        _targets.Clear();
        _lookup.GetEntitiesInRange(origin, comp.Range + LookupMargin, _targets);

        foreach (var target in _targets)
        {
            if (target.Owner == user || _mobState.IsDead(target))
                continue;

            // Lag-compensated position, same as regular RMC bullets.
            var targetCoords = _transform.ToMapCoordinates(_lagCompensation.GetCoordinates(target.Owner, user));
            if (targetCoords.MapId != origin.MapId)
                continue;

            var xenoClass = _carbine.GetXenoClass(target);
            var radius = xenoClass == CCMSonicXenoClass.Large ? LargeRadius : SmallRadius;

            var delta = targetCoords.Position - origin.Position;
            var distance = delta.Length();

            if (distance - radius > comp.Range)
                continue;

            // Hitting the edge of the body is enough, not just its center.
            if (distance > radius)
            {
                var tolerance = Math.Asin(Math.Clamp(radius / distance, 0f, 1f));
                var diff = Angle.ShortestDistance(aimAngle, delta.ToWorldAngle());
                if (Math.Abs(diff.Theta) > halfCone + tolerance)
                    continue;
            }

            // Walls block the wave, bodies do not.
            var targetUid = target.Owner;
            if (!_interaction.InRangeUnobstructed(origin,
                    targetCoords,
                    0f,
                    predicate: e => e == user || e == targetUid || HasComp<MobStateComponent>(e)))
            {
                continue;
            }

            _damageable.TryChangeDamage(targetUid, comp.Damage, origin: user, tool: pulse.Owner, armorPiercing: comp.ArmorPiercing);

            switch (xenoClass)
            {
                case CCMSonicXenoClass.Small:
                    _sizeStun.KnockBack(targetUid, origin, comp.KnockBackPower, comp.KnockBackPower, comp.KnockBackSpeed);
                    _slow.TrySlowdown(targetUid, comp.SmallSlowTime);
                    _slow.TrySuperSlowdown(targetUid, comp.SmallSuperSlowTime);
                    _stun.TryParalyze(targetUid, comp.SmallStunTime, true);
                    _popup.PopupEntity(Loc.GetString("ccm-silencer-pulse-knockdown"), targetUid, targetUid, PopupType.MediumCaution);
                    break;
                case CCMSonicXenoClass.Medium:
                    _slow.TrySlowdown(targetUid, comp.MediumSlowTime);
                    if (comp.MediumSuperSlowTime > TimeSpan.Zero)
                        _slow.TrySuperSlowdown(targetUid, comp.MediumSuperSlowTime);
                    _popup.PopupEntity(Loc.GetString("ccm-silencer-pulse-slow"), targetUid, targetUid, PopupType.MediumCaution);
                    break;
                case CCMSonicXenoClass.Large:
                    _slow.TrySlowdown(targetUid, comp.LargeSlowTime);
                    if (comp.LargeSuperSlowTime > TimeSpan.Zero)
                        _slow.TrySuperSlowdown(targetUid, comp.LargeSuperSlowTime);
                    _popup.PopupEntity(Loc.GetString("ccm-silencer-pulse-slow"), targetUid, targetUid, PopupType.SmallCaution);
                    break;
            }
        }
    }
}
