using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Projectile.Parasite;
using Robust.Client.GameObjects;

namespace Content.Client._RMC14.Xenonids.Parasite;

public sealed class XenoParasitesVisualSystem : VisualizerSystem<XenoParasiteThrowerComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, XenoParasiteThrowerComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null || !AppearanceSystem.TryGetData(uid, ParasiteOverlayVisuals.States, out bool[] states))
            return;

        var layerState = "para_";

        if (AppearanceSystem.TryGetData(uid, RMCXenoStateVisuals.Downed, out var downed) && (bool)downed)
            layerState = "para_downed_";
        else if (AppearanceSystem.TryGetData(uid, RMCXenoStateVisuals.Resting, out var resting) && (bool)resting)
            layerState = "para_rest_";

        foreach (var layer in Enum.GetValues<ParasiteOverlayLayers>())
        {
            if (!args.Sprite.LayerMapTryGet(layer, out _))
                continue;

            args.Sprite.LayerSetVisible(layer, states[(int)layer]);
            args.Sprite.LayerSetState(layer, $"{layerState}{(int)layer}");
        }
    }
}
