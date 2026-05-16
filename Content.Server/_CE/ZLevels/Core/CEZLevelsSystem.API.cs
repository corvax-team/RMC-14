/*
 * Copyright (c) 2026 TornadgoTechnology
 * Copyright (c) 2026 CrystallEdge (https://github.com/crystallpunk-14/crystall-edge)
 *
 * SPDX-License-Identifier: PolyForm-Noncommercial-1.0.0 AND MIT
 */

using System.Linq;
using Content.Server._CE.PVS;
using Content.Shared._CE.ZLevels.Core.Components;
using JetBrains.Annotations;

namespace Content.Server._CE.ZLevels.Core;

public sealed partial class CEZLevelsSystem
{
    /// <summary>
    /// Creates a new entity zLevelNetwork
    /// </summary>
    [PublicAPI]
    public Entity<CEZLevelsNetworkComponent> CreateZNetwork()
    {
        var ent = Spawn();

        var zLevel = EnsureComp<CEZLevelsNetworkComponent>(ent);
        EnsureComp<CEPvsOverrideComponent>(ent);

        return (ent, zLevel);
    }

    /// <summary>
    /// Attempts to add the specified map to the zNetwork network at the specified depth
    /// </summary>
    private bool TryAddMapIntoZNetwork(Entity<CEZLevelsNetworkComponent> network, EntityUid mapUid, int depth)
    {
        if (!ZLevelsEnabled)
            return false;

        if (TryGetZNetwork(mapUid, out var otherNetwork))
        {
            Log.Error($"Failed attempt to add map {mapUid} to ZLevelNetwork {network}: This map is already in another network {otherNetwork}.");
            return false;
        }

        if (network.Comp.ZLevels.ContainsKey(depth))
        {
            Log.Error($"Failed to add map {mapUid} to ZLevelNetwork {network}: This depth is already occupied.");
            return false;
        }

        if (network.Comp.ZLevels.ContainsValue(mapUid))
        {
            Log.Error($"Failed attempt to add map {mapUid} to ZLevelNetwork {network} at depth {depth}: This map is already in this network.");
            return false;
        }

        network.Comp.ZLevels.Add(depth, mapUid);
        network.Comp.ZLevelByEntity[mapUid] = depth;
        Dirty(network);

        // Welcome to fast api code
        QuickApiCache(network, mapUid, depth);

        var levelMapComponent = EnsureComp<CEZLevelMapComponent>(mapUid);
        levelMapComponent.Depth = depth;
        levelMapComponent.NetworkUid = network;

        if (network.Comp.ZLevels.TryGetValue(depth + 1, out var aboveMapUid) &&
            aboveMapUid is { } aboveUid)
        {
            levelMapComponent.MapAbove = aboveUid;
            if (TryComp<CEZLevelMapComponent>(aboveUid, out var aboveMapComponent))
            {
                aboveMapComponent.MapBelow = mapUid;
                Dirty(aboveUid, aboveMapComponent);
            }
        }

        if (network.Comp.ZLevels.TryGetValue(depth - 1, out var belowMapUid) &&
            belowMapUid is { } belowUid)
        {
            levelMapComponent.MapBelow = belowUid;
            if (TryComp<CEZLevelMapComponent>(belowUid, out var belowMapComponent))
            {
                belowMapComponent.MapAbove = mapUid;
                Dirty(belowUid, belowMapComponent);
            }
        }

        Dirty(mapUid, levelMapComponent);
        RefreshZPhysicsOnMap((mapUid, levelMapComponent));

        return true;
    }

    public bool TryAddMapsIntoZNetwork(Entity<CEZLevelsNetworkComponent> network, Dictionary<EntityUid, int> maps)
    {
        var success = true;
        foreach (var (ent, depth) in maps)
        {
            if (!TryAddMapIntoZNetwork(network, ent, depth))
                success = false;
        }

        RaiseLocalEvent(network, new CEZLevelNetworkUpdatedEvent());
        SyncNetworkLighting(network);

        return success;
    }

