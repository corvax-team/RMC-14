using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._CCM.Xenonids.TailWhirlwind;

public sealed class TailWhirlwindSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedRMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly RotateToFaceSystem _rotateTo = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly XenoSystem _xeno = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _colorFlash = default!;
    [Dependency] private readonly RMCSizeStunSystem _size = default!;
    [Dependency] private readonly SharedInteractionSystem _interact = default!;

    private readonly HashSet<Entity<MobStateComponent>> _hits = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<TailWhirlwindComponent, TailWhirlwindActionEvent>(OnXenoTailWhirlwindAction);
    }

    private void OnXenoTailWhirlwindAction(Entity<TailWhirlwindComponent> xeno, ref TailWhirlwindActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_rmcActions.TryUseAction(args))
            return;

        xeno.Comp.EndAt = _timing.CurTime + xeno.Comp.Duration;

        var whirlwinding = EnsureComp<TailWhirlwindingComponent>(xeno);
        UpdateTail((xeno.Owner, xeno.Comp, whirlwinding));

        args.Handled = true;
    }

    private void UpdateTail(Entity<TailWhirlwindComponent, TailWhirlwindingComponent> xeno)
    {
        xeno.Comp1.NextUpdateAt = _timing.CurTime + xeno.Comp1.UpdateDuration;

        xeno.Comp2.LastAngle = _transform.GetWorldRotation(xeno) + Angle.FromDegrees(90);
        _rotateTo.TryFaceAngle(xeno, xeno.Comp2.LastAngle);

        var coordinates = _transform.GetMapCoordinates(xeno);

        _hits.Clear();
        _entityLookup.GetEntitiesInRange(coordinates, xeno.Comp1.Range, _hits);
        foreach (var target in _hits)
        {
            if (!_xeno.CanAbilityAttackTarget(xeno, target))
                continue;

            if (!_interact.InRangeUnobstructed(xeno.Owner, target.Owner, xeno.Comp1.Range))
                continue;

            if (xeno.Comp1.Damage is { } damage && damage.GetTotal() != FixedPoint2.Zero)
            {
                _damageable.TryChangeDamage(target, _xeno.TryApplyXenoSlashDamageMultiplier(target, damage), origin: xeno, tool: xeno);
                var filter = Filter.Pvs(target, entityManager: EntityManager);
                _colorFlash.RaiseEffect(Color.Red, new List<EntityUid> { target }, filter);
            }

            _size.KnockBack(target, coordinates, xeno.Comp1.ThrowDistance, xeno.Comp1.ThrowDistance);
        }

        _audio.PlayPredicted(xeno.Comp1.Sound, xeno, xeno);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<TailWhirlwindComponent, TailWhirlwindingComponent>();
        while (query.MoveNext(out var xeno, out var tailWhirlwind, out var tailWhirlwinding))
        {
            if (tailWhirlwind.EndAt < _timing.CurTime)
            {
                RemComp<TailWhirlwindingComponent>(xeno);
                continue;
            }

            if (tailWhirlwind.NextUpdateAt > _timing.CurTime)
                continue;

            UpdateTail((xeno, tailWhirlwind, tailWhirlwinding));
        }
    }

    public override void FrameUpdate(float frameTime)
    {
        var query = EntityQueryEnumerator<TailWhirlwindingComponent>();
        while (query.MoveNext(out var xeno, out var tailWhirlwinding))
        {
            _rotateTo.TryFaceAngle(xeno, tailWhirlwinding.LastAngle);
        }
    }
}
