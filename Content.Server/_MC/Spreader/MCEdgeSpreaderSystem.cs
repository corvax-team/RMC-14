using Content.Server.Atmos.Components;
using Content.Shared._MC.Spreader;
using Content.Shared.Tag;
using Robust.Shared.Collections;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._MC.Spreader;

public sealed class MCEdgeSpreaderSystem : EntitySystem
{
    private const long SpreadDelayMultiplier = 2;
    private const string SmokeTag = "MCSmoke";
    private static readonly Vector2i[] Directions = [Vector2i.Up, Vector2i.Right, Vector2i.Down, Vector2i.Left];

    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly TagSystem _tag = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;

    private readonly Dictionary<(string PrototypeId, EntityUid GridUid, Vector2i Tile), SpawnDeferredEntry> _spawnDeferredEntries = [];

    private EntityQuery<AirtightComponent> _airtightQuery;

    public override void Initialize()
    {
        base.Initialize();

        _airtightQuery = GetEntityQuery<AirtightComponent>();
    }

    public override void Update(float frameTime)
    {
        var currentTime = _timing.CurTime;
        var query = EntityQueryEnumerator<MCEdgeSpreaderComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var component, out var xform))
        {
            if (component.NextUpdate == TimeSpan.Zero)
            {
                component.NextUpdate = currentTime + GetSpreadDelay(component);
                continue;
            }

            if (component.NextUpdate > currentTime)
                continue;

            if (component.Range <= 0 || xform.GridUid is null)
            {
                RemCompDeferred<MCEdgeSpreaderComponent>(uid);
                continue;
            }

            GetFreeTiles(uid, xform, out var freeTiles);
            if (freeTiles.Count == 0)
            {
                RemCompDeferred<MCEdgeSpreaderComponent>(uid);
                continue;
            }

            if (MetaData(uid).EntityPrototype?.ID is not { } prototypeId)
            {
                RemCompDeferred<MCEdgeSpreaderComponent>(uid);
                continue;
            }

            var nextUpdate = currentTime + GetSpreadDelay(component);
            foreach (var freeTile in freeTiles)
            {
                QueueSpawn(prototypeId, xform.GridUid.Value, freeTile, component.Range - 1, nextUpdate);
            }

            RemCompDeferred<MCEdgeSpreaderComponent>(uid);
        }

        foreach (var entry in _spawnDeferredEntries.Values)
        {
            if (!TryComp<MapGridComponent>(entry.GridUid, out var grid))
                continue;

            var uid = Spawn(entry.PrototypeId, _map.GridTileToLocal(entry.GridUid, grid, entry.Tile));
            var spreader = EnsureComp<MCEdgeSpreaderComponent>(uid);

            spreader.Range = entry.Range;
            spreader.NextUpdate = entry.NextUpdate;
        }

        _spawnDeferredEntries.Clear();
    }

    private void QueueSpawn(string prototypeId, EntityUid gridUid, Vector2i tile, int range, TimeSpan nextUpdate)
    {
        var key = (prototypeId, gridUid, tile);
        if (_spawnDeferredEntries.TryGetValue(key, out var existing))
        {
            existing.Range = Math.Max(existing.Range, range);
            if (nextUpdate < existing.NextUpdate)
                existing.NextUpdate = nextUpdate;

            _spawnDeferredEntries[key] = existing;
            return;
        }

        _spawnDeferredEntries[key] = new SpawnDeferredEntry(prototypeId, gridUid, tile, range, nextUpdate);
    }

    private void GetFreeTiles(EntityUid uid, TransformComponent transformComponent, out ValueList<Vector2i> freeTiles)
    {
        freeTiles = [];

        if (!TryComp<MapGridComponent>(transformComponent.GridUid, out var grid))
            return;

        var tile = _map.TileIndicesFor(transformComponent.GridUid.Value, grid, transformComponent.Coordinates);
        foreach (var direction in  Directions)
        {
            var neighborPos = tile + direction;

            if (!_map.TryGetTileRef(transformComponent.GridUid.Value, grid, neighborPos, out var tileRef) || tileRef.Tile.IsEmpty)
                continue;

            if (SpaceBlocked((transformComponent.GridUid.Value, grid), neighborPos))
                continue;

            freeTiles.Add(neighborPos);
        }
    }

    private bool SpaceBlocked(Entity<MapGridComponent> grid, Vector2i pos)
    {
        var entities = _map.GetAnchoredEntitiesEnumerator(grid, grid, pos);
        while (entities.MoveNext(out var ent))
        {
            if (_tag.HasTag(ent.Value, SmokeTag))
                return true;

            if (_airtightQuery.TryGetComponent(ent, out var airtight) && airtight.AirBlocked)
                return true;
        }

        return false;
    }

    private static TimeSpan GetSpreadDelay(MCEdgeSpreaderComponent component)
    {
        return TimeSpan.FromTicks(component.Delay.Ticks * SpreadDelayMultiplier);
    }

    private struct SpawnDeferredEntry(string prototypeId, EntityUid gridUid, Vector2i tile, int range, TimeSpan nextUpdate)
    {
        public string PrototypeId = prototypeId;
        public EntityUid GridUid = gridUid;
        public Vector2i Tile = tile;
        public int Range = range;
        public TimeSpan NextUpdate = nextUpdate;
    }
}
