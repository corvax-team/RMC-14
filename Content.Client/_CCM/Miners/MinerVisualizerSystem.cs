using Content.Shared._CCM.Miners.Components;
using Robust.Client.GameObjects;

namespace Content.Client._CCM.Miners;

public sealed class MinerVisualizerSystem : VisualizerSystem<MinerComponent>
{
    [Dependency] private readonly SpriteSystem _sprite = null!;

    protected override void OnAppearanceChange(EntityUid uid, MinerComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<MinerState>(uid, MinerVisuals.State, out var state, args.Component))
            state = component.State;

        if (!AppearanceSystem.TryGetData<bool>(uid, MinerVisuals.Active, out var active, args.Component))
            active = state == MinerState.Running;

        var baseRsiState = state switch
        {
            MinerState.Running => active ? "mining_drill_active" : "mining_drill_braced",
            MinerState.SmallDamage => "mining_drill",
            MinerState.MediumDamage => "mining_drill",
            _ => "mining_drill_error"
        };

        var ent = (uid, args.Sprite);
        _sprite.LayerSetRsiState(ent, MinerLayers.Base, baseRsiState);

        UpdateModule(ent, MinerVisuals.HasAutomation, MinerLayers.AutomationOverlay, "automation", baseRsiState, args);
        UpdateModule(ent, MinerVisuals.HasSpeed, MinerLayers.SpeedOverlay, "speed", baseRsiState, args);
        UpdateModule(ent, MinerVisuals.HasReinforced, MinerLayers.ReinforcedOverlay, "reinforced", baseRsiState, args);
    }

    private void UpdateModule(Entity<SpriteComponent?> ent, MinerVisuals key, MinerLayers layer, string suffix,
        string baseState, AppearanceChangeEvent args)
    {
        if (!_sprite.LayerExists(ent, layer))
            return;

        if (!AppearanceSystem.TryGetData<bool>(ent, key, out var hasModule, args.Component))
            hasModule = false;

        _sprite.LayerSetVisible(ent, layer, hasModule);

        if (hasModule) _sprite.LayerSetRsiState(ent, layer, $"{baseState}_{suffix}");
    }
}
