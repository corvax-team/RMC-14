/*
Copyright (c) 2025 Inconnu (Discord: Inconnu1337).
All Rights Reserved.

An exclusive license is granted to Denlero (Discord: Denlero)
for the Corvax Colonial Marines project, with full rights
to use, modify, distribute, and sublicense.
Third-party use requires Denlero's consent.
*/
using Content.Shared._CCM.Attachables;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Shared.Weapons.Ranged.Systems;

public sealed class GunArcRestrictionSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly VehicleAttachableHolderSystem _holder = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GunArcRestrictionComponent, AttemptShootEvent>(OnAttemptShoot);
        SubscribeLocalEvent<GunArcRestrictionComponent, ExaminedEvent>(OnExamined);
    }

    private void OnAttemptShoot(Entity<GunArcRestrictionComponent> ent, ref AttemptShootEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<GunComponent>(ent, out var gun) || gun.ShootCoordinates == null)
            return;

        var shooterEntity = args.User;
        if (_holder.TryGetHolder(ent.Owner, out var holder))
            shooterEntity = holder.Value;

        var shooterPos = _transform.GetWorldPosition(shooterEntity);
        var targetPos = _transform.ToMapCoordinates(gun.ShootCoordinates.Value).Position;

        var shooterRot = _transform.GetWorldRotation(shooterEntity);
        var directionToTarget = (targetPos - shooterPos).Normalized();
        var targetAngle = directionToTarget.ToAngle();

        var allowedCenter = shooterRot + ent.Comp.ArcDirection;

        var diff = GetAngleDifference(allowedCenter, targetAngle);

        if (Math.Abs(diff.Theta) > ent.Comp.MaxAngleDeviation.Theta)
        {
            args.Cancelled = true;
            args.ResetCooldown = true;
            if (_holder.TryGetAttachableUser(ent.Owner, out var pilot) && pilot != null)
            {
                _popup.PopupPredictedCursor(Loc.GetString($"{ent.Comp.RestrictionMessage}"), pilot.Value, PopupType.SmallCaution);
            }
        }
    }

    private void OnExamined(EntityUid uid, GunArcRestrictionComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("ccm-vehicle-gun-arc-restriction-examine",
            ("degrees", Math.Round(component.MaxAngleDeviation.Degrees * 2, 1))));
    }

    private static Angle GetAngleDifference(Angle from, Angle to)
    {
        var diff = to - from;

        while (diff.Theta > Math.PI)
            diff -= 2 * Math.PI;

        while (diff.Theta < -Math.PI)
            diff += 2 * Math.PI;

        return diff;
    }
}
