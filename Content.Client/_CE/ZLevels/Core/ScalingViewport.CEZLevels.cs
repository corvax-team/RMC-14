/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System;
using System.Numerics;
using Content.Client._CE.ZLevels.Core;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared.Maps;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Client.Viewport;

public sealed partial class ScalingViewport
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly ITileDefinitionManager _tile = default!;

    private CEClientZLevelsSystem? _zLevels;
    private SharedMapSystem? _mapSystem;

    private EntityQuery<TransformComponent>? _xformQuery;
    private EntityQuery<MapComponent>? _mapQuery;
    private EntityQuery<CEZLevelHighGroundComponent>? _highGroundQuery;
    private readonly Dictionary<EntityUid, EmptyTileCache> _emptyTileCache = new();
    private EntityUid? _zCacheRootMap;

    private IEye? _fallbackEye;
    private readonly ZEye _zEye = new();

    private readonly record struct EmptyTileCache(Vector2i Min, Vector2i Max, int Revision, bool HasVisibleOpening);

    private void ClearZLevelCache()
    {
        _emptyTileCache.Clear();
        _zCacheRootMap = null;
    }

    /// <summary>
    /// We are looking for at least one empty tile on the screen.
    /// This is used to ensure that it makes sense to draw the z-planes and that they are visible.
    /// </summary>
    public bool TryFindEmptyTiles(EntityUid mapUid)
    {
        if (_xformQuery is null || !_xformQuery.Value.TryComp(mapUid, out var xform))
            return true;

        var drawBox = GetDrawBox();
        var mapId = xform.MapID;

        var bottomLeft = _eyeManager.ScreenToMap(drawBox.BottomLeft).Position;
        var bottomRight = _eyeManager.ScreenToMap(drawBox.BottomRight).Position;
        var topLeft = _eyeManager.ScreenToMap(drawBox.TopLeft).Position;
        var topRight = _eyeManager.ScreenToMap(drawBox.TopRight).Position;

        var minX = Math.Min(Math.Min(bottomLeft.X, bottomRight.X), Math.Min(topLeft.X, topRight.X));
        var minY = Math.Min(Math.Min(bottomLeft.Y, bottomRight.Y), Math.Min(topLeft.Y, topRight.Y));
        var maxX = Math.Max(Math.Max(bottomLeft.X, bottomRight.X), Math.Max(topLeft.X, topRight.X));
        var maxY = Math.Max(Math.Max(bottomLeft.Y, bottomRight.Y), Math.Max(topLeft.Y, topRight.Y));

        var mapCoordsBottomLeft = new MapCoordinates(new Vector2(minX, minY), mapId);
        var mapCoordsTopRight = new MapCoordinates(new Vector2(maxX, maxY), mapId);

        if (!_mapManager.TryFindGridAt(mapUid, mapCoordsBottomLeft.Position, out _, out var grid))
            return true;

        _mapSystem ??= _entityManager.System<SharedMapSystem>();
        var tileBottomLeft = _mapSystem.CoordinatesToTile(mapUid, grid, mapCoordsBottomLeft);
        var tileTopRight = _mapSystem.CoordinatesToTile(mapUid, grid, mapCoordsTopRight);
        var cacheMin = tileBottomLeft - Vector2i.One;
        var cacheMax = tileTopRight + Vector2i.One;
        var revision = _zLevels?.GetVisibilityRevision(mapUid) ?? 0;

        if (_emptyTileCache.TryGetValue(mapUid, out var cache) &&
            cache.Min == cacheMin &&
            cache.Max == cacheMax &&
            cache.Revision == revision)
        {
            return cache.HasVisibleOpening;
        }

        var hasVisibleOpening = false;

        for (var x = tileBottomLeft.X - 1; x <= tileTopRight.X + 1; x++)
        {
            for (var y = tileBottomLeft.Y - 1; y <= tileTopRight.Y + 1; y++)
            {
                if (!_mapSystem.TryGetTileRef(mapUid, grid, new Vector2i(x, y), out var tile))
                {
                    hasVisibleOpening = true;
                    break;
                }

                var tileDef = (ContentTileDefinition)_tile[tile.Tile.TypeId];
                if (tileDef.Transparent || tile.Tile.IsEmpty || HasHighGroundAt(mapUid, grid, new Vector2i(x, y)))
                {
                    hasVisibleOpening = true;
                    break;
                }
            }

            if (hasVisibleOpening)
                break;
        }

        _emptyTileCache[mapUid] = new EmptyTileCache(cacheMin, cacheMax, revision, hasVisibleOpening);
        return hasVisibleOpening;
    }

    private bool HasHighGroundAt(EntityUid mapUid, MapGridComponent grid, Vector2i tile)
    {
        if (_mapSystem is null)
            return false;

        _highGroundQuery ??= _entityManager.GetEntityQuery<CEZLevelHighGroundComponent>();

        var anchored = _mapSystem.GetAnchoredEntitiesEnumerator(mapUid, grid, tile);
        while (anchored.MoveNext(out var uid))
        {
            if (_highGroundQuery.Value.HasComp(uid))
                return true;
        }

        return false;
    }

    private bool RenderZLevels(IClydeViewport viewport)
    {
        if (_eye is null)
            return false;

        _fallbackEye = _eye;

        // Cache frequently accessed components/systems
        _xformQuery ??= _entityManager.GetEntityQuery<TransformComponent>();
        _mapQuery ??= _entityManager.GetEntityQuery<MapComponent>();

        // Cache systems and components
        _zLevels ??= _entityManager.System<CEClientZLevelsSystem>();
        _mapSystem ??= _entityManager.System<SharedMapSystem>();

        if (!_zLevels.IsZLevelsEnabled)
        {
            ClearZLevelCache();
            return false;
        }

        if (_player.LocalEntity is null)
        {
            ClearZLevelCache();
            return false;
        }

        if (!_entityManager.TryGetComponent<CEZLevelViewerComponent>(_player.LocalEntity.Value, out var zLevelViewer))
        {
            ClearZLevelCache();
            return false;
        }

        if (!_xformQuery.Value.TryComp(_player.LocalEntity, out var playerXform))
        {
            ClearZLevelCache();
            return false;
        }

        if (playerXform.MapUid is null)
        {
            ClearZLevelCache();
            return false;
        }

        if (!_entityManager.HasComponent<CEZLevelMapComponent>(playerXform.MapUid.Value))
        {
            ClearZLevelCache();
            return false;
        }

        if (_zCacheRootMap != playerXform.MapUid.Value)
        {
            _emptyTileCache.Clear();
            _zCacheRootMap = playerXform.MapUid.Value;
        }

        var lookUp = zLevelViewer.LookUp ? 1 : 0;

        var lowestDepth = 0;
        var currentMap = playerXform.MapUid.Value;
        var maxBelowDepth = Math.Max(0, _zLevels.MaxRenderedBelowDepth);
        var checkingMap = currentMap;
        for (var depth = 1; depth <= maxBelowDepth; depth++)
        {
            if (!TryFindEmptyTiles(checkingMap))
                break;

            if (!_zLevels.TryMapOffset(currentMap, -depth, out var mapUidBelow))
                break;

            lowestDepth = -depth;
            checkingMap = mapUidBelow;
        }

        if (lowestDepth == 0 && lookUp == 0)
            return false;

        Angle rotation = _fallbackEye.Rotation * -1;
        var zOffset = rotation.ToWorldVec() * CEClientZLevelsSystem.ZLevelOffset;

        //From the lowest depth to the highest, render each level
        for (var depth = lowestDepth; depth <= lookUp; depth++)
        {
            if (depth == 0)
                viewport.Eye = _fallbackEye;
            else
            {
                if (!_zLevels.TryMapOffset(currentMap, depth, out var mapUidBelow))
                    continue;

                if (!_mapQuery.Value.TryComp(mapUidBelow, out var mapComp))
                    continue;

                _zEye.LowestDepth = lowestDepth;
                _zEye.Depth = depth;
                _zEye.HighestDepth = lookUp;
                _zEye.Position = new MapCoordinates(_fallbackEye.Position.Position, mapComp.MapId);
                _zEye.DrawFov = _fallbackEye.DrawFov && depth >= 0;
                _zEye.DrawLight = _fallbackEye.DrawLight;
                _zEye.Offset = _fallbackEye.Offset + zOffset * depth;
                _zEye.Rotation = _fallbackEye.Rotation;
                _zEye.Scale = _fallbackEye.Scale;
                viewport.Eye = _zEye;
            }

            viewport.ClearColor = depth == lowestDepth ? Color.Black : null;
            viewport.Render();
        }

        // Restore the Eye
        Eye = _fallbackEye;
        viewport.Eye = Eye;
        return true;
    }

    public sealed class ZEye : Robust.Shared.Graphics.Eye
    {
        public int LowestDepth;
        public int Depth;
        public int HighestDepth;
    }
}
