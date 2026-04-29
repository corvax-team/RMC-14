// CM14 rework: non-RMC edit marker.
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Content.IntegrationTests;
using Content.IntegrationTests.Pair;
using Content.Server._CE.ZLevels.Core;
using Content.Server.Mind;
using Content.Server.Warps;
using Content.Shared._MC;
using Content.Shared.Warps;
using Robust.Shared;
using Robust.Shared.Analyzers;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Benchmarks;

[Virtual]
public class ZLevelPvsBenchmark
{
    public const string Map = "Maps/box.yml";

    [Params(80, 160)]
    public int PlayerCount { get; set; }

    [Params(ZLevelViewerMode.Off, ZLevelViewerMode.Default, ZLevelViewerMode.Legacy)]
    public ZLevelViewerMode ViewerMode { get; set; }

    private TestPair _pair = default!;
    private IEntityManager _entMan = default!;
    private SharedTransformSystem _xform = default!;
    private ICommonSession[] _players = default!;
    private EntityCoordinates[] _locations = default!;
    private int _cycleOffset;

    [GlobalSetup]
    public void Setup()
    {
        PoolManager.Startup();

        _pair = PoolManager.GetServerClient().GetAwaiter().GetResult();
        _entMan = _pair.Server.ResolveDependency<IEntityManager>();
        _xform = _entMan.System<SharedTransformSystem>();

        _pair.Server.CfgMan.SetCVar(CVars.NetPVS, true);
        _pair.Server.CfgMan.SetCVar(CVars.ThreadParallelCount, 0);
        _pair.Server.CfgMan.SetCVar(CVars.NetPvsAsync, false);
        _pair.Server.CfgMan.SetCVar(MCConfigVars.ZLevelsViewerMaxPreloadBelowDepth, GetBelowDepth(ViewerMode));
        _pair.Server.CfgMan.SetCVar(MCConfigVars.ZLevelsViewerKeepAboveHot, ViewerMode == ZLevelViewerMode.Legacy);

        SetupAsync().Wait();
    }

    private async Task SetupAsync()
    {
        _pair.Server.ResolveDependency<IRobustRandom>().SetSeed(42);

        await _pair.Server.WaitPost(() =>
        {
            var opts = DeserializationOptions.Default with { InitializeMaps = true };
            var mapLoader = _entMan.System<MapLoaderSystem>();
            var zLevels = _entMan.System<CEZLevelsSystem>();

            if (!mapLoader.TryLoadMap(new ResPath(Map), out var middleMap, out _, opts) ||
                !mapLoader.TryLoadMap(new ResPath(Map), out var belowMap, out _, opts) ||
                !mapLoader.TryLoadMap(new ResPath(Map), out var aboveMap, out _, opts))
            {
                throw new Exception("Z-level benchmark map load failed");
            }

            var network = zLevels.CreateZNetwork();
            var maps = new Dictionary<EntityUid, int>
            {
                [belowMap.Value] = -1,
                [middleMap.Value] = 0,
                [aboveMap.Value] = 1,
            };

            if (!zLevels.TryAddMapsIntoZNetwork(network, maps))
                throw new Exception("Failed to create z-level benchmark network");
        });

        var spawns = _entMan.AllComponentsList<WarpPointComponent>()
            .OrderBy(x => x.Component.Location)
            .Select(x => _entMan.GetComponent<TransformComponent>(x.Uid).Coordinates)
            .ToArray();

        if (spawns.Length == 0)
            throw new Exception("No benchmark warp spawns found");

        _players = await _pair.Server.AddDummySessions(PlayerCount);
        await _pair.Server.WaitPost(() =>
        {
            var mind = _pair.Server.System<MindSystem>();
            for (var i = 0; i < PlayerCount; i++)
            {
                var pos = spawns[i % spawns.Length];
                var uid = _entMan.SpawnEntity("MobHuman", pos);
                _pair.Server.ConsoleHost.ExecuteCommand($"setoutfit {_entMan.GetNetEntity(uid)} CaptainGear");
                mind.ControlMob(_players[i].UserId, uid);
            }
        });

        ShufflePlayers(new Random(42), 50);

        _pair.Server.PvsTick(_players);
        _pair.Server.PvsTick(_players);

        var ents = _players.Select(x => x.AttachedEntity!.Value).ToArray();
        _locations = ents.Select(x => _entMan.GetComponent<TransformComponent>(x).Coordinates).ToArray();
    }

    private static int GetBelowDepth(ZLevelViewerMode mode)
    {
        return mode switch
        {
            ZLevelViewerMode.Off => 0,
            ZLevelViewerMode.Default => 1,
            ZLevelViewerMode.Legacy => 3,
            _ => 1,
        };
    }

    private void ShufflePlayers(Random rng, int count)
    {
        while (count-- > 0)
        {
            ShufflePlayers(rng);
        }
    }

    private void ShufflePlayers(Random rng)
    {
        _pair.Server.PvsTick(_players);

        var ents = _players.Select(x => x.AttachedEntity!.Value).ToArray();
        var locations = ents.Select(x => _entMan.GetComponent<TransformComponent>(x).Coordinates).ToArray();

        var n = locations.Length;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (locations[k], locations[n]) = (locations[n], locations[k]);
        }

        _pair.Server.WaitPost(() =>
        {
            for (var i = 0; i < PlayerCount; i++)
            {
                _xform.SetCoordinates(ents[i], locations[i]);
            }
        }).Wait();

        _pair.Server.PvsTick(_players);
    }

    [Benchmark]
    public void StaticTick()
    {
        _pair.Server.PvsTick(_players);
    }

    [Benchmark]
    public void CycleTick()
    {
        _cycleOffset = (_cycleOffset + 1) % _players.Length;
        _pair.Server.WaitPost(() =>
        {
            for (var i = 0; i < PlayerCount; i++)
            {
                _xform.SetCoordinates(_players[i].AttachedEntity!.Value, _locations[(i + _cycleOffset) % _players.Length]);
            }
        }).Wait();

        _pair.Server.PvsTick(_players);
    }
}

public enum ZLevelViewerMode
{
    Off,
    Default,
    Legacy,
}
