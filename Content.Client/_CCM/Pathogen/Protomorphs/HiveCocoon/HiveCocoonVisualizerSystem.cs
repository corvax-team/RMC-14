using Content.Shared._CCM.Pathogen.Protomorphs.HiveCocoon;
using Robust.Client.GameObjects;

namespace Content.Client._CCM.Pathogen.Protomorphs.HiveCocoon;

public sealed class HiveCocoonVisualizerSystem : VisualizerSystem<HiveCocoonComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, HiveCocoonComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is not { } sprite)
            return;

        if (!AppearanceSystem.TryGetData<HiveCocoonState>(uid, HiveCocoonLayers.Base, out var cocoonState))
            return;

        var spriteState = component.VisualStates[cocoonState];
        if (string.IsNullOrEmpty(spriteState))
            return;

#pragma warning disable CS0618
        sprite.LayerMapTryGet(HiveCocoonLayers.Base, out var layer);
        sprite.LayerSetState(layer, spriteState);
#pragma warning restore CS0618
    }
}
