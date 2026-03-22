using Content.Shared._MC.Xeno.Abilities.Evasion;
using Content.Shared._RMC14.Xenonids;
using Robust.Client.GameObjects;
using Robust.Client.Placement;
using Robust.Client.Player;

namespace Content.Client._MC.Xeno.Abilities.Evasion;

public sealed class MCXenoEvasionVisualizerSystem : VisualizerSystem<MCXenoEvasionComponent>
{
    [Dependency] private readonly IPlayerManager _player = default!;

    protected override void OnAppearanceChange(EntityUid uid, MCXenoEvasionComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if (_player.LocalEntity is null || !HasComp<XenoComponent>(_player.LocalEntity))
            return;

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