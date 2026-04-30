/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Server._CE.ZLevels.Core;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Roof;
using Content.Shared.Light.Components;
using Content.Shared.Maps;

namespace Content.Server._CE.ZLevels.Roof;

/// <inheritdoc/>
public sealed class CERoofSystem : CESharedRoofSystem
{
    private readonly HashSet<Vector2i> _roofMap = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CEZLevelsNetworkComponent, CEZLevelNetworkUpdatedEvent>(OnNetworkUpdated);
    }

    private void OnNetworkUpdated(Entity<CEZLevelsNetworkComponent> ent, ref CEZLevelNetworkUpdatedEvent args)
    {
        RecalculateNetworkRoofs(ent);
    }

    public void RecalculateNetworkRoofs(Entity<CEZLevelsNetworkComponent> network)
    {
        _roofMap.Clear();

        // Use pre-sorted SortedZLevels instead of sorting ZLevels every time
        // Iterate in reverse order (highest depth first) to properly calculate roof inheritance
        for (var i = network.Comp.SortedZLevels.Count - 1; i >= 0; i--)
        {
            var mapUid = network.Comp.SortedZLevels[i];
            if (mapUid == EntityUid.Invalid)
                continue;

            if (!GridQuery.TryComp(mapUid, out var mapGrid))
                continue;

            var enumerator = Map.GetAllTilesEnumerator(mapUid, mapGrid);
            var roofComp = EnsureComp<RoofComponent>(mapUid);

            while (enumerator.MoveNext(out var tileRef))
            {
                Roof.SetRoof((mapUid, mapGrid, roofComp), tileRef.Value.GridIndices, _roofMap.Contains(tileRef.Value.GridIndices));

                var tileDef = (ContentTileDefinition)TilDefMan[tileRef.Value.Tile.TypeId];

                if (!tileDef.Transparent)
                    _roofMap.Add(tileRef.Value.GridIndices);
            }
        }
    }
}
