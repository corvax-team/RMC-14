/*
 * Copyright (c) 2026 TornadgoTechnology
 * Copyright (c) 2026 CrystallEdge (https://github.com/crystallpunk-14/crystall-edge)
 *
 * SPDX-License-Identifier: PolyForm-Noncommercial-1.0.0 AND MIT
 */

using System.Numerics;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared.Chasm;
using Content.Shared.Throwing;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    public const int MaxZLevelsBelowRendering = 3;

    private const float ZGravityForce = 9.8f;
    private const float ZVelocityLimit = 20.0f;
    private const int MaxStepsPerFrame = 10;
    private const float HighGroundTransitionEdge = 0.6f;
    private const int HighGroundFallImpactSafeTileRadius = 1;
    /// <summary>
    /// The minimum speed required to trigger LandEvent events.
    /// </summary>
    private const float ImpactVelocityLimit = 3f;

    private EntityQuery<CEZLevelHighGroundComponent> _highgroundQuery;
    private TimeSpan _accumulatedTime = TimeSpan.Zero;

    private void InitializeMovement()
    {
        _highgroundQuery = GetEntityQuery<CEZLevelHighGroundComponent>();

        SubscribeLocalEvent<CEZPhysicsComponent, CEZLevelMapMoveEvent>(OnZLevelMapMove);
        SubscribeLocalEvent<CEActiveZPhysicsComponent, ComponentInit>(OnActiveInit);

        SubscribeLocalEvent<CEZPhysicsComponent, MoveEvent>(OnMoveEvent);
        SubscribeLocalEvent<CEZLevelMapComponent, TileChangedEvent>(OnTileChanged);
    }

    private void OnActiveInit(Entity<CEActiveZPhysicsComponent> ent, ref ComponentInit args)
    {
        if (!ZPhyzQuery.TryComp(ent, out var zComp))
            return;

        RequestCacheMovement((ent, zComp));
    }

    private void OnTileChanged(Entity<CEZLevelMapComponent> ent, ref TileChangedEvent args)
    {
        if (!TryComp<MapGridComponent>(args.Entity, out var grid))
            return;

        // For each changed tile compute its world AABB and query all entities intersecting it.
        foreach (var change in args.Changes)
        {
            var mapCoords = _map.GridTileToWorld(args.Entity, grid, change.GridIndices);

            var half = grid.TileSizeHalfVector;
            var min = mapCoords.Position - half;
            var max = mapCoords.Position + half;
            var aabb = new Box2(min, max);

            var entities = _lookup.GetEntitiesIntersecting(mapCoords.MapId, aabb);
            foreach (var uid in entities)
            {
                if (!ZPhyzQuery.TryComp(uid, out var zComp))
                    continue;

                RequestCacheMovement((uid, zComp));
            }
        }
    }

    private void RequestCacheMovement(Entity<CEZPhysicsComponent> entity, bool force = true)
    {
        var tile = _transform.GetGridOrMapTilePosition(entity);
        var map = Transform(entity).MapUid;

        if (entity.Comp.SuppressedStairTransitionTile != null &&
            (entity.Comp.SuppressedStairTransitionTile != tile ||
             entity.Comp.SuppressedStairTransitionMap != map))
        {
            ClearSuppressedStairTransition(entity.Comp);
        }

        // If we stay at same tile we don't need to recalculate a lot of math.
        if (tile == entity.Comp.CachedTile && !force)
            return;

        entity.Comp.CachedTile = _transform.GetGridOrMapTilePosition(entity);
        entity.Comp.CachedGroundHeight = ComputeGroundHeightInternal((entity, entity), out var sticky);
        entity.Comp.CachedStickyGround = sticky;

        if (entity.Comp.SuppressedStairTransitionTile != null &&
            !IsCachedStairTransitionActive(entity.Comp, entity.Comp.SuppressedStairTransitionOffset))
        {
            ClearSuppressedStairTransition(entity.Comp);
        }
    }

    private void OnMoveEvent(Entity<CEZPhysicsComponent> ent, ref MoveEvent args)
    {
        RequestCacheMovement(ent);
    }

    private void OnZLevelMapMove(Entity<CEZPhysicsComponent> ent, ref CEZLevelMapMoveEvent args)
    {
        ent.Comp.CurrentZLevel = args.CurrentZLevel;
        DirtyField(ent, ent.Comp, nameof(CEZPhysicsComponent.CurrentZLevel));
        // Update cached ground height when entity moves between Z-level maps
        RequestCacheMovement(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!ZLevelsEnabled)
        {
            _accumulatedTime = TimeSpan.Zero;
            return;
        }

        if (_net.IsClient && !_clientSimulation)
            return;

        if (_zMapCount == 0 || _activeZPhysicsCount == 0)
        {
            _accumulatedTime = TimeSpan.Zero;
            return;
        }

        _accumulatedTime += TimeSpan.FromSeconds(frameTime);

        var steps = 0;
        while (_accumulatedTime >= _fixedTimestep && steps < MaxStepsPerFrame)
        {
            UpdateZPhysics((float) _fixedTimestep.TotalSeconds);

            _accumulatedTime -= _fixedTimestep;
            steps++;
        }
    }

    private void UpdateZPhysics(float frameTime)
    {
        var query = EntityQueryEnumerator<CEZPhysicsComponent, CEActiveZPhysicsComponent, TransformComponent, PhysicsComponent>();
        while (query.MoveNext(out var uid, out var zPhysicsComponent, out _, out var xform, out var physics))
        {
            // Early exit if map is invalid - cheaper check before component query
            if (xform.MapUid == null || xform.MapUid == EntityUid.Invalid)
                continue;

            if (!_zMapQuery.HasComp(xform.MapUid.Value))
                continue;

            var oldVelocity = zPhysicsComponent.Velocity;
            var oldHeight = zPhysicsComponent.LocalPosition;

            if (physics.BodyStatus == BodyStatus.OnGround)
            {
                if (zPhysicsComponent.VelocityGravity)
                {
                    zPhysicsComponent.Velocity -= ZGravityForce * zPhysicsComponent.GravityMultiplier * frameTime;
                }

                // Custom velocity application
                if (zPhysicsComponent.VelocityRaiseEvent)
                {
                    var velocityEvent = new CEGetZVelocityEvent((uid, zPhysicsComponent));
                    RaiseLocalEvent(uid, ref velocityEvent);

                    zPhysicsComponent.Velocity += velocityEvent.VelocityDelta * frameTime;
                }
            }

            // Movement application
            zPhysicsComponent.LocalPosition += zPhysicsComponent.Velocity * frameTime;

            var distanceToGround = zPhysicsComponent.LocalPosition - zPhysicsComponent.CachedGroundHeight;

            // AutoStep: lift entity up if floor is higher
            if (zPhysicsComponent.AutoStep && distanceToGround < 0)
                zPhysicsComponent.LocalPosition -= distanceToGround; //Lift up

            // Sticky ground: only pull down when slowly falling on sticky surfaces (ladders)
            if (zPhysicsComponent.CachedStickyGround)
                zPhysicsComponent.LocalPosition -= distanceToGround; //Sticky move down

            if (zPhysicsComponent is { Velocity: < 0, Fallable: true }) //Falling down
            {
                if (distanceToGround <= 0.05f) //There`s a ground
                {
                    var nearHighGround = HasNearbyHighGroundForFallImpact(uid);
                    if (float.Abs(zPhysicsComponent.Velocity) >= ImpactVelocityLimit &&
                        !nearHighGround)
                    {
                        var hitEv = new CEZLevelHitEvent(-zPhysicsComponent.Velocity);
                        RaiseLocalEvent(uid, ref hitEv);

                        var land = new LandEvent(null, true);
                        RaiseLocalEvent(uid, ref land);
                    }

                    zPhysicsComponent.Velocity = -zPhysicsComponent.Velocity * zPhysicsComponent.Bounciness;
                }
            }

            if (zPhysicsComponent.LocalPosition < 0) //Need teleport to ZLevel down
            {
                var wasOnStairs = zPhysicsComponent.CachedStickyGround;
                if (wasOnStairs && IsStairTransitionSuppressed((uid, zPhysicsComponent), -1))
                {
                    zPhysicsComponent.LocalPosition = 0.05f;
                    zPhysicsComponent.Velocity = 0f;
                }
                else if (TryMoveDownOrChasm(uid))
                {
                    zPhysicsComponent.LocalPosition = wasOnStairs ? GetPostStairTransitionLocalPosition(zPhysicsComponent, 1) : zPhysicsComponent.LocalPosition + 1;

                    if (wasOnStairs)
                    {
                        zPhysicsComponent.Velocity = 0f;
                        SuppressReverseStairTransition((uid, zPhysicsComponent), 1);
                    }

                    if (zPhysicsComponent is { CachedStickyGround: false, Fallable: true } &&
                        !HasNearbyHighGroundForFallImpact(uid))
                    {
                        var fallEv = new CEZLevelFallMapEvent();
                        RaiseLocalEvent(uid, ref fallEv);
                    }
                }
            }

            if (zPhysicsComponent.LocalPosition >= 1) //Need teleport to ZLevel up
            {
                if (HasTileAbove(uid)) //Hit roof
                {
                    if (float.Abs(zPhysicsComponent.Velocity) >= ImpactVelocityLimit &&
                        !HasNearbyHighGroundForFallImpact(uid))
                    {
                        var hitEv = new CEZLevelHitEvent(zPhysicsComponent.Velocity);
                        RaiseLocalEvent(uid, ref hitEv);

                        var land = new LandEvent(null, true);
                        RaiseLocalEvent(uid, ref land);
                    }

                    zPhysicsComponent.LocalPosition = 1;
                    zPhysicsComponent.Velocity = -zPhysicsComponent.Velocity * zPhysicsComponent.Bounciness;
                }
                else
                {
                    var wasOnStairs = zPhysicsComponent.CachedStickyGround;
                    if (wasOnStairs && IsStairTransitionSuppressed((uid, zPhysicsComponent), 1))
                    {
                        zPhysicsComponent.LocalPosition = 0.95f;
                        zPhysicsComponent.Velocity = 0f;
                    }
                    else if (TryMoveUp(uid))
                    {
                        zPhysicsComponent.LocalPosition = wasOnStairs ? GetPostStairTransitionLocalPosition(zPhysicsComponent, -1) : zPhysicsComponent.LocalPosition - 1;

                        if (wasOnStairs)
                        {
                            zPhysicsComponent.Velocity = 0f;
                            SuppressReverseStairTransition((uid, zPhysicsComponent), -1);
                        }
                    }
                }
            }

            if (float.Abs(zPhysicsComponent.Velocity) > ZVelocityLimit)
                zPhysicsComponent.Velocity = float.Sign(zPhysicsComponent.Velocity) * ZVelocityLimit;

            if (float.Abs(oldVelocity - zPhysicsComponent.Velocity) > 0.01f)
                DirtyField(uid, zPhysicsComponent, nameof(CEZPhysicsComponent.Velocity));

            if (float.Abs(oldHeight - zPhysicsComponent.LocalPosition) > 0.01f)
                DirtyField(uid, zPhysicsComponent, nameof(CEZPhysicsComponent.LocalPosition));
        }
    }

    /// <summary>
    /// Returns the last cached distance to the floor.
    /// </summary>
    /// <param name="target">The entity, the distance to the floor which we calculate</param>
    /// <returns></returns>
    public float DistanceToGround(Entity<CEZPhysicsComponent?> target)
    {
        if (!Resolve(target, ref target.Comp, false))
            return 0;

        return target.Comp.LocalPosition - target.Comp.CachedGroundHeight;
    }

    /// <summary>
    /// Computes the "ground height" relative to the entity's current Z-level.
    /// Returns values where 0 means ground on the same level, -1 means ground one level below,
    /// and intermediate values are possible for high ground entities (stairs).
    /// </summary>
    private float ComputeGroundHeightInternal(Entity<CEZPhysicsComponent?> target, out bool stickyGround, int maxFloors = 1)
    {
        stickyGround = false;

        if (!Resolve(target, ref target.Comp, false))
            return 0;

        var xform = Transform(target);
        if (!_zMapQuery.TryComp(xform.MapUid, out var zMapComp))
            return 0;

        var worldPosI = _transform.GetGridOrMapTilePosition(target);
        var worldPos = _transform.GetWorldPosition(target);

        //Select current map by default
        Entity<CEZLevelMapComponent> checkingMap = (xform.MapUid.Value, zMapComp);
        var checkingGridUid = EntityUid.Invalid;
        MapGridComponent? checkingGrid = null;
        TryGetZMapGrid(checkingMap, worldPos, out checkingGridUid, out checkingGrid);

        for (var floor = 0; floor <= maxFloors; floor++)
        {
            if (floor != 0) //Select map below
            {
                if (!TryMapOffset((checkingMap.Owner, checkingMap.Comp), -floor, out var tempCheckingMap))
                    continue;
                if (!TryGetZMapGrid(tempCheckingMap, worldPos, out var tempCheckingGridUid, out var tempCheckingGrid))
                    continue;

                checkingMap = tempCheckingMap;
                checkingGridUid = tempCheckingGridUid;
                checkingGrid = tempCheckingGrid;
            }

            // Validate map and grid before using enumerator
            if (checkingMap.Owner == EntityUid.Invalid || checkingGridUid == EntityUid.Invalid || checkingGrid == null)
                continue;

            //Check all types of ZHeight entities
            var query = _map.GetAnchoredEntitiesEnumerator(checkingGridUid, checkingGrid, worldPosI);
            bool foundHighground = false;
            while (query.MoveNext(out var uid))
            {
                if (!_highgroundQuery.TryComp(uid, out var heightComp))
                    continue;

                foundHighground = true;

                var dir = _transform.GetWorldRotation(uid.Value).GetCardinalDir();

                var local = new Vector2((worldPos.X % 1 + 1) % 1, (worldPos.Y % 1 + 1) % 1);

                var t = dir switch
                {
                    Direction.East => heightComp.Corner ? (local.X + 1f - local.Y) / 2f : local.X,
                    Direction.West => heightComp.Corner ? (1f - local.X + local.Y) / 2f : 1f - local.X,
                    Direction.North => heightComp.Corner ? (local.X + local.Y) / 2f : local.Y,
                    Direction.South => heightComp.Corner ? (1f - local.X + 1f - local.Y) / 2f : 1f - local.Y,
                    _ => 0.5f,
                };

                t = float.Clamp(t, 0f, 1f);

                var curve = heightComp.HeightCurve;
                if (curve.Count == 0)
                    continue;

                if (curve.Count == 1)
                {
                    var groundY = curve[0];
                    // groundHeight is negative downwards: -floor + groundY
                    return -floor + groundY;
                }

                var step = 1f / (curve.Count - 1);
                var index = (int)(t / step);
                var frac = (t - index * step) / step;

                var y0 = curve[Math.Clamp(index, 0, curve.Count - 1)];
                var y1 = curve[Math.Clamp(index + 1, 0, curve.Count - 1)];

                var groundYInterp = MathHelper.Lerp(y0, y1, frac);
                groundYInterp = ClampHighGroundTransition(curve, t, groundYInterp);

                if (target.Comp.Velocity < 0 && target.Comp.Velocity > -2f && heightComp.Stick)
                    stickyGround = true;

                return -floor + groundYInterp;
            }

            //No ZEntities found, check floor tiles
            if (!foundHighground)
            {
                if (_map.TryGetTileRef(checkingGridUid, checkingGrid, worldPosI, out var floorTileRef) &&
                    IsZSupportTile(floorTileRef.Tile))
                    return -floor; // tile ground has groundY == 0 -> -floor
            }
        }

        return -maxFloors;
    }

    private static float ClampHighGroundTransition(List<float> curve, float t, float height)
    {
        if (height >= 1f && !IsAtHighGroundTransitionEdge(curve, t, 1f, true))
            return 0.95f;

        if (height < 0f && !IsAtHighGroundTransitionEdge(curve, t, 0f, false))
            return 0.05f;

        return height;
    }

    private static bool IsAtHighGroundTransitionEdge(List<float> curve, float t, float limit, bool upper)
    {
        var first = curve[0];
        var last = curve[^1];

        if (upper)
        {
            if (first >= limit && t <= HighGroundTransitionEdge)
                return true;

            if (last >= limit && t >= 1f - HighGroundTransitionEdge)
                return true;
        }
        else
        {
            if (first < limit && t <= HighGroundTransitionEdge)
                return true;

            if (last < limit && t >= 1f - HighGroundTransitionEdge)
                return true;
        }

        return false;
    }

    private float GetPostStairTransitionLocalPosition(CEZPhysicsComponent component, int reverseOffset)
    {
        if (!component.CachedStickyGround)
            return 0.05f;

        return reverseOffset switch
        {
            > 0 when component.CachedGroundHeight >= 1f => 0.95f,
            < 0 when component.CachedGroundHeight < 0f => 0.05f,
            _ => float.Clamp(component.CachedGroundHeight, 0.05f, 0.95f),
        };
    }

    private bool HasNearbyHighGroundForFallImpact(EntityUid uid)
    {
        var xform = Transform(uid);
        if (xform.MapUid is not { } mapUid)
            return false;

        var worldPos = _transform.GetWorldPosition(xform);
        if (!TryGetZMapGrid(mapUid, worldPos, out var gridUid, out var grid))
            return false;

        var center = _transform.GetGridOrMapTilePosition(uid);
        var radius = HighGroundFallImpactSafeTileRadius;

        for (var x = center.X - radius; x <= center.X + radius; x++)
        {
            for (var y = center.Y - radius; y <= center.Y + radius; y++)
            {
                var anchored = _map.GetAnchoredEntitiesEnumerator(gridUid, grid, new Vector2i(x, y));
                while (anchored.MoveNext(out var ent))
                {
                    if (_highgroundQuery.HasComp(ent))
                        return true;
                }
            }
        }

        return false;
    }

    private bool IsStairTransitionSuppressed(Entity<CEZPhysicsComponent> ent, int offset)
    {
        return ent.Comp.SuppressedStairTransitionOffset == offset &&
               ent.Comp.SuppressedStairTransitionTile == _transform.GetGridOrMapTilePosition(ent) &&
               ent.Comp.SuppressedStairTransitionMap == Transform(ent).MapUid;
    }

    private static bool IsCachedStairTransitionActive(CEZPhysicsComponent component, int offset)
    {
        if (!component.CachedStickyGround)
            return false;

        return offset switch
        {
            > 0 => component.CachedGroundHeight >= 1f,
            < 0 => component.CachedGroundHeight < 0f,
            _ => false,
        };
    }

    private void SuppressReverseStairTransition(Entity<CEZPhysicsComponent> ent, int reverseOffset)
    {
        ent.Comp.SuppressedStairTransitionTile = _transform.GetGridOrMapTilePosition(ent);
        ent.Comp.SuppressedStairTransitionMap = Transform(ent).MapUid;
        ent.Comp.SuppressedStairTransitionOffset = reverseOffset;
    }

    private static void ClearSuppressedStairTransition(CEZPhysicsComponent component)
    {
        component.SuppressedStairTransitionTile = null;
        component.SuppressedStairTransitionMap = null;
        component.SuppressedStairTransitionOffset = 0;
    }

    /// <summary>
    /// Checks whether there is a ceiling above the specified entity (tiles on the layer above).
    /// If there are no Z-levels above, false will be returned.
    /// </summary>
    [PublicAPI]
    public bool HasTileAbove(EntityUid ent, Entity<CEZLevelMapComponent?>? currentMapUid = null)
    {
        currentMapUid ??= Transform(ent).MapUid;

        if (currentMapUid is null)
            return false;

        if (!TryMapUp(currentMapUid.Value, out var mapAboveUid))
            return false;

        var worldPos = _transform.GetWorldPosition(ent);
        if (!TryGetZMapGrid(mapAboveUid, worldPos, out var mapAboveGridUid, out var mapAboveGrid))
            return false;

        if (_map.TryGetTileRef(mapAboveGridUid, mapAboveGrid, worldPos, out var tileRef) &&
            IsZFloorTile(tileRef.Tile))
            return true;

        return false;
    }

    /// <summary>
    /// Checks whether there is a ceiling above the specified entity (tiles on the layer above).
    /// If there are no Z-levels above, false will be returned.
    /// </summary>
    [PublicAPI]
    public bool HasTileAbove(Vector2i indices, Entity<CEZLevelMapComponent?> map)
    {
        if (!Resolve(map, ref map.Comp, false))
            return false;

        if (!TryMapUp(map, out var mapAboveUid))
            return false;

        var worldPos = new Vector2(indices.X + 0.5f, indices.Y + 0.5f);
        if (!TryGetZMapGrid(mapAboveUid, worldPos, out var mapAboveGridUid, out var mapAboveGrid))
            return false;

        if (_map.TryGetTileRef(mapAboveGridUid, mapAboveGrid, indices, out var tileRef) &&
            IsZFloorTile(tileRef.Tile))
            return true;

        return false;
    }

    [PublicAPI]
    public void SetZPosition(Entity<CEZPhysicsComponent?> ent, float newPosition)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.LocalPosition = newPosition;
        DirtyField(ent, ent.Comp, nameof(CEZPhysicsComponent.LocalPosition));
    }

    [PublicAPI]
    public void UpdateGravityState(Entity<CEZPhysicsComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        var ev = new CECheckGravityEvent();
        RaiseLocalEvent(ent.Owner, ref ev);

        SetZGravity(ent, ev.Gravity);
    }

    private void SetZGravity(Entity<CEZPhysicsComponent?> ent, float newGravityMultiplier)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.GravityMultiplier = newGravityMultiplier;
        DirtyField(ent, ent.Comp, nameof(CEZPhysicsComponent.GravityMultiplier));
    }

    /// <summary>
    /// Sets the vertical velocity for the entity. Positive values make the entity fly upward. Negative values make it fly downward.
    /// </summary>
    [PublicAPI]
    public void SetZVelocity(Entity<CEZPhysicsComponent?> ent, float newVelocity)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.Velocity = newVelocity;
        DirtyField(ent, ent.Comp, nameof(CEZPhysicsComponent.Velocity));
    }

    /// <summary>
    /// Add the vertical velocity for the entity. Positive values make the entity fly upward. Negative values make it fly downward.
    /// </summary>
    [PublicAPI]
    public void AddZVelocity(Entity<CEZPhysicsComponent?> ent, float newVelocity)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        ent.Comp.Velocity += newVelocity;
        DirtyField(ent, ent.Comp, nameof(CEZPhysicsComponent.Velocity));
    }

    [PublicAPI]
    public bool TryMove(EntityUid ent, int offset, Entity<CEZLevelMapComponent?>? map = null)
    {
        if (!ZLevelsEnabled)
            return false;

        map ??= Transform(ent).MapUid;

        if (map is null)
            return false;

        if (!TryMapOffset(map.Value, offset, out var targetMap))
            return false;

        if (!_mapQuery.TryComp(targetMap, out var targetMapComp))
            return false;


        var worldPosition = _transform.GetWorldPosition(ent);
        var worldRotation = _transform.GetWorldRotation(ent);

        _transform.SetMapCoordinates(ent, new MapCoordinates(worldPosition, targetMapComp.MapId));
        _transform.SetWorldRotation(ent, worldRotation);

        var ev = new CEZLevelMapMoveEvent(offset, targetMap.Comp.Depth);
        RaiseLocalEvent(ent, ref ev);

        return true;
    }

    [PublicAPI]
    public bool TryMoveUp(EntityUid ent)
    {
        return TryMove(ent, 1);
    }

    [PublicAPI]
    public bool TryMoveDown(EntityUid ent)
    {
        return TryMove(ent, -1);
    }

    [PublicAPI]
    public bool TryMoveDownOrChasm(EntityUid ent)
    {
        if (TryMoveDown(ent))
            return true;

        return false;
    }
}

