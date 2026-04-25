using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._CCM.CommunicationsConsole;
using Content.Shared._CCM.CommunicationsConsole.Components;
using Content.Shared._CCM.CommunicationsConsole.ERT;
using Content.Shared._CCM.CommunicationsConsole.UI;
using Content.Shared._RMC14.Marines.Announce;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._CCM.CommunicationsConsole;

public sealed class CCMCommunicationsConsoleSystem : CCMSharedCommunicationsConsoleSystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedMarineAnnounceSystem _marineAnnounce = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;

    protected override void OnRunMessage(Entity<CCMCommunicationsConsoleComponent> entity,
        ref CCMCommunicationsConsoleERTCallBuiMessage args)
    {
        if (entity.Comp.ERTCalled)
            return;

        entity.Comp.ERTCalled = true;
        Dirty(entity);

        _marineAnnounce.AnnounceHighCommand(Loc.GetString("ert-announce-text"), Loc.GetString("ert-announce-author"));

        SpawnERTMap(entity.Comp.MapPaths);
        CrashERTShuttle(entity.Comp.FTLFlyTime);
    }

    private void CrashERTShuttle(TimeSpan flyTime)
    {
        var points = new List<(EntityUid Uid, CCMERTCrashMarkerComponent Comp)>();
        var pointQuery = EntityQueryEnumerator<CCMERTCrashMarkerComponent>();
        while (pointQuery.MoveNext(out var uid, out var comp))
        {
            points.Add((uid, comp));
        }

        if (points.Count == 0)
            return;

        var point = _random.Pick(points);
        var pointUid = point.Uid;

        var query = EntityQueryEnumerator<CCMERTShuttleComponent, ShuttleComponent>();
        while (query.MoveNext(out var uid, out _, out var shuttle))
        {
            _shuttle.FTLToCoordinates(
                uid,
                shuttle,
                Transform(pointUid).Coordinates.Offset(point.Comp.Offset),
                Angle.Zero,
                hyperspaceTime: (float)flyTime.TotalSeconds
            );
            return;
        }
    }

    private void SpawnERTMap(List<ResPath> mapPaths)
    {
        if (mapPaths.Count == 0)
            return;

        var selectedMapPath = _random.Pick(mapPaths);

        if (!_mapLoader.TryLoadMap(selectedMapPath, out var mapUid, out _))
            return;

        if (!TryComp(mapUid.Value, out MapComponent? mapComp))
            return;

        _mapSystem.InitializeMap(mapComp.MapId);
    }
}
// thanks to _gadmin1 (discord) for the provided code