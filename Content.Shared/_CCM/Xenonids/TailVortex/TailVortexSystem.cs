using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Effects;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Shared._CCM.Xenonids.TailVortex;

public sealed class TailVortexSystem : EntitySystem
{
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly XenoSystem _xeno = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _colorFlash = default!;
    [Dependency] private readonly RMCSizeStunSystem _rmcSizeStun = default!;
    [Dependency] private readonly RotateToFaceSystem _rotateTo = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TailVortexComponent, TailVortexActionEvent>(OnXenoTailVortexAction);
        SubscribeLocalEvent<TailVortexComponent, TailVortexDoAfterEvent>(OnXenoTailVortexDoAfter);
    }

    private void OnXenoTailVortexAction(Entity<TailVortexComponent> xeno, ref TailVortexActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        args.Handled = true;

        AddComp<TailVortexRotationComponent>(xeno);

        var ev = new TailVortexDoAfterEvent(xeno.Comp.GuaranteedQuantity + xeno.Comp.StacksCount);
        xeno.Comp.StacksCount = 0;

        var doAfter = new DoAfterArgs(EntityManager, xeno, xeno.Comp.DoAfterTime, ev, xeno)
        {
            BreakOnRest = true,
            DamageThreshold = 175,
            BreakOnDamage = true
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnXenoTailVortexDoAfter(Entity<TailVortexComponent> xeno, ref TailVortexDoAfterEvent args)
    {
        if (args.Cancelled)
        {
            RemComp<TailVortexRotationComponent>(xeno);
            return;
        }

        var comp = EnsureComp<TailVortexRotationComponent>(xeno);
        comp.Angle = comp.Angle == null
            ? _transform.GetWorldRotation(xeno) + Angle.FromDegrees(90)
            : comp.Angle + Angle.FromDegrees(90);

        var coordinates = _transform.GetMapCoordinates(xeno);
        foreach (var target in _entityLookup.GetEntitiesInRange<MobStateComponent>(coordinates, xeno.Comp.Range))
        {
            if (!_xeno.CanAbilityAttackTarget(xeno, target))
                continue;

            if (!_interaction.InRangeUnobstructed(xeno.Owner, target.Owner, xeno.Comp.Range))
                continue;

            var damage = _damageable.TryChangeDamage(target, _xeno.TryApplyXenoSlashDamageMultiplier(target, xeno.Comp.Damage), origin: xeno, tool: xeno);
            if (damage?.GetTotal() > FixedPoint2.Zero)
            {
                var filter = Filter.Pvs(target, entityManager: EntityManager).RemoveWhereAttachedEntity(o => o == xeno.Owner);
                _colorFlash.RaiseEffect(Color.Red, new List<EntityUid> { target }, filter);
            }

            _rmcSizeStun.KnockBack(target, coordinates, xeno.Comp.Power, xeno.Comp.Power);
        }

        _audio.PlayPredicted(xeno.Comp.Sound, xeno, xeno);

        args.TurnsQuantity--;
        if (args.TurnsQuantity > 0)
        {
            var doAfter = new DoAfterArgs(EntityManager, xeno, xeno.Comp.VortexDelay, args, xeno)
            {
                Hidden = true,
                BreakOnRest = true,
                DamageThreshold = 100,
                BreakOnDamage = true
            };

            _doAfter.TryStartDoAfter(doAfter);
        }
        else
        {
            if (comp.Angle is { } angle)
                _rotateTo.TryFaceAngle(xeno, angle);

            RemComp(xeno, comp);
        }
    }

    public void AddStacks(Entity<TailVortexComponent?> xeno, int amount)
    {
        if (!Resolve(xeno, ref xeno.Comp, false))
            return;

        xeno.Comp.StacksCount = Math.Min(xeno.Comp.StacksCount + amount, xeno.Comp.MaxStacks);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<TailVortexRotationComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Angle is { } angle)
                _rotateTo.TryFaceAngle(uid, angle);
        }
    }
}