/// <summary>
/// Is called on an entity when it moves between z-levels.
/// </summary>
/// <param name="offset">How many levels were crossed. If negative, it means there was a downward movement. If positive, it means an upward movement.</param>
[ByRefEvent]
public struct CEZLevelMapMoveEvent(int offset, int level)
{
    /// <summary>
    /// How many levels were crossed. If negative, it means there was a downward movement. If positive, it means an upward movement.
    /// </summary>
    public int Offset = offset;

    public int CurrentZLevel = level;
}

/// <summary>
/// Is triggered when an entity falls to the lower z-levels under the force of gravity
/// </summary>
[ByRefEvent]
public struct CEZLevelFallMapEvent;

/// <summary>
/// It is called on an entity when it hits the floor or ceiling with force.
/// </summary>
/// <param name="impactPower">The speed at the moment of impact. Always positive</param>
[ByRefEvent]
public struct CEZLevelHitEvent(float impactPower)
{
    /// <summary>
    /// The speed at the moment of impact. Always positive
    /// </summary>
    public float ImpactPower = impactPower;
}

/// <summary>
/// Is called every frame to calculate the current vertical velocity of the object with CEActiveZPhysicsComponent.
/// </summary>
[ByRefEvent]
public struct CEGetZVelocityEvent(Entity<CEZPhysicsComponent> target)
{
    public Entity<CEZPhysicsComponent> Target = target;
    public float VelocityDelta = 0;
}

/// <summary>
/// Called when UpdateGravityState is used to update the current strength of the active z-level gravity. Various systems can subscribe to this to disable gravity.
/// </summary>
[ByRefEvent]
public struct CECheckGravityEvent()
{
    public float Gravity = 1f;
}
