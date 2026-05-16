/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Numerics;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared.GameTicking;
using Content.Shared._MC;
using Content.Shared.Camera;
using Content.Shared.Damage.Components;
using Robust.Shared.Configuration;
using Robust.Shared.GameStates;
using Robust.Client.Player;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;

namespace Content.Client._CE.ZLevels.Core;

/// <summary>
/// Only process Eye offset and drawdepth on clientside
/// </summary>
public sealed partial class CEClientZLevelsSystem : CESharedZLevelsSystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    private readonly HashSet<EntityUid> _trackedVisuals = new();
    private readonly List<EntityUid> _trackedVisualSnapshot = new();
    private readonly List<EntityUid> _trackedVisualRemovals = new();
    private readonly Dictionary<EntityUid, int> _visibilityRevision = new();
    private CEZLevelBlurOverlay? _lowerFxOverlay;
    private CEZLevelLowerFxMode _lowerFxMode = CEZLevelLowerFxMode.Tint;
    private int _maxRenderBelowDepth = 1;

    public static float ZLevelOffset = 0.5f;
    public int MaxRenderedBelowDepth => _maxRenderBelowDepth;

    public override void Initialize()
    {
        base.Initialize();
        _lowerFxOverlay = new CEZLevelBlurOverlay();
        if (ZLevelsEnabled)
            _overlay.AddOverlay(_lowerFxOverlay);

        _config.OnValueChanged(MCConfigVars.ZLevelsRenderMaxBelowDepth, v => _maxRenderBelowDepth = Math.Max(0, v), true);
        _config.OnValueChanged(MCConfigVars.ZLevelsRenderLowerFx, OnLowerFxChanged, true);

        SubscribeLocalEvent<CEZPhysicsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CEZPhysicsComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<CEZPhysicsComponent, GetEyeOffsetEvent>(OnEyeOffset);
        SubscribeLocalEvent<CEZPhysicsComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnLocalPlayerDetached);
        SubscribeNetworkEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
    }

    private void OnEyeOffset(Entity<CEZPhysicsComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (!ZLevelsEnabled)
            return;

        Angle rotation = _eye.CurrentEye.Rotation * -1;
        var localPosition = GetVisualsLocalPosition((ent.Owner, ent.Comp), Transform(ent));
        var offset = rotation.RotateVec(new Vector2(0, localPosition * ZLevelOffset));
        args.Offset += offset;
    }

    private void OnStartup(Entity<CEZPhysicsComponent> ent, ref ComponentStartup args)
    {
        if (!ZLevelsEnabled)
            return;

        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (sprite.SnapCardinals)
            return;

        ent.Comp.NoRotDefault = sprite.NoRotation;
        ent.Comp.DrawDepthDefault = sprite.DrawDepth;
        ent.Comp.SpriteOffsetDefault = sprite.Offset;
        RefreshTrackedVisualState(ent, sprite);
    }

    private void OnRemove(Entity<CEZPhysicsComponent> ent, ref ComponentRemove args)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
            RestoreVisualDefaults(ent, sprite);

        _trackedVisuals.Remove(ent);
    }

    protected override void OnParentChanged(Entity<CEZPhysicsComponent> ent, ref EntParentChangedMessage args)
    {
        base.OnParentChanged(ent, ref args);
        if (!ZLevelsEnabled)
            return;

        RefreshTrackedVisualState(ent);
    }

    private void OnAfterHandleState(Entity<CEZPhysicsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!ZLevelsEnabled)
            return;

        RefreshTrackedVisualState(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!ZLevelsEnabled)
            return;

        if (_player.LocalEntity is not { } localEntity)
        {
            if (_trackedVisuals.Count > 0)
                RestoreAllTrackedVisuals();
            return;
        }

        var localXform = Transform(localEntity);
        if (localXform.MapUid is not { } mapUid || !HasComp<CEZLevelMapComponent>(mapUid))
        {
            if (_trackedVisuals.Count > 0)
                RestoreAllTrackedVisuals();
            return;
        }

        if (TryComp<CEZPhysicsComponent>(localEntity, out var localZPhys))
            RefreshTrackedVisualState((localEntity, localZPhys));

        _trackedVisualRemovals.Clear();
        _trackedVisualSnapshot.Clear();
        _trackedVisualSnapshot.AddRange(_trackedVisuals);
        foreach (var uid in _trackedVisualSnapshot)
        {
            if (TerminatingOrDeleted(uid) ||
                !TryComp<CEZPhysicsComponent>(uid, out var zPhys) ||
                !TryComp<SpriteComponent>(uid, out var sprite) ||
                !TryComp<TransformComponent>(uid, out var xform))
            {
                _trackedVisualRemovals.Add(uid);
                continue;
            }

            var localPosition = GetVisualsLocalPosition((uid, zPhys), xform);

            // Only update if values actually changed to reduce redundant operations
            var newNoRotation = localPosition != 0 || zPhys.NoRotDefault;
            sprite.NoRotation = newNoRotation;

            var newOffset = zPhys.SpriteOffsetDefault + new Vector2(0, localPosition * ZLevelOffset);
            if (sprite.Offset != newOffset)
                _sprite.SetOffset((uid, sprite), newOffset);

            var newDrawDepth = localPosition > 0 ? (int)Shared.DrawDepth.DrawDepth.OverMobs : zPhys.DrawDepthDefault;
            if (sprite.DrawDepth != newDrawDepth)
                _sprite.SetDrawDepth((uid, sprite), newDrawDepth);

            if (!ShouldTrackVisualState((uid, zPhys), sprite, xform, localPosition))
            {
                RestoreVisualDefaults((uid, zPhys), sprite);
                _trackedVisualRemovals.Add(uid);
            }
        }

        foreach (var uid in _trackedVisualRemovals)
        {
            _trackedVisuals.Remove(uid);
        }

        _trackedVisualSnapshot.Clear();

        // Update StartOffset for entities with running fatigue animations
        // This allows animations to follow dynamic offset changes (e.g., from Z-levels system)
        /*
        var query2 = EntityQueryEnumerator<StaminaComponent, SpriteComponent, CEZPhysicsComponent>();
        while (query2.MoveNext(out var uid, out var stamina, out var sprite, out var zPhys))
        {
            // Only update if animation is running
            if (!_animation.HasRunningAnimation(uid, StaminaSystem.StaminaAnimationKey))
                continue;

            // Update the base offset to track changes made by other systems
            stamina.StartOffset = zPhys.SpriteOffsetDefault;
        }
        */
    }


    public float GetVisualsLocalPosition(Entity<CEZPhysicsComponent> ent, TransformComponent? xform = null)
    {
        if (!ZLevelsEnabled)
            return 0;

        if (!Resolve(ent, ref xform, false))
            return 0;

        var pos = ent.Comp.LocalPosition;

        if (xform.ParentUid != xform.MapUid && ZPhysicsQuery.TryComp(xform.ParentUid, out var parentZPhys))
            pos = parentZPhys.LocalPosition;

        if (ent.Comp.CachedStickyGround)
            return 0;

        return pos;
    }

    public int GetVisibilityRevision(EntityUid mapUid)
    {
        if (!ZLevelsEnabled)
            return 0;

        return _visibilityRevision.GetValueOrDefault(mapUid, 0);
    }

    protected override void OnTileChanged(Entity<CEZLevelMapComponent> ent, ref TileChangedEvent args)
    {
        base.OnTileChanged(ent, ref args);

        if (!ZLevelsEnabled)
            return;

        _visibilityRevision[ent.Owner] = _visibilityRevision.GetValueOrDefault(ent.Owner) + 1;
    }

    protected override void OnZMapShutdown(Entity<CEZLevelMapComponent> ent, ref ComponentShutdown args)
    {
        base.OnZMapShutdown(ent, ref args);
        _visibilityRevision.Remove(ent.Owner);
    }

    private void OnLocalPlayerDetached(LocalPlayerDetachedEvent args)
    {
        ClearClientRuntimeState();
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        ClearClientRuntimeState();
    }

    protected override void OnZLevelsEnabledChanged(bool enabled)
    {
        base.OnZLevelsEnabledChanged(enabled);

        if (_lowerFxOverlay == null)
            return;

        if (enabled)
        {
            if (!_overlay.HasOverlay<CEZLevelBlurOverlay>())
                _overlay.AddOverlay(_lowerFxOverlay);

            return;
        }

        if (_overlay.HasOverlay<CEZLevelBlurOverlay>())
            _overlay.RemoveOverlay(_lowerFxOverlay);

        ClearClientRuntimeState();
    }

    private void RefreshTrackedVisualState(Entity<CEZPhysicsComponent> ent, SpriteComponent? sprite = null, TransformComponent? xform = null)
    {
        if (!Resolve(ent, ref sprite, false) ||
            !Resolve(ent, ref xform, false))
            return;

        if (sprite.SnapCardinals)
        {
            _trackedVisuals.Remove(ent);
            return;
        }

        if (ShouldTrackVisualState(ent, sprite, xform))
        {
            _trackedVisuals.Add(ent);
            return;
        }

        RestoreVisualDefaults(ent, sprite);
        _trackedVisuals.Remove(ent);
    }

    private bool ShouldTrackVisualState(
        Entity<CEZPhysicsComponent> ent,
        SpriteComponent sprite,
        TransformComponent xform,
        float? localPositionOverride = null)
    {
        var localPosition = localPositionOverride ?? GetVisualsLocalPosition(ent, xform);
        return Math.Abs(localPosition) > 0.001f ||
               sprite.NoRotation != ent.Comp.NoRotDefault ||
               sprite.DrawDepth != ent.Comp.DrawDepthDefault ||
               sprite.Offset != ent.Comp.SpriteOffsetDefault;
    }

    private void RestoreVisualDefaults(Entity<CEZPhysicsComponent> ent, SpriteComponent sprite)
    {
        sprite.NoRotation = ent.Comp.NoRotDefault;
        _sprite.SetOffset((ent.Owner, sprite), ent.Comp.SpriteOffsetDefault);
        _sprite.SetDrawDepth((ent.Owner, sprite), ent.Comp.DrawDepthDefault);
    }

    private void RestoreAllTrackedVisuals()
    {
        _trackedVisualSnapshot.Clear();
        _trackedVisualSnapshot.AddRange(_trackedVisuals);

        foreach (var uid in _trackedVisualSnapshot)
        {
            if (TryComp<CEZPhysicsComponent>(uid, out var zPhys) &&
                TryComp<SpriteComponent>(uid, out var sprite))
            {
                RestoreVisualDefaults((uid, zPhys), sprite);
            }
        }

        _trackedVisuals.Clear();
        _trackedVisualSnapshot.Clear();
        _trackedVisualRemovals.Clear();
        _visibilityRevision.Clear();
    }

    private void ClearClientRuntimeState()
    {
        if (_trackedVisuals.Count > 0)
            RestoreAllTrackedVisuals();
        else
            _visibilityRevision.Clear();
    }

    private void OnLowerFxChanged(string value)
    {
        if (!Enum.TryParse(value, true, out CEZLevelLowerFxMode mode))
            mode = CEZLevelLowerFxMode.Tint;

        _lowerFxMode = mode;

        if (_lowerFxOverlay != null)
            _lowerFxOverlay.Mode = _lowerFxMode;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        ClearClientRuntimeState();
        if (_lowerFxOverlay != null)
            _overlay.RemoveOverlay(_lowerFxOverlay);

        _trackedVisuals.Clear();
        _trackedVisualSnapshot.Clear();
        _trackedVisualRemovals.Clear();
        _visibilityRevision.Clear();
    }
}
