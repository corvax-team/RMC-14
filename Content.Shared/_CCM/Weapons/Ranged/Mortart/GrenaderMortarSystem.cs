using System.Numerics;
using Content.Shared._RMC14.Areas;
using Content.Shared._RMC14.Weapons.Ranged;
using Content.Shared._RMC14.Weapons.Common;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Map;

namespace Content.Shared._CCM.Weapons.Ranged.Mortar;

public sealed class GrenaderMortarSystem : EntitySystem
{
    [Dependency] private readonly AreaSystem _area = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedProjectileSystem _projectile = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private bool _isPerformingMortarShot = false;

    public override void Initialize()
    {
        SubscribeLocalEvent<MortarModeComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MortarModeComponent, UniqueActionEvent>(OnUniqueAction);
        SubscribeLocalEvent<MortarModeComponent, AttemptShootEvent>(OnAttemptShoot);
        SubscribeLocalEvent<MortarModeComponent, MortarDoAfterEvent>(OnMortarDoAfter);
        SubscribeLocalEvent<MortarModeComponent, AmmoShotEvent>(OnAmmoShot);
    }

    private void OnExamined(Entity<MortarModeComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(ent.Comp.Examine), 1);
    }

    private void OnUniqueAction(Entity<MortarModeComponent> ent, ref UniqueActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        ent.Comp.Activated = !ent.Comp.Activated;
        Dirty(ent);

        _audio.PlayPredicted(ent.Comp.ToggleSound, ent, args.UserUid);

        var message = ent.Comp.Activated ? "ccm-gun-mode-mortar" : "ccm-gun-mode-default";
        _popup.PopupClient(Loc.GetString(message), ent, args.UserUid, PopupType.Small);
    }

    private void OnAttemptShoot(Entity<MortarModeComponent> ent, ref AttemptShootEvent args)
    {
        if (!ent.Comp.Activated || _isPerformingMortarShot)
            return;

        if (args.ToCoordinates == null)
        {
            args.Cancelled = true;
            return;
        }

        var userMap = _transform.GetMapCoordinates(args.User);
        var targetMap = _transform.ToMapCoordinates(args.ToCoordinates.Value);
        if (userMap.MapId != targetMap.MapId)
        {
            args.Cancelled = true;
            return;
        }
        var distance = (targetMap.Position - userMap.Position).Length();
        if (distance > ent.Comp.MaxRange)
        {
            args.Cancelled = true;
            _popup.PopupClient(Loc.GetString("ccm-mortar-too-far"), args.User, args.User, PopupType.MediumCaution);
            return;
        }

        if (!_area.CanMortarPlacement(args.User.ToCoordinates()))
        {
            args.Cancelled = true;
            _popup.PopupClient(Loc.GetString("ccm-mortar-covered"), args.User, args.User, PopupType.MediumCaution);
            return;
        }

        if (!_area.CanMortarFire(_transform.ToCoordinates(targetMap)))
        {
            args.Cancelled = true;
            _popup.PopupClient(Loc.GetString("ccm-mortar-covered"), args.User, args.User, PopupType.MediumCaution);
            return;
        }

        _popup.PopupClient(Loc.GetString("ccm-mortar-prepare"), args.User, args.User, PopupType.Medium);

        var user = args.User;
        var gunNet = GetNetEntity(ent);
        var gridCoords = _transform.ToCoordinates(targetMap);
        var targetNetCoords = GetNetCoordinates(gridCoords);

        var doAfterArgs = new DoAfterArgs(EntityManager, user, ent.Comp.DoAfterDuration,
            new MortarDoAfterEvent(gunNet, targetNetCoords), ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnHandChange = true,
            NeedHand = true
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Cancelled = true;
    }

    private void OnMortarDoAfter(Entity<MortarModeComponent> ent, ref MortarDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;
        if (!ent.Comp.Activated)
            return;

        var user = args.User;
        if (!_hands.IsHolding(user, ent))
            return;
        if (!TryComp<GunComponent>(ent, out var gun))
            return;

        var targetCoords = GetCoordinates(args.TargetCoords);
        ent.Comp.TargetCoordinates = targetCoords;
        Dirty(ent);

        _popup.PopupClient(Loc.GetString("ccm-mortar-fired"), user, user, PopupType.Medium);

        _isPerformingMortarShot = true;
        try
        {
            _gun.AttemptShoot((ent, gun), user, targetCoords);
        }
        finally
        {
            _isPerformingMortarShot = false;
        }
    }

    private void OnAmmoShot(Entity<MortarModeComponent> ent, ref AmmoShotEvent args)
    {
        if (ent.Comp.TargetCoordinates == null)
            return;

        var targetCoords = ent.Comp.TargetCoordinates.Value;
        ent.Comp.TargetCoordinates = null;

        var targetMap = _transform.ToMapCoordinates(targetCoords);

        foreach (var projectile in args.FiredProjectiles)
        {
            var scatter = ent.Comp.Scatter;
            var offset = new Vector2(
                _random.NextFloat(-scatter, scatter),
                _random.NextFloat(-scatter, scatter)
            );
            var finalPos = targetMap.Position + offset;

            var fixedDist = EnsureComp<ProjectileFixedDistanceComponent>(projectile);
            fixedDist.ArcProj = false;
            fixedDist.FlyEndTime = _timing.CurTime;

            _transform.SetMapCoordinates(projectile, new MapCoordinates(finalPos, targetMap.MapId));

            if (TryComp<PhysicsComponent>(projectile, out var physics))
            {
                _physics.SetLinearVelocity(projectile, Vector2.Zero, body: physics);
                _physics.SetAngularVelocity(projectile, 0f, body: physics);
                _physics.SetBodyStatus(projectile, physics, BodyStatus.OnGround);
            }
        }
    }
}
