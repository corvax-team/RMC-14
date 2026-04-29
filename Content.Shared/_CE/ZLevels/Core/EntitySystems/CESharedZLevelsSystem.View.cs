/*
 * Copyright (c) 2026 TornadgoTechnology
 * Copyright (c) 2026 CrystallEdge (https://github.com/crystallpunk-14/crystall-edge)
 *
 * SPDX-License-Identifier: PolyForm-Noncommercial-1.0.0 AND MIT
 */

using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared.Actions;
using Content.Shared.Maps;
using Robust.Shared.Map;
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
        currentMapUid ??= Transform(ent).MapUid;

        if (currentMapUid is null)
            return false;

        var indices = _transform.GetGridOrMapTilePosition(ent);
        return HasOpaqueAbove(indices, currentMapUid.Value);
    }

    public void RefreshViewerVisibilityCache(Entity<CEZLevelViewerComponent> ent, bool force = false)
    {
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

        var indices = _transform.GetGridOrMapTilePosition(ent);
        if (!force && ent.Comp.CachedOpaqueAboveValid && ent.Comp.CachedOpaqueAboveTile == indices)
            return;

        ent.Comp.CachedOpaqueAboveTile = indices;
        ent.Comp.CachedOpaqueAbove = HasOpaqueAbove(indices, (mapUid, zMapComp));
        ent.Comp.CachedOpaqueAboveValid = true;
    }

    private bool HasOpaqueAbove(Vector2i indices, Entity<CEZLevelMapComponent?> currentMapUid)
    {
        if (!TryMapUp(currentMapUid, out var mapAboveUid))
            return false;

        if (!_gridQuery.TryComp(mapAboveUid, out var mapAboveGrid))
            return false;

        if (!_map.TryGetTileRef(mapAboveUid, mapAboveGrid, indices, out var tileRef))
            return false;

        var tileDef = (ContentTileDefinition)TilDefMan[tileRef.Tile.TypeId];
        return !tileDef.Transparent;
    }
}

public sealed partial class CEToggleZLevelLookUpAction : InstantActionEvent;
