using System;
using System.Numerics;
using Content.Shared._RMC14.CameraShake;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Slow;
using Content.Shared._RMC14.Stamina;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

namespace Content.Shared._MC.Xeno.Abilities.Ravage;

public sealed class MCXenoRavageSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmote = default!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcHive = default!;
    [Dependency] private readonly RMCCameraShakeSystem _cameraShake = default!;

    // RMC замены
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly RMCSlowSystem _slow = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MCXenoRavageComponent, MCXenoRavageActionEvent>(OnUse);
    }

    private void OnUse(Entity<MCXenoRavageComponent> entity, ref MCXenoRavageActionEvent args)
    {
        var origin = _transform.GetMapCoordinates(entity);
        var position = origin.Position;

        var rotation = Transform(entity).LocalRotation;
        var direction = (rotation - Angle.FromDegrees(90)).ToVec();

        var box = new Box2Rotated(
            new Box2(position.X - 1, position.Y, position.X + 1, position.Y + 1.5f),
            rotation,
            position
        );

        _rmcEmote.TryEmoteWithChat(entity, entity.Comp.Emote);

        foreach (var uid in _entityLookup.GetEntitiesIntersecting(origin.MapId, box))
        {
            if (uid == entity.Owner)
                continue;

            if (_rmcHive.FromSameHive(entity.Owner, uid))
                continue;

            if (_mobState.IsDead(uid))
                continue;

            ApplyEffect(uid, entity.Owner, origin);
        }
    }

    private void ApplyEffect(EntityUid target, EntityUid owner, MapCoordinates origin)
    {
        //  УРОН
        var damage = GetDamage(owner);
        _damageable.TryChangeDamage(target, damage, origin: owner);

        //  KNOCKBACK (как в RMC)
        KnockBack(target, origin);

        //  STUN 
        _stun.TryStun(target, TimeSpan.FromSeconds(1f), true);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(1f), true);

        //  SLOW
        _slow.TrySlowdown(target, TimeSpan.FromSeconds(2f));

        //  CAMERA
        _cameraShake.ShakeCamera(target, 2, 1);
    }

    private void KnockBack(EntityUid target, MapCoordinates from)
    {
        if (!TryComp(target, out PhysicsComponent? physics))
            return;

        // сброс скорости
        _physics.SetLinearVelocity(target, Vector2.Zero, body: physics);
        _physics.SetAngularVelocity(target, 0f, body: physics);

        var targetPos = _transform.GetMoverCoordinates(target).Position;
        var dir = (targetPos - from.Position);

        if (dir.Length() == 0)
            return;

        dir = dir.Normalized();

        var power = _random.NextFloat(1.5f, 3f);

        _throwing.TryThrow(
            target,
            dir * power,
            6f,
            animated: false,
            playSound: false,
            compensateFriction: true
        );
    }

    private DamageSpecifier GetDamage(EntityUid owner)
    {
        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 40);
        return damage;
    }
}