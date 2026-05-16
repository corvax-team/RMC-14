/*
 * Copyright (c) 2026 TornadgoTechnology
 * Copyright (c) 2026 CrystallEdge (https://github.com/crystallpunk-14/crystall-edge)
 *
 * SPDX-License-Identifier: PolyForm-Noncommercial-1.0.0 AND MIT
 */

using System.Numerics;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared.Actions;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    [Dependency] protected readonly ITileDefinitionManager TilDefMan = null!;

    private void InitializeView()
    {
        SubscribeLocalEvent<CEZLevelViewerComponent, MoveEvent>(OnViewerMove);
        SubscribeLocalEvent<CEZLevelViewerComponent, CEToggleZLevelLookUpAction>(OnToggleLookUp);
    }

    protected virtual void OnViewerMove(Entity<CEZLevelViewerComponent> ent, ref MoveEvent args)
    {
        if (!ZLevelsEnabled)
            return;

        RefreshViewerVisibilityCache(ent);

        if (!ent.Comp.LookUp)
            return;

        if (!ent.Comp.CachedOpaqueAbove)
            return;

        ent.Comp.LookUp = false;
        DirtyField(ent, ent.Comp, nameof(CEZLevelViewerComponent.LookUp));
    }

    protected virtual void OnToggleLookUp(Entity<CEZLevelViewerComponent> ent, ref CEToggleZLevelLookUpAction args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!ZLevelsEnabled)
            return;

        RefreshViewerVisibilityCache(ent, true);

        if (ent.Comp.CachedOpaqueAbove)
        {
            _popup.PopupClient(Loc.GetString("ce-zlevel-look-up-fail"), ent, ent);
            return;
        }

        ent.Comp.LookUp = !ent.Comp.LookUp;
        DirtyField(ent, ent.Comp, nameof(CEZLevelViewerComponent.LookUp));
    }

    public bool HasOpaqueAbove(EntityUid ent, Entity<CEZLevelMapComponent?>? currentMapUid = null)
    {
        if (!ZLevelsEnabled)
            return false;

        currentMapUid ??= Transform(ent).MapUid;

        if (currentMapUid is null)
            return false;

        var worldPos = _transform.GetWorldPosition(ent);
        return HasOpaqueAbove(worldPos, currentMapUid.Value);
    }

    public void RefreshViewerVisibilityCache(Entity<CEZLevelViewerComponent> ent, bool force = false)
    {
        if (!ZLevelsEnabled)
        {
            ent.Comp.CachedOpaqueAbove = false;
            ent.Comp.CachedOpaqueAboveValid = false;
            ent.Comp.CachedOpaqueAboveTile = null;
            return;
        }

        var xform = Transform(ent);
        if (xform.MapUid is not { } mapUid)
        {
            ent.Comp.CachedOpaqueAbove = false;
            ent.Comp.CachedOpaqueAboveValid = false;
            ent.Comp.CachedOpaqueAboveTile = null;
            return;
        }

        if (!TryComp<CEZLevelMapComponent>(mapUid, out var zMapComp))
        {
            ent.Comp.CachedOpaqueAbove = false;
            ent.Comp.CachedOpaqueAboveValid = false;
            ent.Comp.CachedOpaqueAboveTile = null;
            return;
        }

        var indices = GetViewerTilePosition(ent);
        if (!force && ent.Comp.CachedOpaqueAboveValid && ent.Comp.CachedOpaqueAboveTile == indices)
            return;

        var worldPos = _transform.GetWorldPosition(xform);
        ent.Comp.CachedOpaqueAboveTile = indices;
        ent.Comp.CachedOpaqueAbove = HasOpaqueAbove(worldPos, (mapUid, zMapComp));
        ent.Comp.CachedOpaqueAboveValid = true;
    }

    private bool HasOpaqueAbove(Vector2 worldPos, Entity<CEZLevelMapComponent?> currentMapUid)
    {
        if (!TryMapUp(currentMapUid, out var mapAboveUid))
            return false;

        if (!TryGetZMapGrid(mapAboveUid, worldPos, out var mapAboveGridUid, out var mapAboveGrid))
            return false;

        if (!_map.TryGetTileRef(mapAboveGridUid, mapAboveGrid, worldPos, out var tileRef))
            return false;

        return IsZSupportTile(tileRef.Tile);
    }

    private bool HasHighGroundAt(Entity<CEZLevelMapComponent?> map, Vector2i indices)
    {
        if (!Resolve(map, ref map.Comp, false))
            return false;

        var worldPos = new Vector2(indices.X + 0.5f, indices.Y + 0.5f);
        if (!TryGetZMapGrid(map.Owner, worldPos, out var gridUid, out var grid))
            return false;

        var anchored = _map.GetAnchoredEntitiesEnumerator(gridUid, grid, indices);
        while (anchored.MoveNext(out var uid))
        {
            if (_zHighGroundQuery.HasComp(uid))
                return true;
        }

        return false;
    }

    protected Vector2i GetViewerTilePosition(EntityUid ent)
    {
        var xform = Transform(ent);
        var worldPos = _transform.GetWorldPosition(xform);
        if (xform.GridUid is not { } gridUid)
            return worldPos.Floored();

        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return worldPos.Floored();

        if (Transform(gridUid).MapID != xform.MapID)
            return worldPos.Floored();

        var local = _map.WorldToLocal(gridUid, grid, worldPos);
        return new Vector2i(
            (int) Math.Floor(local.X / grid.TileSize),
            (int) Math.Floor(local.Y / grid.TileSize));
    }
}

public sealed partial class CEToggleZLevelLookUpAction : InstantActionEvent;
