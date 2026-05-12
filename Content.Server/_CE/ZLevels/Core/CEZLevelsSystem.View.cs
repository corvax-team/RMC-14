/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Ghost;
using Content.Shared.IdentityManagement;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Content.Shared._MC;
using Prometheus;
using System.Numerics;
using Robust.Shared;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CE.ZLevels.Core;

public sealed partial class CEZLevelsSystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ViewSubscriberSystem _viewSubscriber = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private readonly EntProtoId _zEyeProto = "CEZLevelEye";
    private const float ViewerChunkSize = 8f;

    private int _viewerMaxPreloadBelowDepth = 1;
    private bool _viewerKeepAboveHot;
    private float _viewerPvsRange = CVars.NetMaxUpdateRange.DefaultValue;
    private float _viewerPvsPriorityRange = CVars.NetPvsPriorityRange.DefaultValue;
    private int _viewerPreloadTileRadius = 18;

    private static readonly Histogram ViewerPreloadUsage = Metrics.CreateHistogram(
        "content_zlevels_viewer_preload_usage",
        "Amount of time spent updating z-level preload viewers");

    private void InitView()
    {
        _config.OnValueChanged(MCConfigVars.ZLevelsViewerMaxPreloadBelowDepth, v => _viewerMaxPreloadBelowDepth = Math.Max(0, v), true);
        _config.OnValueChanged(MCConfigVars.ZLevelsViewerKeepAboveHot, v => _viewerKeepAboveHot = v, true);
        _config.OnValueChanged(CVars.NetMaxUpdateRange, v =>
        {
            _viewerPvsRange = v;
            RefreshViewerPreloadTileRadius();
        }, true);
        _config.OnValueChanged(CVars.NetPvsPriorityRange, v =>
        {
            _viewerPvsPriorityRange = v;
            RefreshViewerPreloadTileRadius();
        }, true);

        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<CEZLevelViewerComponent, MapInitEvent>(OnViewerInit);
        SubscribeLocalEvent<CEZLevelViewerComponent, ComponentRemove>(OnCompRemove);

        SubscribeLocalEvent<CEZLevelViewerComponent, MapUidChangedEvent>(OnViewerMapUidChanged);
        SubscribeLocalEvent<CEZPhysicsComponent, CEZLevelFallMapEvent>(OnZLevelFall);
    }

    private void UpdateView(float _)
    {
    }

    private void RefreshViewerPreloadTileRadius()
    {
        var viewSize = Math.Max(_viewerPvsRange, _viewerPvsPriorityRange);
        _viewerPreloadTileRadius = Math.Max(1, (int)Math.Ceiling(viewSize / 2f) + 1);
    }

    private void OnViewerInit(Entity<CEZLevelViewerComponent> ent, ref MapInitEvent args)
    {
        if (!ZLevelsEnabled)
            return;

        _actions.AddAction(ent, ref ent.Comp.ZLevelActionEntity, ent.Comp.ActionProto);
        _meta.AddFlag(ent, MetaDataFlags.ExtraTransformEvents);
        RefreshViewerVisibilityCache(ent, true);
        UpdateViewer(ent, true);
    }

    private void OnCompRemove(Entity<CEZLevelViewerComponent> ent, ref ComponentRemove args)
    {
        _actions.RemoveAction(ent.Comp.ZLevelActionEntity);
        _meta.RemoveFlag(ent, MetaDataFlags.ExtraTransformEvents);
        ClearViewerEyes(ent);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        if (!ZLevelsEnabled)
            return;

        var viewer = EnsureComp<CEZLevelViewerComponent>(ev.Entity);
        UpdateViewer((ev.Entity, viewer), true);
    }

    private void OnPlayerDetached(PlayerDetachedEvent ev)
    {
        RemComp<CEZLevelViewerComponent>(ev.Entity);
    }

    private void OnViewerMapUidChanged(Entity<CEZLevelViewerComponent> ent, ref MapUidChangedEvent args)
    {
        RefreshViewerVisibilityCache(ent, true);
        UpdateViewer(ent, true);
    }

    protected override void OnViewerMove(Entity<CEZLevelViewerComponent> ent, ref MoveEvent args)
    {
        if (!ZLevelsEnabled)
            return;

        base.OnViewerMove(ent, ref args);
        UpdateViewer(ent);
    }

    protected override void OnToggleLookUp(Entity<CEZLevelViewerComponent> ent, ref CEToggleZLevelLookUpAction args)
    {
        if (!ZLevelsEnabled)
        {
            args.Handled = true;
            return;
        }

        base.OnToggleLookUp(ent, ref args);
        UpdateViewer(ent, true);
    }

    private void UpdateViewer(Entity<CEZLevelViewerComponent> ent, bool force = false)
    {
        if (!ZLevelsEnabled)
        {
            ClearViewerEyes(ent);
            return;
        }

        using var _ = ViewerPreloadUsage.NewTimer();

        if (!TryComp<ActorComponent>(ent, out var actor))
        {
            ClearViewerEyes(ent);
            return;
        }

        var xform = Transform(ent);
        if (xform.MapUid is not { } mapUid ||
            !TryComp<CEZLevelMapComponent>(mapUid, out var zMapComp))
        {
            ClearViewerEyes(ent);
            return;
        }

        var map = (mapUid, zMapComp);
        var tile = GetViewerTilePosition(ent);
        var worldPos = _transform.GetWorldPosition(xform);
        var chunk = (worldPos / ViewerChunkSize).Floored();

        RefreshViewerVisibilityCache(ent, force);
        if (ent.Comp.LookUp && ent.Comp.CachedOpaqueAbove)
        {
            ent.Comp.LookUp = false;
            DirtyField(ent, ent.Comp, nameof(CEZLevelViewerComponent.LookUp));
            force = true;
        }

        if (!force &&
            ent.Comp.CachedTile == tile &&
            ent.Comp.CachedChunk == chunk)
            return;

        var repositionEyes = force || ent.Comp.CachedChunk != chunk;
        var belowDepth = GetDesiredBelowPreloadDepth(ent, map, tile);
        var aboveActive = ShouldPreloadAbove(ent, map, tile);

        SyncBelowEyes(ent, actor.PlayerSession, map, worldPos, belowDepth, repositionEyes);
        SyncAboveEye(ent, actor.PlayerSession, map, worldPos, aboveActive, repositionEyes);

        ent.Comp.CachedTile = tile;
        ent.Comp.CachedChunk = chunk;
    }

    private int GetDesiredBelowPreloadDepth(Entity<CEZLevelViewerComponent> ent, Entity<CEZLevelMapComponent> map, Vector2i tile)
    {
        if (_viewerMaxPreloadBelowDepth <= 0 ||
            !ShouldPreloadBelow(ent, map, tile))
            return 0;

        var maxDepth = 0;
        for (var depth = 1; depth <= _viewerMaxPreloadBelowDepth; depth++)
        {
            if (!TryMapOffset((map.Owner, map.Comp), -depth, out _))
                break;

            maxDepth = depth;
        }

        return maxDepth;
    }

    private void OnZLevelFall(Entity<CEZPhysicsComponent> ent, ref CEZLevelFallMapEvent args)
    {
        if (!ZLevelsEnabled)
            return;

        //A dirty trick: we call PredictedPopup on the falling entity on SERVER.
        //This means that the one who is falling does not see the popup itself, but everyone around them does. This is what we need.
        _popup.PopupPredictedCoordinates(Loc.GetString("ce-zlevel-falling-popup", ("name", Identity.Name(ent, EntityManager))), Transform(ent).Coordinates, ent);
    }

    private bool ShouldPreloadBelow(Entity<CEZLevelViewerComponent> ent, Entity<CEZLevelMapComponent> map, Vector2i tile)
    {
        if (!TryMapDown((map.Owner, map.Comp), out _))
            return false;

        // Ghosts can freely inspect z-levels. Keep the lower PVS hot so client-side
        // z-rendering does not draw an empty/dark floor before lower entities arrive.
        if (HasComp<GhostComponent>(ent))
            return true;

        return HasVisibleOpeningToLowerMap((map.Owner, map.Comp), tile) ||
               HasNearbyHighGround((map.Owner, map.Comp), tile) ||
               IsTransitioning(ent);
    }

    private bool ShouldPreloadAbove(Entity<CEZLevelViewerComponent> ent, Entity<CEZLevelMapComponent> map, Vector2i tile)
    {
        if (!TryMapUp((map.Owner, map.Comp), out _))
            return false;

        return _viewerKeepAboveHot ||
               ent.Comp.LookUp ||
               HasNearbyHighGround((map.Owner, map.Comp), tile) ||
               IsAscending(ent);
    }

    private bool IsTransitioning(EntityUid uid)
    {
        return TryComp<CEZPhysicsComponent>(uid, out var zPhys) &&
               (Math.Abs(zPhys.LocalPosition) > 0.01f || Math.Abs(zPhys.Velocity) > 0.01f);
    }

    private bool IsAscending(EntityUid uid)
    {
        return TryComp<CEZPhysicsComponent>(uid, out var zPhys) &&
               (zPhys.LocalPosition > 0.01f || zPhys.Velocity > 0.01f);
    }

    private bool HasNearbyHighGround(Entity<CEZLevelMapComponent> map, Vector2i center)
    {
        if (!TryComp<MapGridComponent>(map.Owner, out var grid))
            return false;

        for (var x = center.X - 1; x <= center.X + 1; x++)
        {
            for (var y = center.Y - 1; y <= center.Y + 1; y++)
            {
                var query = _map.GetAnchoredEntitiesEnumerator(map.Owner, grid, new Vector2i(x, y));
                while (query.MoveNext(out var uid))
                {
                    if (HasComp<CEZLevelHighGroundComponent>(uid))
                        return true;
                }
            }
        }

        return false;
    }

    private bool HasVisibleOpeningToLowerMap(Entity<CEZLevelMapComponent> map, Vector2i center)
    {
        if (!TryComp<MapGridComponent>(map.Owner, out var grid))
            return true;

        var radius = _viewerPreloadTileRadius;
        for (var x = center.X - radius; x <= center.X + radius; x++)
        {
            for (var y = center.Y - radius; y <= center.Y + radius; y++)
            {
                if (!_map.TryGetTileRef(map.Owner, grid, new Vector2i(x, y), out var tileRef))
                    return true;

                if (tileRef.Tile.IsEmpty)
                    return true;

                var tileDef = (ContentTileDefinition)TilDefMan[tileRef.Tile.TypeId];
                if (tileDef.Transparent)
                    return true;
            }
        }

        return false;
    }

    private void SyncBelowEyes(
        Entity<CEZLevelViewerComponent> ent,
        ICommonSession session,
        Entity<CEZLevelMapComponent> map,
        Vector2 worldPos,
        int desiredDepth,
        bool reposition)
    {
        while (ent.Comp.BelowEyes.Count > desiredDepth)
        {
            var eye = ent.Comp.BelowEyes[^1];
            ent.Comp.BelowEyes.RemoveAt(ent.Comp.BelowEyes.Count - 1);
            QueueDel(eye);
        }

        for (var depth = 1; depth <= desiredDepth; depth++)
        {
            if (!TryMapOffset((map.Owner, map.Comp), -depth, out var belowMap))
                break;

            while (ent.Comp.BelowEyes.Count < depth)
            {
                var newEye = SpawnAuxiliaryEye(session, belowMap.Owner, worldPos);
                ent.Comp.BelowEyes.Add(newEye);
            }

            if (reposition)
                MoveAuxiliaryEye(ent.Comp.BelowEyes[depth - 1], belowMap.Owner, worldPos);
        }
    }

    private void SyncAboveEye(
        Entity<CEZLevelViewerComponent> ent,
        ICommonSession session,
        Entity<CEZLevelMapComponent> map,
        Vector2 worldPos,
        bool active,
        bool reposition)
    {
        if (!active || !TryMapUp((map.Owner, map.Comp), out var aboveMap))
        {
            if (ent.Comp.AboveEye is { } aboveEye)
            {
                QueueDel(aboveEye);
                ent.Comp.AboveEye = null;
            }

            return;
        }

        if (ent.Comp.AboveEye is not { } eye)
            ent.Comp.AboveEye = eye = SpawnAuxiliaryEye(session, aboveMap.Owner, worldPos);

        if (reposition)
            MoveAuxiliaryEye(eye, aboveMap.Owner, worldPos);
    }

    private EntityUid SpawnAuxiliaryEye(ICommonSession session, EntityUid targetMap, Vector2 worldPos)
    {
        var eye = SpawnAtPosition(_zEyeProto, new EntityCoordinates(targetMap, worldPos));
        Transform(eye).GridTraversal = false;
        _viewSubscriber.AddViewSubscriber(eye, session);
        return eye;
    }

    private void MoveAuxiliaryEye(EntityUid eye, EntityUid targetMap, Vector2 worldPos)
    {
        if (!TryComp<MapComponent>(targetMap, out var mapComp))
            return;

        _transform.SetMapCoordinates(eye, new MapCoordinates(worldPos, mapComp.MapId));
    }

    private void ClearViewerEyes(Entity<CEZLevelViewerComponent> ent)
    {
        foreach (var eye in ent.Comp.BelowEyes)
        {
            if (!TerminatingOrDeleted(eye))
                QueueDel(eye);
        }

        ent.Comp.BelowEyes.Clear();

        if (ent.Comp.AboveEye is { } aboveEye && !TerminatingOrDeleted(aboveEye))
            QueueDel(aboveEye);

        ent.Comp.AboveEye = null;
        ent.Comp.CachedChunk = null;
    }
}
