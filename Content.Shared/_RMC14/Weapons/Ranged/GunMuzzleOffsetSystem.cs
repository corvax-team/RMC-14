using System.Numerics;
using Content.Shared._RMC14.Emplacements;
using Content.Shared._RMC14.Vehicle;
using Content.Shared.Vehicle.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Shared._RMC14.Weapons.Ranged;

public sealed class GunMuzzleOffsetSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GunMuzzleOffsetComponent, AttemptShootEvent>(OnAttemptShoot);
        SubscribeLocalEvent<GunMuzzleOffsetComponent, RMCBeforeMuzzleFlashEvent>(OnBeforeMuzzleFlash, after: new[] { typeof(MountableWeaponSystem) });
    }

    private void OnAttemptShoot(Entity<GunMuzzleOffsetComponent> ent, ref AttemptShootEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryGetMuzzleCoordinates(ent, args.ToCoordinates, out var fromCoords))
            return;

        args.FromCoordinates = fromCoords;
    }

    private void OnBeforeMuzzleFlash(Entity<GunMuzzleOffsetComponent> ent, ref RMCBeforeMuzzleFlashEvent args)
    {
        if (!ent.Comp.ApplyToMuzzleFlash)
            return;

        EntityCoordinates? target = null;
        if (TryComp(ent, out GunComponent? gun))
            target = gun.ShootCoordinates;

        if (!TryGetMuzzleCoordinates(ent, target, out var muzzleCoords))
            return;

        var muzzleMap = _transform.ToMapCoordinates(muzzleCoords);
        var weaponMap = _transform.GetMapCoordinates(args.Weapon);
        if (muzzleMap.MapId != weaponMap.MapId)
            return;

        var worldOffset = muzzleMap.Position - weaponMap.Position;
        var weaponRotation = _transform.GetWorldRotation(args.Weapon);
        args.Offset = (-weaponRotation).RotateVec(worldOffset);
    }

    private bool TryGetMuzzleCoordinates(
        Entity<GunMuzzleOffsetComponent> ent,
        EntityCoordinates? toCoordinates,
        out EntityCoordinates muzzleCoords)
    {
        muzzleCoords = default;

        if (ent.Comp.Offset == Vector2.Zero &&
            ent.Comp.MuzzleOffset == Vector2.Zero &&
            !ent.Comp.UseDirectionalOffsets)
        {
            return false;
        }

        var baseUid = ent.Owner;
        if (ent.Comp.UseContainerOwner &&
            _container.TryGetContainingContainer((ent.Owner, null), out var container))
        {
            baseUid = container.Owner;
        }

        var baseCoords = _transform.GetMoverCoordinates(baseUid);
        var baseRotation = GetBaseRotation(baseUid, ent.Comp.AngleOffset);
        var (offset, rotateOffset) = GetOffset(ent.Comp, baseUid, baseRotation);
        muzzleCoords = rotateOffset
            ? baseCoords.Offset(baseRotation.RotateVec(offset))
            : baseCoords.Offset(offset);

        if (ent.Comp.MuzzleOffset == Vector2.Zero)
            return true;

        var muzzleRotation = baseRotation;
        if (ent.Comp.UseAimDirection && toCoordinates != null)
        {
            var pivotMap = _transform.ToMapCoordinates(muzzleCoords);
            var targetMap = _transform.ToMapCoordinates(toCoordinates.Value);
            if (pivotMap.MapId == targetMap.MapId)
            {
                var direction = targetMap.Position - pivotMap.Position;
                if (direction.LengthSquared() > 0.0001f)
                    muzzleRotation = direction.ToWorldAngle() + ent.Comp.AngleOffset;
            }
        }

        muzzleCoords = muzzleCoords.Offset(muzzleRotation.RotateVec(ent.Comp.MuzzleOffset));
        return true;
    }

    private Angle GetBaseRotation(EntityUid baseUid, Angle angleOffset)
    {
        var rotation = _transform.GetWorldRotation(baseUid);
        if (TryComp(baseUid, out GridVehicleMoverComponent? mover) && mover.CurrentDirection != Vector2i.Zero)
            rotation = new Vector2(mover.CurrentDirection.X, mover.CurrentDirection.Y).ToWorldAngle();

        return rotation + angleOffset;
    }

    private (Vector2 Offset, bool Rotate) GetOffset(
        GunMuzzleOffsetComponent muzzle,
        EntityUid baseUid,
        Angle baseRotation)
    {
        if (!muzzle.UseDirectionalOffsets)
            return (muzzle.Offset, true);

        var dir = TryGetTurretLocalDirection(baseUid, out var turretDir)
            ? turretDir
            : GetBaseDirection(baseUid, baseRotation);

        var offset = GetDirectionalOffset(muzzle, dir);

        return (offset, muzzle.RotateDirectionalOffsets);
    }

    private bool TryGetTurretLocalDirection(EntityUid baseUid, out Direction dir)
    {
        dir = default;

        if (!TryComp(baseUid, out VehicleTurretComponent? turret) ||
            !turret.OffsetRotatesWithTurret)
        {
            return false;
        }

        TryGetAnchorTurret(baseUid, turret, out _, out var anchorTurret);
        var localRotation = anchorTurret.RotateToCursor ? anchorTurret.WorldRotation : Angle.Zero;
        dir = VehicleTurretDirectionHelpers.GetRenderAlignedCardinalDir(localRotation);
        return true;
    }

    private static Vector2 GetDirectionalOffset(GunMuzzleOffsetComponent muzzle, Direction dir)
    {
        return dir switch
        {
            Direction.North => muzzle.OffsetNorth,
            Direction.East => muzzle.OffsetEast,
            Direction.South => muzzle.OffsetSouth,
            Direction.West => muzzle.OffsetWest,
            _ => muzzle.Offset,
        };
    }

    private Direction GetBaseDirection(EntityUid baseUid, Angle baseRotation)
    {
        if (TryComp(baseUid, out GridVehicleMoverComponent? mover) && mover.CurrentDirection != Vector2i.Zero)
            return mover.CurrentDirection.AsDirection();

        return VehicleTurretDirectionHelpers.GetRenderAlignedCardinalDir(baseRotation);
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

}