    /// <summary>
    /// Attempts to remove the specified map from the zNetwork network
    /// </summary>
    [PublicAPI]
    public bool TryRemoveMapFromZNetwork(Entity<CEZLevelsNetworkComponent> network, EntityUid mapUid)
    {
        if (!TryComp<CEZLevelMapComponent>(mapUid, out var zLevelMapComponent))
        {
            Log.Error($"Failed to remove map {mapUid} from ZLevelNetwork {network}: Map does not have CEZLevelMapComponent.");
            return false;
        }

        if (zLevelMapComponent.NetworkUid != network.Owner)
        {
            Log.Error($"Failed to remove map {mapUid} from ZLevelNetwork {network}: Map is not in this network.");
            return false;
        }

        var depth = zLevelMapComponent.Depth;

        // Remove from dictionary
        if (!network.Comp.ZLevels.Remove(depth))
        {
            Log.Error($"Failed to remove map {mapUid} from ZLevelNetwork {network}: Depth {depth} not found in dictionary.");
            return false;
        }

        network.Comp.ZLevelByEntity.Remove(mapUid);
        Dirty(network);

        // Update cache
        QuickApiCacheRemove(network, depth);

        // Update neighbors
        if (zLevelMapComponent.MapAbove.HasValue)
        {
            if (TryComp<CEZLevelMapComponent>(zLevelMapComponent.MapAbove.Value, out var aboveMap))
            {
                aboveMap.MapBelow = null;
                Dirty(zLevelMapComponent.MapAbove.Value, aboveMap);
            }
        }

        if (zLevelMapComponent.MapBelow.HasValue)
        {
            if (TryComp<CEZLevelMapComponent>(zLevelMapComponent.MapBelow.Value, out var belowMap))
            {
                belowMap.MapAbove = null;
                Dirty(zLevelMapComponent.MapBelow.Value, belowMap);
            }
        }

        // Remove component from map
        RemComp<CEZLevelMapComponent>(mapUid);

        RaiseLocalEvent(network, new CEZLevelNetworkUpdatedEvent());

        return true;
    }

    private void QuickApiCache(Entity<CEZLevelsNetworkComponent> network, EntityUid value, int depth)
    {
        var comp = network.Comp;
        var list = comp.SortedZLevels;

        // Zero handling
        if (comp.SortedMin == depth && comp.SortedMax == depth)
        {
            list.Add(value);
            return;
        }

        var min = comp.SortedMin;
        var max = comp.SortedMax;

        if (depth < min)
        {
            var delta = min - depth;
            if (delta == 1)
            {
                list.Insert(0, value);

                comp.SortedMin = depth;
                Dirty(network);
                return;
            }

            list.InsertRange(0, Enumerable.Repeat(EntityUid.Invalid, delta - 1));
            list.Insert(0, value);

            comp.SortedMin = depth;
            Dirty(network);
            return;
        }

        if (depth > max)
        {
            var delta = depth - max;
            if (delta == 1)
            {
                list.Add(value);

                comp.SortedMax = depth;
                Dirty(network);
                return;
            }

            list.AddRange(Enumerable.Repeat(EntityUid.Invalid, delta - 1));
            list.Add(value);

            comp.SortedMax = depth;
            Dirty(network);
            return;
        }

        list[depth - min] = value;
    }

    private void QuickApiCacheRemove(Entity<CEZLevelsNetworkComponent> network, int depth)
    {
        var comp = network.Comp;
        var list = comp.SortedZLevels;

        var index = depth - comp.SortedMin;

        if (index < 0 || index >= list.Count)
        {
            Log.Error($"QuickApiCacheRemove: depth {depth} is out of range for network {network}. Min: {comp.SortedMin}, Max: {comp.SortedMax}");
            return;
        }

        list[index] = EntityUid.Invalid;

        // Update min/max if needed
        if (depth == comp.SortedMin)
        {
            // Find new min
            var newMin = comp.SortedMin;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] != EntityUid.Invalid)
                {
                    newMin = comp.SortedMin + i;
                    break;
                }
            }
            if (newMin != comp.SortedMin)
            {
                comp.SortedMin = newMin;
                Dirty(network);
            }
        }

        if (depth == comp.SortedMax)
        {
            // Find new max
            var newMax = comp.SortedMax;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] != EntityUid.Invalid)
                {
                    newMax = comp.SortedMin + i;
                    break;
                }
            }
            if (newMax != comp.SortedMax)
            {
                comp.SortedMax = newMax;
                Dirty(network);
            }
        }
    }
}

/// <summary>
/// Called on ZLevel Network Entity, when maps added or removed from network
/// </summary>
public sealed class CEZLevelNetworkUpdatedEvent : EntityEventArgs;
