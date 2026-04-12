using System.Numerics;
using Content.Shared._MC.Xeno.Abilities;
using Content.Shared._RMC14.CameraShake;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Slow;
using Content.Shared._RMC14.Stamina;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared._RMC14.Power;
using Content.Shared._RMC14.Entrenching;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Coordinates.Helpers;
using Content.Shared._RMC14.Marines;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Ravage;

public sealed class MCXenoRavageSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmote = default!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcHive = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly RMCCameraShakeSystem _cameraShake = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly RMCSlowSystem _slow = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

public override void Initialize()
{
    base.Initialize();
    SubscribeLocalEvent<MCXenoRavageComponent, MCXenoRavageActionEvent>(OnUse);

}

private void OnUse(Entity<MCXenoRavageComponent> entity, ref MCXenoRavageActionEvent args)
{
    var xeno = entity.Owner;
    if (args.PlasmaCost != 0 && !_xenoPlasma.TryRemovePlasmaPopup(xeno, args.PlasmaCost))
        return;

    if (args.Handled)
        return;

    var curTime = _timing.CurTime;

    // Проверка задержки между использованными
    if (curTime < entity.Comp.NextUse)
        return;

    // Выполнение способности
    ExecuteRavage(entity);

    // Обновляем таймер повторного использования
    entity.Comp.NextUse = curTime + entity.Comp.UseDelay;

    args.Handled = true;
}

    private void ExecuteRavage(Entity<MCXenoRavageComponent> entity)
    {
        var origin = _transform.GetMapCoordinates(entity);
        var position = origin.Position;
        var localRotation = Transform(entity).LocalRotation;

        var rotation = localRotation - Angle.FromDegrees(180);
        var direction = (localRotation - Angle.FromDegrees(90)).ToVec();

        var effectCoords = Transform(entity).Coordinates.Offset(direction * 1.25f);
        ServerSpawnAttachedTo(entity.Comp.EffectEntId, effectCoords, localRotation);

        var aabb = new Box2Rotated(new Box2(position.X - 1, position.Y + 1.5f, position.X + 1, position.Y), rotation, position);

        _rmcEmote.TryEmoteWithChat(entity, entity.Comp.EffectEmote);

        foreach (var uid in _entityLookup.GetEntitiesIntersecting(origin.MapId, aabb))
        {
            if (entity.Owner == uid)
                continue;

            if (_rmcHive.FromSameHive(entity.Owner, uid))
                continue;

            if (_mobState.IsDead(uid))
                continue;

            if (!HasComp<BarricadeComponent>(uid) && !HasComp<MarineComponent>(uid) && !HasComp<XenoComponent>(uid) && !HasComp<RMCApcComponent>(uid))
                continue;

            ApplyEffect(uid, entity, direction);
        }
    }

    private void ApplyEffect(EntityUid target, EntityUid owner, Vector2 direction)
    {
        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 40);
        _damageable.TryChangeDamage(target, damage, origin: owner);

        _stun.TryStun(target, TimeSpan.FromSeconds(1f), true);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(1f), true);

        KnockBack(target, direction);

        _slow.TrySlowdown(target, TimeSpan.FromSeconds(2f));

        _cameraShake.ShakeCamera(target, 2, 1);
    }

    private void KnockBack(EntityUid target, Vector2 direction)
    {
        if (!TryComp(target, out PhysicsComponent? physics))
            return;

        _physics.SetLinearVelocity(target, Vector2.Zero, body: physics);
        _physics.SetAngularVelocity(target, 0f, body: physics);

        var power = _random.NextFloat(1.5f, 3f);

        _throwing.TryThrow(
            target,
            direction.Normalized() * power,
            6f,
            animated: false,
            playSound: false,
            compensateFriction: true
        );
    }
}