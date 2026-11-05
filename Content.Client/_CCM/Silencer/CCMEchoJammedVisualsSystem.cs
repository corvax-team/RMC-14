using Content.Shared._CCM.Silencer;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.NightVision;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Examine;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Map;

namespace Content.Client._CCM.Silencer;

public sealed class CCMEchoJammedVisualsSystem : EntitySystem
{
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private const float MaxCheckRange = 30f;

    private EntityQuery<XenoComponent> _xenoQuery;
    private EntityQuery<NightVisionComponent> _nightVisionQuery;
    private EntityQuery<CCMEchoJammedComponent> _jammedQuery;
    private EntityQuery<MarineComponent> _marineQuery;

    private readonly List<(MapCoordinates Center, float Half, EntityUid Wearer)> _zones = new();

    private CCMEchoJammerOverlay? _jammerOverlay;

    public override void Initialize()
    {
        _xenoQuery = GetEntityQuery<XenoComponent>();
        _nightVisionQuery = GetEntityQuery<NightVisionComponent>();
        _jammedQuery = GetEntityQuery<CCMEchoJammedComponent>();
        _marineQuery = GetEntityQuery<MarineComponent>();

        _jammerOverlay = new CCMEchoJammerOverlay();
        _overlay.AddOverlay(_jammerOverlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        if (_jammerOverlay != null)
            _overlay.RemoveOverlay(_jammerOverlay);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        _zones.Clear();
        var query = EntityQueryEnumerator<CCMEchoJammerComponent, TransformComponent>();
        while (query.MoveNext(out _, out var jammer, out var xform))
        {
            if (!jammer.Enabled)
                continue;

            var wearer = xform.ParentUid;
            _zones.Add((_transform.GetMapCoordinates(xform), jammer.HalfExtent, wearer));
        }
    }

    public bool LocalIsXenoWithNightVision()
    {
        if (_player.LocalEntity is not { } local || !_xenoQuery.HasComp(local))
            return false;

        return _nightVisionQuery.TryComp(local, out var nightVision) &&
               nightVision.State != NightVisionState.Off;
    }

    /// <summary>
    ///     True if the local xeno must not see this entity through night vision:
    ///     it is inside an active Echo zone and not in direct line of sight.
    /// </summary>
    public bool IsHiddenFromNightVision(EntityUid uid, TransformComponent xform)
    {
        if (_zones.Count == 0 && !_jammedQuery.HasComp(uid))
            return false;

        if (_player.LocalEntity is not { } local || !_xenoQuery.HasComp(local))
            return false;

        var pos = _transform.GetMapCoordinates(uid, xform);
        if (!IsInJammedZone(uid, pos))
            return false;

        return !IsVisibleFromEye(pos);
    }

    private bool IsInJammedZone(EntityUid uid, MapCoordinates pos)
    {
        if (_jammedQuery.HasComp(uid))
            return true;

        var isTarget = _marineQuery.HasComp(uid) || _xenoQuery.HasComp(uid);
        foreach (var (center, half, wearer) in _zones)
        {
            if (!isTarget && uid != wearer)
                continue;

            if (center.MapId != pos.MapId)
                continue;

            if (MathF.Abs(pos.Position.X - center.Position.X) <= half &&
                MathF.Abs(pos.Position.Y - center.Position.Y) <= half)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsVisibleFromEye(MapCoordinates target)
    {
        var eye = _eye.CurrentEye.Position;
        if (eye.MapId != target.MapId || eye.MapId == MapId.Nullspace)
            return false;

        if ((target.Position - eye.Position).LengthSquared() > MaxCheckRange * MaxCheckRange)
            return false;

        return _examine.InRangeUnOccluded(eye, target, MaxCheckRange, null);
    }
}
