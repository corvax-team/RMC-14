/*
 * Copyright (c) 2026 TornadgoTechnology
 * Copyright (c) 2026 CrystallEdge (https://github.com/crystallpunk-14/crystall-edge)
 *
 * SPDX-License-Identifier: PolyForm-Noncommercial-1.0.0 AND MIT
 */

using System.Linq;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._RMC14.Light;
using Content.Shared._RMC14.Weather;
using Content.Shared.Light.Components;
using Content.Shared.Weather;
using Robust.Shared.Map.Components;

namespace Content.Server._CE.ZLevels.Core;

public sealed partial class CEZLevelsSystem
{
    private EntityQuery<MapLightComponent> _mapLightQuery;
    private EntityQuery<LightCycleComponent> _lightCycleQuery;
    private EntityQuery<SunShadowCycleComponent> _sunShadowCycleQuery;
    private EntityQuery<RMCAmbientLightComponent> _rmcAmbientLightQuery;
    private EntityQuery<RMCAmbientLightEffectsComponent> _rmcAmbientLightEffectsQuery;
    private EntityQuery<RMCWeatherCycleComponent> _rmcWeatherCycleQuery;
    private EntityQuery<WeatherComponent> _weatherQuery;

    private void InitLightSync()
    {
        _mapLightQuery = GetEntityQuery<MapLightComponent>();
        _lightCycleQuery = GetEntityQuery<LightCycleComponent>();
        _sunShadowCycleQuery = GetEntityQuery<SunShadowCycleComponent>();
        _rmcAmbientLightQuery = GetEntityQuery<RMCAmbientLightComponent>();
        _rmcAmbientLightEffectsQuery = GetEntityQuery<RMCAmbientLightEffectsComponent>();
        _rmcWeatherCycleQuery = GetEntityQuery<RMCWeatherCycleComponent>();
        _weatherQuery = GetEntityQuery<WeatherComponent>();
    }

    private void UpdateLightSync(float frameTime)
    {
        var query = EntityQueryEnumerator<CEZLevelsNetworkComponent>();
        while (query.MoveNext(out var uid, out var network))
        {
            SyncNetworkLighting((uid, network));
        }
    }

    private void SyncNetworkLighting(Entity<CEZLevelsNetworkComponent> network)
    {
        if (!TryGetLightSyncSource(network, out var source))
            return;

        foreach (var map in network.Comp.ZLevels.Values)
        {
            if (map == null || map.Value == source)
                continue;

            SyncMapLighting(source, map.Value);
        }
    }

    private bool TryGetLightSyncSource(Entity<CEZLevelsNetworkComponent> network, out EntityUid source)
    {
        if (network.Comp.ZLevels.TryGetValue(0, out var zeroMap) && zeroMap != null)
        {
            source = zeroMap.Value;
            return true;
        }

        foreach (var map in network.Comp.ZLevels.Values)
        {
            if (map == null)
                continue;

            source = map.Value;
            return true;
        }

        source = default;
        return false;
    }

    private void SyncMapLighting(EntityUid source, EntityUid target)
    {
        SyncMapLight(source, target);
        SyncLightCycle(source, target);
        SyncSunShadowCycle(source, target);
        SyncRmcAmbientLight(source, target);
        SyncRmcAmbientLightEffects(source, target);
        DisableSecondaryWeather(target);
    }

    private void SyncMapLight(EntityUid source, EntityUid target)
    {
        if (!_mapLightQuery.TryComp(source, out var sourceLight))
        {
            if (_mapLightQuery.HasComp(target))
                RemComp<MapLightComponent>(target);

            return;
        }

        var targetLight = EnsureComp<MapLightComponent>(target);
        if (targetLight.AmbientLightColor.Equals(sourceLight.AmbientLightColor))
            return;

        targetLight.AmbientLightColor = sourceLight.AmbientLightColor;
        Dirty(target, targetLight);
    }

