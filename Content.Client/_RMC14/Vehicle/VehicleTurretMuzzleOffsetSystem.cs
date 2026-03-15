using System.Numerics;
using Content.Shared._RMC14.Vehicle;
using Content.Shared._RMC14.Weapons.Ranged;
using Content.Shared.Vehicle.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.Graphics;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Client._RMC14.Vehicle;

public sealed class VehicleTurretMuzzleOffsetSystem : EntitySystem
{
    private const float PixelsPerMeter = 32f;

    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VehicleWeaponsOperatorComponent, BeforeAttemptShootEvent>(OnBeforeAttemptShoot);
        SubscribeLocalEvent<VehicleTurretComponent, RMCBeforeMuzzleFlashEvent>(
            OnBeforeMuzzleFlash,
            after: new[] { typeof(GunMuzzleOffsetSystem) });
    }

    public bool TryGetRenderedGunOrigin(EntityUid weaponUid, EntityCoordinates? target, out EntityCoordinates origin)
    {
        origin = default;

        if (!TryComp(weaponUid, out VehicleTurretComponent? turret))
            return false;

        if (!TryGetVisualTurretOrigin(weaponUid, turret, out origin))
            return false;

        if (TryComp(weaponUid, out GunMuzzleOffsetComponent? gunMuzzle))
            origin = ApplyGunMuzzleOffset(weaponUid, gunMuzzle, target, origin);

        if (TryComp(weaponUid, out VehicleTurretMuzzleComponent? turretMuzzle))
            origin = ApplyTurretMuzzleOffset(weaponUid, turretMuzzle, origin);

        return true;
    }

    private void OnBeforeAttemptShoot(Entity<VehicleWeaponsOperatorComponent> ent, ref BeforeAttemptShootEvent args)
    {
        if (ent.Comp.SelectedWeapon is not { } selectedWeapon)
            return;

        if (!TryGetRenderedGunOrigin(selectedWeapon, null, out var renderedOrigin))
            return;

        args.Origin = renderedOrigin;
        args.Handled = true;
    }

    private void OnBeforeMuzzleFlash(Entity<VehicleTurretComponent> ent, ref RMCBeforeMuzzleFlashEvent args)
    {
        if (!TryGetRenderedGunOrigin(ent.Owner, null, out var renderedOrigin))
            return;

        var renderedMap = _transform.ToMapCoordinates(renderedOrigin);
        var weaponMap = _transform.GetMapCoordinates(args.Weapon);
        if (renderedMap.MapId != weaponMap.MapId)
            return;

        var weaponRotation = _transform.GetWorldRotation(args.Weapon);
        var worldOffset = renderedMap.Position - weaponMap.Position;
        args.Offset = (-weaponRotation).RotateVec(worldOffset);
    }

    private bool TryGetVisualTurretOrigin(EntityUid turretUid, VehicleTurretComponent turret, out EntityCoordinates origin)
    {
        origin = default;

        if (!TryGetVehicle(turretUid, out var vehicle))
            return false;

        TryGetAnchorTurret(turretUid, turret, out var anchorUid, out var anchorTurret);

        var vehicleRot = _transform.GetWorldRotation(vehicle);
        var eyeRot = _eye.CurrentEye.Rotation;
        var baseFacingAngle = GetVehicleFacingAngle(vehicle, vehicleRot);
        var anchorFacingAngle = GetRenderFacing(anchorTurret, anchorTurret, vehicleRot, baseFacingAngle, eyeRot);
        var anchorPixelOffset = GetPixelOffset(anchorTurret, anchorFacingAngle) / PixelsPerMeter;
        var anchorLocalOffset = GetVehicleLocalOffset(anchorTurret, anchorPixelOffset, vehicleRot, eyeRot);
        var localOffset = anchorLocalOffset;

        if (anchorUid == turretUid)
        {
            origin = new EntityCoordinates(vehicle, localOffset);
            return true;
        }

        var targetLocalRotation = anchorTurret.RotateToCursor ? anchorTurret.WorldRotation : Angle.Zero;
        var turretFacingAngle = GetRenderFacing(turret, anchorTurret, vehicleRot, baseFacingAngle, eyeRot);
        var worldOffset = GetPixelOffset(turret, turretFacingAngle) / PixelsPerMeter;
        Vector2 relativeAnchorOffset;

        if (turret.OffsetRotatesWithTurret)
        {
            if (turret.UseDirectionalOffsets)
            {
                var dir = GetDirectionalDir(turretFacingAngle);
                var snappedAngle = GetDirectionalAngle(dir);
                relativeAnchorOffset = (targetLocalRotation - snappedAngle).RotateVec(worldOffset);
            }
            else
            {
                relativeAnchorOffset = targetLocalRotation.RotateVec(worldOffset);
            }
        }
        else
        {
            var turretLocalOffset = GetVehicleLocalOffset(turret, worldOffset, vehicleRot, eyeRot);
            relativeAnchorOffset = (-targetLocalRotation).RotateVec(turretLocalOffset);
        }

        origin = new EntityCoordinates(vehicle, localOffset + relativeAnchorOffset);
        return true;
    }

    private EntityCoordinates ApplyGunMuzzleOffset(
        EntityUid weaponUid,
        GunMuzzleOffsetComponent muzzle,
        EntityCoordinates? target,
        EntityCoordinates origin)
    {
        if (muzzle.Offset == Vector2.Zero &&
            muzzle.MuzzleOffset == Vector2.Zero &&
            !muzzle.UseDirectionalOffsets)
        {
            return origin;
        }

        var baseRotation = GetBaseRotation(weaponUid, muzzle.AngleOffset);
        var offset = GetGunMuzzleOffset(weaponUid, muzzle, baseRotation);
        origin = muzzle.RotateDirectionalOffsets || !muzzle.UseDirectionalOffsets
            ? origin.Offset(baseRotation.RotateVec(offset))
            : origin.Offset(offset);

        if (muzzle.MuzzleOffset == Vector2.Zero)
            return origin;

        var muzzleRotation = baseRotation;
        var aimTarget = target;
        if (aimTarget == null &&
            muzzle.UseAimDirection &&
            TryComp(weaponUid, out GunComponent? gun) &&
            gun.ShootCoordinates is { } shootCoordinates)
        {
            aimTarget = shootCoordinates;
        }

        if (muzzle.UseAimDirection && aimTarget != null)
        {
            var pivotMap = _transform.ToMapCoordinates(origin);
            var targetMap = _transform.ToMapCoordinates(aimTarget.Value);
            if (pivotMap.MapId == targetMap.MapId)
            {
                var direction = targetMap.Position - pivotMap.Position;
                if (direction.LengthSquared() > 0.0001f)
                    muzzleRotation = direction.ToWorldAngle() + muzzle.AngleOffset;
            }
        }

        return origin.Offset(muzzleRotation.RotateVec(muzzle.MuzzleOffset));
    }

    private EntityCoordinates ApplyTurretMuzzleOffset(
        EntityUid weaponUid,
        VehicleTurretMuzzleComponent muzzle,
        EntityCoordinates origin)
    {
        var baseRotation = _transform.GetWorldRotation(weaponUid);
        var eyeRotation = _eye.CurrentEye.Rotation;
        var useRight = muzzle.Alternate && muzzle.UseRightNext;
        var currentOffset = GetTurretMuzzleWorldOffset(muzzle, baseRotation, Angle.Zero, useRight);
        var desiredOffset = GetTurretMuzzleWorldOffset(muzzle, baseRotation, eyeRotation, useRight);
        return origin.Offset(desiredOffset - currentOffset);
    }

    private Vector2 GetGunMuzzleOffset(EntityUid weaponUid, GunMuzzleOffsetComponent muzzle, Angle baseRotation)
    {
        if (!muzzle.UseDirectionalOffsets)
            return muzzle.Offset;

        var facing = baseRotation;
        if (TryComp(weaponUid, out VehicleTurretComponent? _) &&
            TryGetVehicle(weaponUid, out var vehicle))
        {
            var vehicleRot = _transform.GetWorldRotation(vehicle);
            facing = GetVehicleFacingAngle(vehicle, vehicleRot);
        }

        var worldOffset = VehicleTurretDirectionHelpers.GetRenderAlignedCardinalDir(facing.Reduced()) switch
        {
            Direction.North => muzzle.OffsetNorth,
            Direction.East => muzzle.OffsetEast,
            Direction.South => muzzle.OffsetSouth,
            Direction.West => muzzle.OffsetWest,
            _ => muzzle.Offset,
        };

        return worldOffset;
    }

    private Angle GetBaseRotation(EntityUid baseUid, Angle angleOffset)
    {
        var rotation = _transform.GetWorldRotation(baseUid);
        if (TryComp(baseUid, out GridVehicleMoverComponent? mover) && mover.CurrentDirection != Vector2i.Zero)
            rotation = new Vector2(mover.CurrentDirection.X, mover.CurrentDirection.Y).ToWorldAngle();

        return rotation + angleOffset;
    }

    private static Vector2 GetTurretMuzzleWorldOffset(
        VehicleTurretMuzzleComponent muzzle,
        Angle baseRotation,
        Angle eyeRotation,
        bool useRight)
    {
        var worldOffset = VehicleTurretDirectionHelpers.GetRenderAlignedCardinalDir(baseRotation.Reduced()) switch
        {
            Direction.North => useRight ? muzzle.OffsetRightNorth : muzzle.OffsetLeftNorth,
            Direction.East => useRight ? muzzle.OffsetRightEast : muzzle.OffsetLeftEast,
            Direction.South => useRight ? muzzle.OffsetRightSouth : muzzle.OffsetLeftSouth,
            Direction.West => useRight ? muzzle.OffsetRightWest : muzzle.OffsetLeftWest,
            _ => useRight ? muzzle.OffsetRight : muzzle.OffsetLeft
        };

        return baseRotation.RotateVec(worldOffset);
    }

    private bool TryGetVehicle(EntityUid turretUid, out EntityUid vehicle)
    {
        vehicle = default;
        var current = turretUid;

        while (_container.TryGetContainingContainer((current, null), out var container))
        {
            var owner = container.Owner;
            if (HasComp<VehicleComponent>(owner))
            {
                vehicle = owner;
                return true;
            }

            current = owner;
        }

        return false;
    }

    private void TryGetAnchorTurret(
        EntityUid turretUid,
        VehicleTurretComponent turret,
        out EntityUid anchorUid,
        out VehicleTurretComponent anchorTurret)
    {
        anchorUid = turretUid;
        anchorTurret = turret;

        if (!HasComp<VehicleTurretAttachmentComponent>(turretUid))
            return;

        if (!TryGetParentTurret(turretUid, out var parentUid, out var parentTurret))
            return;

        anchorUid = parentUid;
        anchorTurret = parentTurret;
    }

    private bool TryGetParentTurret(
        EntityUid turretUid,
        out EntityUid parentUid,
        out VehicleTurretComponent parentTurret)
    {
        parentUid = default;
        parentTurret = default!;
        var current = turretUid;

        while (_container.TryGetContainingContainer((current, null), out var container))
        {
            var owner = container.Owner;
            if (TryComp(owner, out VehicleTurretComponent? turret))
            {
                parentUid = owner;
                parentTurret = turret;
                return true;
            }

            current = owner;
        }

        return false;
    }

    private static Vector2 GetVehicleLocalOffset(
        VehicleTurretComponent turret,
        Vector2 offset,
        Angle vehicleRot,
        Angle eyeRot)
    {
        if (turret.UseDirectionalOffsets)
            offset = (-eyeRot).RotateVec(offset);

        return (-vehicleRot).RotateVec(offset);
    }

    private static Vector2 GetPixelOffset(VehicleTurretComponent turret, Angle facing)
    {
        if (!turret.UseDirectionalOffsets)
            return turret.PixelOffset;

        return turret.PixelOffset + GetDirectionalOffset(turret, GetDirectionalDir(facing));
    }

    private static Vector2 GetDirectionalOffset(VehicleTurretComponent turret, Direction dir)
    {
        return dir switch
        {
            Direction.South => turret.PixelOffsetSouth,
            Direction.East => turret.PixelOffsetEast,
            Direction.North => turret.PixelOffsetNorth,
            Direction.West => turret.PixelOffsetWest,
            _ => Vector2.Zero
        };
    }

    private static Direction GetDirectionalDir(Angle facing)
    {
        return VehicleTurretDirectionHelpers.GetRenderAlignedCardinalDir(facing);
    }

    private Angle GetVehicleFacingAngle(EntityUid vehicle, Angle vehicleRot)
    {
        if (TryComp(vehicle, out GridVehicleMoverComponent? mover) && mover.CurrentDirection != Vector2i.Zero)
            return new Vector2(mover.CurrentDirection.X, mover.CurrentDirection.Y).ToWorldAngle();

        return vehicleRot;
    }

    private static Angle GetDirectionalAngle(Direction dir)
    {
        return dir.ToAngle();
    }

    private static Angle GetRenderFacing(
        VehicleTurretComponent turret,
        VehicleTurretComponent anchorTurret,
        Angle vehicleRot,
        Angle baseFacingAngle,
        Angle eyeRot)
    {
        return (GetOffsetFacing(turret, anchorTurret, vehicleRot, baseFacingAngle) + eyeRot).Reduced();
    }

    private static Angle GetOffsetFacing(
        VehicleTurretComponent turret,
        VehicleTurretComponent anchorTurret,
        Angle vehicleRot,
        Angle baseFacingAngle)
    {
        if (!turret.OffsetRotatesWithTurret)
            return baseFacingAngle;

        return (vehicleRot + anchorTurret.WorldRotation).Reduced();
    }

}
