using System.Numerics;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio.Systems;

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

    public override void Initialize()
    {
        SubscribeLocalEvent<CCMXenoChargeLineComponent, CCMXenoChargeLineActionEvent>(OnUse);
        SubscribeLocalEvent<CCMXenoChargeLineComponent, CCMXenoChargeLineDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<CCMXenoChargeLineActiveComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<CCMXenoChargeLineActiveComponent, MoveEvent>(OnMove);
    }

    private void OnUse(Entity<CCMXenoChargeLineComponent> ent, ref CCMXenoChargeLineActionEvent args)
    {
        var ev = new CCMXenoChargeLineDoAfterEvent();

        var doAfter = new DoAfterArgs(EntityManager, ent, ent.Comp.ActivationDelay, ev, ent)
        {
            BreakOnMove = false
        };

        var xeno = entity.Owner;
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

        foreach (var target in _lookup.GetEntitiesInRange(coords, ent.Comp.HitRadius))
        {
            if (target == ent.Owner)
                continue;

            if (!HasComp<MobStateComponent>(target) || _mobState.IsDead(target))
                continue;

            // направление к цели
            var targetCoords = Transform(target).Coordinates;
            var dir = (targetCoords.Position - coords.Position).Normalized();

            // только вперед
            if (Vector2.Dot(dir, forward) < 0.3f)
                continue;

            // уже били
            if (!ent.Comp.HitEntities.Add(target))
                continue;

            _damageable.TryChangeDamage(target, ent.Comp.Damage, tool: ent);
            hitCount++;
        }

        if (hitCount > 0)
        {
            var healAmount = ent.Comp.HealPerHit * hitCount;

            var heal = new DamageSpecifier();
            heal.DamageDict["Blunt"] = -healAmount;
            heal.DamageDict["Slash"] = -healAmount;
            heal.DamageDict["Piercing"] = -healAmount;
            heal.DamageDict["Heat"] = -healAmount;
            heal.DamageDict["Cold"] = -healAmount;
            heal.DamageDict["Shock"] = -healAmount;

            _damageable.TryChangeDamage(ent, heal);

            var baseComp = CompOrNull<CCMXenoChargeLineComponent>(ent.Owner);
            if (baseComp?.HitSound != null)
                _audio.PlayPredicted(baseComp.HitSound, ent, ent);
        }
    }
}
