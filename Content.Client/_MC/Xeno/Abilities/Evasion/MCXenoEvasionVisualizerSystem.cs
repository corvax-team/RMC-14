using Content.Shared._MC.Xeno.Abilities.Evasion;
using Robust.Client.GameObjects;

namespace Content.Client._MC.Xeno.Abilities.Evasion;

public sealed class MCXenoEvasionVisualizerSystem : VisualizerSystem<MCXenoEvasionComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, MCXenoEvasionComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if (args.Sprite is null)
            return;

        if (!AppearanceSystem.TryGetData<bool>(uid, EvasionLayer.Base, out var value, args.Component))
            return;

        if (!args.Sprite.LayerMapTryGet(EvasionLayer.Base, out var layer))
            return;

        args.Sprite.LayerSetVisible(layer, value);

        if (!AppearanceSystem.TryGetData<int>(uid, EvasionVisuals.Visuals, out var seconds, args.Component))
            return;

        seconds = Math.Clamp(seconds, 1, 6);
        args.Sprite.LayerSetState(layer, seconds.ToString());
    }
}