    private void SyncLightCycle(EntityUid source, EntityUid target)
    {
        if (!_lightCycleQuery.TryComp(source, out var sourceCycle))
        {
            if (_lightCycleQuery.HasComp(target))
                RemComp<LightCycleComponent>(target);

            return;
        }

        var targetCycle = EnsureComp<LightCycleComponent>(target);
        var changed = false;

        changed |= SetIfChanged(ref targetCycle.OriginalColor, sourceCycle.OriginalColor);
        changed |= SetIfChanged(ref targetCycle.Duration, sourceCycle.Duration);
        changed |= SetIfChanged(ref targetCycle.Offset, sourceCycle.Offset);
        changed |= SetIfChanged(ref targetCycle.Enabled, sourceCycle.Enabled);
        changed |= SetIfChanged(ref targetCycle.InitialOffset, sourceCycle.InitialOffset);
        changed |= SetIfChanged(ref targetCycle.MinLightLevel, sourceCycle.MinLightLevel);
        changed |= SetIfChanged(ref targetCycle.MaxLightLevel, sourceCycle.MaxLightLevel);
        changed |= SetIfChanged(ref targetCycle.ClipLight, sourceCycle.ClipLight);
        changed |= SetIfChanged(ref targetCycle.ClipLevel, sourceCycle.ClipLevel);
        changed |= SetIfChanged(ref targetCycle.MinLevel, sourceCycle.MinLevel);
        changed |= SetIfChanged(ref targetCycle.MaxLevel, sourceCycle.MaxLevel);

        if (changed)
            Dirty(target, targetCycle);
    }

    private void SyncSunShadowCycle(EntityUid source, EntityUid target)
    {
        if (!_sunShadowCycleQuery.TryComp(source, out var sourceCycle))
        {
            if (_sunShadowCycleQuery.HasComp(target))
                RemComp<SunShadowCycleComponent>(target);

            return;
        }

        var targetCycle = EnsureComp<SunShadowCycleComponent>(target);
        var changed = false;

        changed |= SetIfChanged(ref targetCycle.Duration, sourceCycle.Duration);
        changed |= SetIfChanged(ref targetCycle.Offset, sourceCycle.Offset);

        if (!targetCycle.Directions.SequenceEqual(sourceCycle.Directions))
        {
            targetCycle.Directions = new List<SunShadowCycleDirection>(sourceCycle.Directions);
            changed = true;
        }

        if (changed)
            Dirty(target, targetCycle);
    }

    private void SyncRmcAmbientLight(EntityUid source, EntityUid target)
    {
        if (!_rmcAmbientLightQuery.TryComp(source, out var sourceLight))
        {
            if (_rmcAmbientLightQuery.HasComp(target))
                RemComp<RMCAmbientLightComponent>(target);

            return;
        }

        var targetLight = EnsureComp<RMCAmbientLightComponent>(target);
        var changed = false;

        changed |= SetIfChanged(ref targetLight.IsAnimating, sourceLight.IsAnimating);
        changed |= SetIfChanged(ref targetLight.Duration, sourceLight.Duration);
        changed |= SetIfChanged(ref targetLight.StartTime, sourceLight.StartTime);

        if (!targetLight.Colors.SequenceEqual(sourceLight.Colors))
        {
            targetLight.Colors.Clear();
            targetLight.Colors.AddRange(sourceLight.Colors);
            changed = true;
        }

        if (changed)
            Dirty(target, targetLight);
    }

    private void SyncRmcAmbientLightEffects(EntityUid source, EntityUid target)
    {
        if (!_rmcAmbientLightEffectsQuery.TryComp(source, out var sourceEffects))
        {
            if (_rmcAmbientLightEffectsQuery.HasComp(target))
                RemComp<RMCAmbientLightEffectsComponent>(target);

            return;
        }

        var targetEffects = EnsureComp<RMCAmbientLightEffectsComponent>(target);
        var changed = false;

        changed |= SetIfChanged(ref targetEffects.Sunset, sourceEffects.Sunset);
        changed |= SetIfChanged(ref targetEffects.Sunrise, sourceEffects.Sunrise);

        if (changed)
            Dirty(target, targetEffects);
    }

    private void DisableSecondaryWeather(EntityUid target)
    {
        if (_rmcWeatherCycleQuery.HasComp(target))
            RemComp<RMCWeatherCycleComponent>(target);

        if (_weatherQuery.HasComp(target))
            RemComp<WeatherComponent>(target);
    }

    private static bool SetIfChanged<T>(ref T target, T source)
    {
        if (EqualityComparer<T>.Default.Equals(target, source))
            return false;

        target = source;
        return true;
    }
}
