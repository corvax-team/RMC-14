/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared.Weather;
using Robust.Shared.Map.Components;

namespace Content.Shared._CE.ZLevels.Weather;

/// <summary>
/// A subsystem that connects WeatherSystem with ZLevelSystem.
/// </summary>
public sealed class CEWeatherSystem : EntitySystem
{
    [Dependency] private readonly SharedWeatherSystem _weather = default!;

    public void SetWeather(Entity<CEZLevelsNetworkComponent?> network, WeatherPrototype? proto, TimeSpan? endTime)
    {
        if (!Resolve(network, ref network.Comp))
            return;

        EntityUid? mainMap = null;
        foreach (var (depth, map) in network.Comp.ZLevels)
        {
            if (depth != 0 || map == null)
                continue;

            mainMap = map.Value;
            break;
        }

        if (mainMap == null || !TryComp<MapComponent>(mainMap.Value, out var mapComp))
            return;

        _weather.SetWeather(mapComp.MapId, proto, endTime);
    }
}
