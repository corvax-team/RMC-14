using System.Numerics;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce;

public sealed class MCXenoPounceSystem : EntitySystem
{
    private static readonly ProtoId<TagPrototype> AcidSprayTag = "MCAcidSpray";

    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly SharedStunSystem _stun = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly TagSystem _tag = null!;

    [Dependency] private readonly SharedXenoHiveSystem _rmcXenoHive = null!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = null!;
    [Dependency] private readonly SharedRMCActionsSystem _rmcActions = null!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<MCXenoPounceComponent, MCXenoPounceActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPounceComponent, MCXenoPounceDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<MCXenoPouncingComponent, PreventCollideEvent>(OnHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoPouncingComponent>();
        while (query.MoveNext(out var entityUid, out var pouncingComponent))
        {
            if (_timing.CurTime < pouncingComponent.End)
                continue;

            Stop(entityUid);
        }
    }

private void OnAction(Entity<MCXenoPounceComponent> entity, ref MCXenoPounceActionEvent args)
{
    var xeno = entity.Owner;
    if (args.PlasmaCost != 0 && !_xenoPlasma.TryRemovePlasmaPopup(xeno, args.PlasmaCost))
         return;

    if (args.Handled)
        return;

    if (entity.Comp.Delay == TimeSpan.Zero)
    {
        if (UseAbility(entity, args.Target.ToMap(EntityManager, _transform)))
            args.Handled = true;

        return;
    }

    // Преобразуем EntityCoordinates в MapCoordinates сразу перед DoAfter
    var targetMap = args.Target.ToMap(EntityManager, _transform);

    _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, entity, entity.Comp.Delay,
        new MCXenoPounceDoAfterEvent(targetMap), entity)
    {
        BreakOnMove = true,
        BreakOnRest = true,
    });

    args.Handled = true;
}

    private void OnDoAfter(Entity<MCXenoPounceComponent> entity, ref MCXenoPounceDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        UseAbility(entity, args.TargetCoordinates);
    }

private bool UseAbility(Entity<MCXenoPounceComponent> entity, MapCoordinates target)
{
    if (!_physicsQuery.TryGetComponent(entity, out var physicsComponent))
        return false;

    if (EnsureComp<MCXenoPouncingComponent>(entity, out var pouncingComponent))
        return false;

    var origin = _transform.GetMapCoordinates(entity);
    var direction = target.Position - origin.Position;

    if (direction == Vector2.Zero)
        return false;

    var length = direction.Length();
    var distance = Math.Clamp(length, 0.1f, entity.Comp.MaxDistance);

    var impulse = direction.Normalized() * entity.Comp.Strength * physicsComponent.Mass;

    _rmcPulling.TryStopAllPullsFromAndOn(entity);

    _physics.ApplyLinearImpulse(entity, impulse, body: physicsComponent);
    _physics.SetBodyStatus(entity, physicsComponent, BodyStatus.InAir);

    var duration = _timing.CurTime + TimeSpan.FromSeconds(distance / entity.Comp.Strength);
    pouncingComponent.End = duration;
    Dirty(entity, pouncingComponent);

    // событие для эффекта
    var ev = new MCXenoPounceStartEvent(entity, origin, target, direction.Normalized(), distance);
    RaiseLocalEvent(entity, ref ev);

    return true;
}

    private void OnHit(Entity<MCXenoPouncingComponent> entity, ref PreventCollideEvent args)
    {
        if (args.OtherFixture.CollisionLayer == (int)CollisionGroup.SlipLayer)
            return;

        if (_tag.HasTag(args.OtherEntity, AcidSprayTag))
            return;

        if (_tag.HasTag(args.OtherEntity, AcidSprayTag))
            return;

        if (entity.Comp.Hit.Contains(args.OtherEntity))
        {
            args.Cancelled = true;
            return;
        }

        entity.Comp.Hit.Add(args.OtherEntity);
        Hit(entity, args.OtherEntity);

        if (!HasComp<Shared.Mobs.Components.MobStateComponent>(args.OtherEntity))
            return;

        args.Cancelled = true;
    }

    private void Hit(Entity<MCXenoPouncingComponent> entity, EntityUid target)
    {
        if (!HasComp<Shared.Mobs.Components.MobStateComponent>(target))
        {
            Stop(entity);
            return;
        }

        if (_mobState.IsDead(target))
            return;

        if (_rmcXenoHive.FromSameHive(entity.Owner, target))
        {
            Stop(entity);
            return;
        }

        if (!TryComp<MCXenoPounceComponent>(entity, out var pounceComponent))
            return;

        if (pounceComponent.StopOnHit)
            Stop(entity);

        _stun.TrySlowdown(entity, pounceComponent.HitSelfParalyzeTime, true, 0f, 0f);
        _stun.TryParalyze(target, pounceComponent.HitKnockdownTime, true);

        if (pounceComponent.HitDamage is { } damage)
            _damageable.TryChangeDamage(target, damage, origin: entity, tool: entity);

        var first = entity.Comp.Hit.Count == 1;

        if (pounceComponent.HitSound is not null && first)
            _audio.PlayPredicted(pounceComponent.HitSound, entity, entity);
    }

    private void Stop(EntityUid entityUid)
    {
        if (!_physicsQuery.TryGetComponent(entityUid, out var physics))
            return;

        _physics.SetLinearVelocity(entityUid, Vector2.Zero, body: physics);
        _physics.SetBodyStatus(entityUid, physics, BodyStatus.OnGround);

        RemCompDeferred<MCXenoPouncingComponent>(entityUid);
    }
}