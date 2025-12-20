using Content.Shared._CCM.Mech;
using Content.Shared._CCM.Mech.Components;
using Content.Shared._CCM.Mech.EntitySystems;
using Robust.Client.GameObjects;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Client._CCM.Mech;

public sealed class MechSystem : SharedMechSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CCMMechComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(Entity<CCMMechComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        var state = ent.Comp.BaseState;
        var drawDepth = DrawDepth.Mobs;

        var isBroken = false;
        var isOpen = false;

        if (args.AppearanceData.TryGetValue(MechVisuals.Broken, out var brokenObj) && brokenObj is bool brokenFlag)
            isBroken = brokenFlag;
        if (args.AppearanceData.TryGetValue(MechVisuals.Open, out var openObj) && openObj is bool openFlag)
            isOpen = openFlag;

        // Priority: Broken > Open > Base
        if (ent.Comp.BrokenState != null && isBroken)
        {
            state = ent.Comp.BrokenState;
            drawDepth = DrawDepth.SmallMobs;
        }
        else if (ent.Comp.OpenState != null && isOpen)
        {
            state = ent.Comp.OpenState;
            drawDepth = DrawDepth.SmallMobs;
        }

        _sprite.LayerSetVisible((ent.Owner, args.Sprite), MechVisualLayers.Base, true);
        _sprite.LayerSetAutoAnimated((ent.Owner, args.Sprite), MechVisualLayers.Base, true);
        _sprite.LayerSetRsiState((ent.Owner, args.Sprite), MechVisualLayers.Base, state);
        _sprite.SetDrawDepth((ent.Owner, args.Sprite), (int)drawDepth);
    }
}
