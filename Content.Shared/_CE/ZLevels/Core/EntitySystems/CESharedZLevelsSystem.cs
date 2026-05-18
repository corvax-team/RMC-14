/*
 * Copyright (c) 2026 TornadgoTechnology
 * Copyright (c) 2026 CrystallEdge (https://github.com/crystallpunk-14/crystall-edge)
 *
 * SPDX-License-Identifier: PolyForm-Noncommercial-1.0.0 AND MIT
 */

using System.Numerics;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._MC;
using Content.Shared.ActionBlocker;
using Content.Shared.Maps;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IConfigurationManager _config = null!;

    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly ActionBlockerSystem _blocker = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly IMapManager _mapManager = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    private EntityQuery<MapComponent> _mapQuery;
    private EntityQuery<MapGridComponent> _gridQuery;

    private EntityQuery<CEZLevelMapComponent> _zMapQuery;
    private EntityQuery<CEZLevelsNetworkComponent> _zNetworkQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<TransformComponent> _transformQuery;

    protected EntityQuery<CEZPhysicsComponent> ZPhysicsQuery;

    private bool _clientSimulation;
    private TimeSpan _fixedTimestep;
    private int _zMapCount;
    private TimeSpan _nextClientBodyTrackingRecovery;

    protected bool ZLevelsEnabled { get; private set; }
    public bool IsZLevelsEnabled => ZLevelsEnabled;
    protected bool ShouldTrackClientPhysics => !_net.IsClient || _clientSimulation;

    public override void Initialize()
    {
        base.Initialize();

        _config.OnValueChanged(MCConfigVars.ZLevelsEnabled, OnZLevelsEnabledChanged, true);
        _config.OnValueChanged(MCConfigVars.ZLevelsPhysicsClientSimulation, i => _clientSimulation = i, true);
        _config.OnValueChanged(MCConfigVars.ZLevelsPhysicsTickRate, i => _fixedTimestep = TimeSpan.FromSeconds(1d / i), true);

        _mapQuery = GetEntityQuery<MapComponent>();
        _gridQuery = GetEntityQuery<MapGridComponent>();

        _zMapQuery = GetEntityQuery<CEZLevelMapComponent>();
        _zNetworkQuery = GetEntityQuery<CEZLevelsNetworkComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _transformQuery = GetEntityQuery<TransformComponent>();

        ZPhysicsQuery = GetEntityQuery<CEZPhysicsComponent>();

        SubscribeLocalEvent<CEZLevelMapComponent, ComponentStartup>(OnZMapStartup);
        SubscribeLocalEvent<CEZLevelMapComponent, ComponentShutdown>(OnZMapShutdown);

        InitializeActivation();
        InitializeCacheHooks();
        InitializeMovement();
        InitializeView();
    }

    protected virtual void OnZLevelsEnabledChanged(bool enabled)
    {
        ZLevelsEnabled = enabled;

        if (!enabled)
        {
            _accumulatedTime = TimeSpan.Zero;
            ClearActiveBodies();
            ClearDirtyMovement();
            return;
        }

        RebuildBodyTracking();
    }

    private void OnZMapStartup(Entity<CEZLevelMapComponent> ent, ref ComponentStartup args)
    {
        _zMapCount++;
    }

    protected virtual void OnZMapShutdown(Entity<CEZLevelMapComponent> ent, ref ComponentShutdown args)
    {
        _zMapCount = Math.Max(0, _zMapCount - 1);
    }

    public bool IsVoidAtCoordinates(EntityCoordinates coords, out Entity<CEZLevelMapComponent> belowMap)
    {
        belowMap = default;

        var mapUid = _transform.GetMapId(coords);
        if (mapUid == MapId.Nullspace)
            return false;

        var mapEntity = _map.GetMap(mapUid);
        if (!_zMapQuery.TryComp(mapEntity, out var zMapComp))
            return false;

        if (!TryMapDown((mapEntity, zMapComp), out belowMap))
            return false;

        if (!TryGetZMapGrid(mapEntity, coords.ToMap(EntityManager, _transform).Position, out var gridUid, out var mapGridComponent))
            return true;

        var tileIndices = _map.LocalToTile(gridUid, mapGridComponent, coords);
        if (!_map.TryGetTileRef(gridUid, mapGridComponent, tileIndices, out var tile))
            return true;

        return !IsZSupportTile(tile.Tile);
    }

    /// <summary>
    /// Checks whether the map is in the zLevels network. If so, returns true and the current depth + Entity of the current zLevels network.
    /// </summary>
    [PublicAPI]
    public bool TryGetZNetwork(EntityUid mapUid, out Entity<CEZLevelsNetworkComponent> zLevel)
    {
        zLevel = default;
        if (!TryComp<CEZLevelMapComponent>(mapUid, out var zLevelMapComponent))
            return false;

        if (zLevelMapComponent.NetworkUid == EntityUid.Invalid)
            return false;

        if (TerminatingOrDeleted(zLevelMapComponent.NetworkUid))
        {
            Log.Error($"Trying access to terminated z-network, map: {mapUid}, outdated network uid: {zLevelMapComponent.NetworkUid}");
            return false;
        }

        if (!TryComp<CEZLevelsNetworkComponent>(zLevelMapComponent.NetworkUid, out var zNetworkComponent))
        {
            Log.Error($"Trying access to z-network without component??? WHY?! map: {mapUid}, network uid: {zLevelMapComponent.NetworkUid}");
            return false;
        }

        zLevel = new Entity<CEZLevelsNetworkComponent>(zLevelMapComponent.NetworkUid, zNetworkComponent);
        return true;
    }

    [PublicAPI]
    public bool TryGetDepth(EntityUid mapUid, out int depth)
    {
        depth = 0;
        if (!TryComp<CEZLevelMapComponent>(mapUid, out var zLevelMapComponent))
            return false;

        if (zLevelMapComponent.NetworkUid == EntityUid.Invalid)
            return false;

        if (!TryComp<CEZLevelsNetworkComponent>(zLevelMapComponent.NetworkUid, out var zNetworkComponent))
            return false;

        // Use depth cache for O(1) lookup instead of linear search
        if (zNetworkComponent.ZLevelByEntity.TryGetValue(mapUid, out depth))
            return true;

        // Fallback to component depth if cache miss
        depth = zLevelMapComponent.Depth;
        return true;
    }

    [PublicAPI]
    public bool TryMapOffset(Entity<CEZLevelMapComponent?> entity, int offset, out Entity<CEZLevelMapComponent> output)
    {
        output = default;

        if (MapOffset(entity, offset) is not { } result)
            return false;

        output = result;
        return true;
    }

    [PublicAPI]
    public Entity<CEZLevelMapComponent>? MapOffset(Entity<CEZLevelMapComponent?> entity, int offset)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return null;

        // Offen we use 1 or -1 for getting maps
        // Because we process this separated for performance boost
        switch (offset)
        {
            case 1 when entity.Comp.MapAbove is not null:
                return new Entity<CEZLevelMapComponent>(entity.Comp.MapAbove.Value, _zMapQuery.GetComponent(entity.Comp.MapAbove.Value));
            case -1 when entity.Comp.MapBelow is not null:
                return new Entity<CEZLevelMapComponent>(entity.Comp.MapBelow.Value, _zMapQuery.GetComponent(entity.Comp.MapBelow.Value));
        }

        if (!_zNetworkQuery.TryComp(entity.Comp.NetworkUid, out var zLevelsNetworkComponent))
            return null;

        var requiredDepth = entity.Comp.Depth + offset;
        if (!zLevelsNetworkComponent.ZLevels.TryGetValue(requiredDepth, out var targetId))
            return null;

        if (!_zMapQuery.TryComp(targetId, out var zLevelMapComponent))
            return null;

        return (targetId.Value, zLevelMapComponent);
    }

    [PublicAPI]
    public bool TryMapUp(Entity<CEZLevelMapComponent?> inputMapUid, out Entity<CEZLevelMapComponent> aboveMapUid)
    {
        return TryMapOffset(inputMapUid, 1, out aboveMapUid);
    }

    [PublicAPI]
    public bool TryMapDown(Entity<CEZLevelMapComponent?> inputMapUid, out Entity<CEZLevelMapComponent> belowMapUid)
    {
        return TryMapOffset(inputMapUid, -1, out belowMapUid);
    }

    private bool IsZFloorTile(Tile tile)
    {
        if (tile.IsEmpty)
            return false;

        var tileDef = (ContentTileDefinition) TilDefMan[tile.TypeId];
        return !tileDef.IsSubFloor && !tileDef.MapAtmosphere;
    }

    private static bool IsZSupportTile(Tile tile)
    {
        return !tile.IsEmpty;
    }

    private bool TryGetZMapGrid(
        EntityUid mapUid,
        Vector2 worldPos,
        out EntityUid gridUid,
        out MapGridComponent grid)
    {
        if (_gridQuery.TryComp(mapUid, out var mapGrid))
        {
            gridUid = mapUid;
            grid = mapGrid;
            return true;
        }

        gridUid = EntityUid.Invalid;
        grid = default!;

        if (!_mapQuery.TryComp(mapUid, out var map))
            return false;

        if (!_mapManager.TryFindGridAt(new MapCoordinates(worldPos, map.MapId), out var foundGridUid, out var foundGrid))
            return false;

        gridUid = foundGridUid;
        grid = foundGrid;
        return true;
    }

    /// <summary>
    /// Returns a list of all maps above the specified map. The closest map at the top is returned first.
    /// </summary>
    [PublicAPI]
    public List<EntityUid> GetAllMapsAbove(Entity<CEZLevelMapComponent> mapUid)
    {
        if (!_zNetworkQuery.TryComp(mapUid.Comp.NetworkUid, out var networkComp) || mapUid.Comp.Depth >= networkComp.SortedMax)
            return new List<EntityUid>(0);

        var depth = mapUid.Comp.Depth;
        // Pre-allocate capacity based on estimated count to reduce reallocations
        var estimatedCapacity = networkComp.SortedZLevels.Count - (depth - networkComp.SortedMin);
        var result = new List<EntityUid>(estimatedCapacity);

        // Use reverse depth lookup for O(1) checks instead of iterating through SortedZLevels
        foreach (var (entityUid, entityDepth) in networkComp.ZLevelByEntity)
        {
            if (entityDepth > depth && _zMapQuery.TryComp(entityUid, out _))
                result.Add(entityUid);
        }
        // Sort by depth ascending (closest first)
        result.Sort((a, b) => networkComp.ZLevelByEntity[a].CompareTo(networkComp.ZLevelByEntity[b]));

        return result;
    }

    /// <summary>
    /// Returns a list of all maps below the specified map. The closest map at the bottom is returned first.
    /// </summary>
    [PublicAPI]
    public List<EntityUid> GetAllMapsBelow(Entity<CEZLevelMapComponent> mapUid)
    {
        if (!_zNetworkQuery.TryComp(mapUid.Comp.NetworkUid, out var zLevelsNetworkComponent))
            return new List<EntityUid>(0);

        var depth = mapUid.Comp.Depth;
        // Pre-allocate capacity based on depth to reduce reallocations
        var estimatedCapacity = Math.Min(depth, zLevelsNetworkComponent.SortedZLevels.Count);
        var result = new List<EntityUid>(estimatedCapacity);

        // Use reverse depth lookup for O(1) checks instead of iterating through SortedZLevels
        foreach (var (entityUid, entityDepth) in zLevelsNetworkComponent.ZLevelByEntity)
        {
            if (entityDepth < depth && _zMapQuery.TryComp(entityUid, out _))
                result.Add(entityUid);
        }

        // Sort by depth descending (closest first)
        result.Sort((a, b) => zLevelsNetworkComponent.ZLevelByEntity[b].CompareTo(zLevelsNetworkComponent.ZLevelByEntity[a]));

        return result;
    }
}
