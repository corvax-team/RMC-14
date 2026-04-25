// CM14 rework: non-RMC edit marker.
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Content.IntegrationTests;
using Content.IntegrationTests.Pair;
using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;

namespace Content.Benchmarks;

[Virtual]
public sealed class EntityManagerGetAllComponents
{
    private TestPair _pair = default!;
    private IEntityManager _entityManager = default!;

    [Params(5000)]
    public int N { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        PoolManager.Startup();
        _pair = await PoolManager.GetServerClient();
        _entityManager = _pair.Server.ResolveDependency<IEntityManager>();

        await _pair.Server.WaitPost(() =>
        {
            for (var i = 0; i < N; i++)
            {
                _entityManager.SpawnEntity(null, MapCoordinates.Nullspace);
            }
        });
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _pair.DisposeAsync();
        PoolManager.Shutdown();
    }

    [Benchmark]
    public int Run()
    {
        var count = 0;
        foreach (var _ in _entityManager.EntityQuery<TransformComponent>(true))
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int Noop()
    {
        _entityManager.TryGetComponent<TransformComponent>(default, out _);
        return 0;
    }
}
