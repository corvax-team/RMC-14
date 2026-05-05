using System.Numerics;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared._RMC14.Xenonids.Weeds;
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
    [Dependency] private readonly EntityLookupSystem _lookup = null!;

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

        var query = EntityQueryEnumerator<MCXenoPouncingComponent, PhysicsComponent>();

        while (query.MoveNext(out var uid, out var pounce, out var physics))
        {
            if (_timing.CurTime >= pounce.End)
            {
                Stop(uid);
                continue;
            }

            if (!pounce.ZigZag)
                continue;

            if (pounce.Direction == Vector2.Zero)
                continue;


            var forward = pounce.Direction;

            var perp = new Vector2(-forward.Y, forward.X);

            var time = (float)(_timing.CurTime - pounce.StartTime).TotalSeconds;

            var offset = MathF.Sin(time * pounce.ZigZagFrequency) * pounce.ZigZagAmplitude;

            var baseSpeed = pounce.Strength;

            var baseVelocity = forward * baseSpeed;

            var sideVelocity = perp * offset * baseSpeed * 3.5f;

            var finalVelocity = baseVelocity + sideVelocity;

            _physics.SetLinearVelocity(uid, finalVelocity, body: physics);
        }
    }

    private void OnAction(Entity<MCXenoPounceComponent> entity, ref MCXenoPounceActionEvent args)
    {
        var xeno = entity.Owner;

        if (args.PlasmaCost != 0 && !_xenoPlasma.TryRemovePlasmaPopup(xeno, args.PlasmaCost))
            return;

        if (args.Handled)
            return;

        var targetMap = args.Target.ToMap(EntityManager, _transform);

        if (entity.Comp.Delay == TimeSpan.Zero)
        {
            if (UseAbility(entity, targetMap))
                args.Handled = true;

            return;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(
            EntityManager,
            entity,
            entity.Comp.Delay,
            new MCXenoPounceDoAfterEvent(targetMap),
            entity)
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

        if (EnsureComp<MCXenoPouncingComponent>(entity, out var pouncing))
            return false;

        var origin = _transform.GetMapCoordinates(entity);

        var raw = target.Position - origin.Position;

        if (raw == Vector2.Zero)
            return false;

        var length = raw.Length();
        var distance = Math.Clamp(length, 0.1f, entity.Comp.MaxDistance);

        var direction = Vector2.Normalize(raw);

        var impulse = direction * entity.Comp.Strength * physicsComponent.Mass;

        _rmcPulling.TryStopAllPullsFromAndOn(entity);

        pouncing.Direction = direction;
        pouncing.StartTime = _timing.CurTime;
        Dirty(entity, pouncing);

        _physics.ApplyLinearImpulse(entity, impulse, body: physicsComponent);
        _physics.SetBodyStatus(entity, physicsComponent, BodyStatus.InAir);

        pouncing.End = _timing.CurTime + TimeSpan.FromSeconds(distance / entity.Comp.Strength);

        var ev = new MCXenoPounceStartEvent(
            entity,
            origin,
            target,
            direction,
            distance);

        RaiseLocalEvent(entity, ref ev);

        return true;
    }

    private void OnHit(Entity<MCXenoPouncingComponent> entity, ref PreventCollideEvent args)
    {
        if (args.OtherFixture.CollisionLayer == (int)CollisionGroup.SlipLayer)
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

        if (HasComp<Shared.Mobs.Components.MobStateComponent>(args.OtherEntity))
            args.Cancelled = true;
    }

    private void Hit(Entity<MCXenoPouncingComponent> entity, EntityUid target)
    {
        if (!HasComp<Shared.Mobs.Components.MobStateComponent>(target))
        {
            if (!IsOnWeeds(entity))
                Stop(entity);

            return;
        }

        if (_mobState.IsDead(target))
            return;

        if (_rmcXenoHive.FromSameHive(entity.Owner, target))
        {
            if (!IsOnWeeds(entity))
                Stop(entity);

            return;
        }

        if (!TryComp<MCXenoPounceComponent>(entity, out var config))
            return;

        if (config.StopOnHit && !IsOnWeeds(entity))
        {
            Stop(entity);
            return;
        }

        _stun.TrySlowdown(entity, config.HitSelfParalyzeTime, true, 0f, 0f);
        _stun.TryParalyze(target, config.HitKnockdownTime, true);

        if (config.HitDamage is { } damage)
            _damageable.TryChangeDamage(target, damage, origin: entity, tool: entity);

        if (config.HitSound != null && entity.Comp.Hit.Count == 1)
            _audio.PlayPredicted(config.HitSound, entity, entity);
    }

    private void Stop(EntityUid entityUid)
    {
        if (!_physicsQuery.TryGetComponent(entityUid, out var physics))
            return;

        _physics.SetLinearVelocity(entityUid, Vector2.Zero);
        _physics.SetBodyStatus(entityUid, physics, BodyStatus.OnGround);

        if (TryComp<MCXenoPouncingComponent>(entityUid, out var comp))
            comp.Hit.Clear();

        RemCompDeferred<MCXenoPouncingComponent>(entityUid);
    }

    private bool IsOnWeeds(EntityUid uid)
    {
        var coords = Transform(uid).Coordinates;

        var entities = _lookup.GetEntitiesIntersecting(coords);

        foreach (var ent in entities)
        {
            if (HasComp<XenoWeedsComponent>(ent))
                return true;
        }

        return false;
    }
}
