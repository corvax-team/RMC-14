using System.Numerics;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio.Systems;
using Content.Shared.Effects;
using Content.Shared.Interaction;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Xenonids.Heal;
using Content.Shared.Maps;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Standing;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Xenonids.Leap;

namespace Content.Shared._CCM.Xenonids.Abilities.Runi.Charge;

public sealed class CCMXenoChargeLineSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _colorFlash = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedRMCEmoteSystem _emote = default!;
    [Dependency] private readonly SharedXenoHealSystem _xenoHeal = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly RMCSizeStunSystem _size = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CCMXenoChargeLineComponent, CCMXenoChargeLineActionEvent>(OnUse);
        SubscribeLocalEvent<CCMXenoChargeLineComponent, CCMXenoChargeLineDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<CCMXenoChargeLineActiveComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<CCMXenoChargeLineActiveComponent, MoveEvent>(OnMove);
    }

    private bool IsValidChargeTarget(EntityUid attacker, EntityUid target)
    {
        if (target == attacker)
            return false;

        if (!HasComp<MobStateComponent>(target))
            return false;

        if (_mobState.IsDead(target))
            return false;

        if (_hive.FromSameHive(attacker, target))
            return false;

        if (_standing.IsDown(target))
            return false;

        if (HasComp<LeapIncapacitatedComponent>(target))
            return false;

        if (_size.TryGetSize(target, out var size))
        {
            if (size >= RMCSizes.Big)
                return false;

            if (size == RMCSizes.VerySmallXeno)
                return false;
        }

        // защита от блоков / баррикад / спец-защит
        var attempt = new XenoLeapHitAttempt(attacker);
        RaiseLocalEvent(target, ref attempt);

        if (attempt.Cancelled)
            return false;

        return true;
    }

    private void OnUse(Entity<CCMXenoChargeLineComponent> ent, ref CCMXenoChargeLineActionEvent args)
    {
        var ev = new CCMXenoChargeLineDoAfterEvent();

        var doAfter = new DoAfterArgs(EntityManager, ent, ent.Comp.ActivationDelay, ev, ent)
        {
            BreakOnMove = false
        };

        var xeno = ent.Owner;

        if (args.PlasmaCost != 0 && !_xenoPlasma.TryRemovePlasmaPopup(xeno, args.PlasmaCost))
            return;

        _doAfter.TryStartDoAfter(doAfter);
        args.Handled = true;
    }

    private void OnDoAfter(Entity<CCMXenoChargeLineComponent> ent, ref CCMXenoChargeLineDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        var active = new CCMXenoChargeLineActiveComponent
        {
            SpeedMultiplier = ent.Comp.SpeedMultiplier,
            Damage = ent.Comp.Damage,
            MaxTiles = ent.Comp.MaxTiles,
            HitRadius = ent.Comp.HitRadius,
            HealPerHit = ent.Comp.HealPerHit,
            HitEntities = new HashSet<EntityUid>()
        };

        AddComp(ent, active, true);

        if (ent.Comp.Emote != null)
            _emote.TryEmoteWithChat(ent, ent.Comp.Emote, cooldown: ent.Comp.EmoteDelay);
    }

    private void OnSpeed(Entity<CCMXenoChargeLineActiveComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.SpeedMultiplier);
    }

    private void OnMove(Entity<CCMXenoChargeLineActiveComponent> ent, ref MoveEvent args)
    {
        var xform = Transform(ent);

        ent.Comp.TilesTraveled++;

        if (ent.Comp.TilesTraveled >= ent.Comp.MaxTiles)
        {
            RemCompDeferred<CCMXenoChargeLineActiveComponent>(ent);
            return;
        }

        var coords = xform.Coordinates;
        var forward = xform.LocalRotation.ToWorldVec();

        var hitCount = 0;
        var baseComp = CompOrNull<CCMXenoChargeLineComponent>(ent.Owner);

        foreach (var target in _lookup.GetEntitiesInRange(coords, ent.Comp.HitRadius))
        {
            if (target == ent.Owner)
                continue;

            if (!IsValidChargeTarget(ent.Owner, target))
                continue;

            var targetCoords = Transform(target).Coordinates;
            var dir = (targetCoords.Position - coords.Position).Normalized();

            if (Vector2.Dot(dir, forward) < 0.3f)
                continue;

            if (!ent.Comp.HitEntities.Add(target))
                continue;

            _damageable.TryChangeDamage(target, ent.Comp.Damage, tool: ent);
            hitCount++;

            if (baseComp?.AttackEffect != null)
                SpawnAttachedTo(baseComp.AttackEffect, targetCoords);

            if (!_net.IsClient)
            {
                var filter = Filter.Pvs(target, entityManager: EntityManager)
                    .RemoveWhereAttachedEntity(o => o == ent.Owner);

                _colorFlash.RaiseEffect(Color.Red, new List<EntityUid> { target }, filter);
            }
        }

        if (hitCount > 0 && !ent.Comp.HealTriggered)
        {
            ent.Comp.HealTriggered = true;

            var healAmount = ent.Comp.HealPerHit * hitCount;

            var heal = new DamageSpecifier();
            heal.DamageDict["Blunt"] = -healAmount;
            heal.DamageDict["Slash"] = -healAmount;
            heal.DamageDict["Piercing"] = -healAmount;
            heal.DamageDict["Heat"] = -healAmount;
            heal.DamageDict["Cold"] = -healAmount;
            heal.DamageDict["Shock"] = -healAmount;

            _damageable.TryChangeDamage(ent, heal);

            if (baseComp?.HealEffect != null)
            {
                var effect = Spawn(baseComp.HealEffect, Transform(ent).Coordinates);
                _transform.SetParent(effect, ent);
            }

            _xenoHeal.CreateHealStacks(
                ent,
                healAmount,
                TimeSpan.FromSeconds(0.5),
                1,
                TimeSpan.FromSeconds(0.5)
            );

            if (baseComp?.HitSound != null)
                _audio.PlayPredicted(baseComp.HitSound, ent, ent);
        }
    }
}
