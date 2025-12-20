using Content.Shared._RMC14.Sprite;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hide;
using Content.Shared._RMC14.Xenonids.Leap;
using Content.Shared._RMC14.Xenonids.Parasite;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._RMC14.Xenonids.Infected;

public sealed class XenoParasiteSystem : SharedXenoParasiteSystem
{
    [Dependency] private readonly XenoVisualizerSystem _xenoVisualizer = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private readonly Dictionary<EntityUid, bool> _lastInMaskState = new();
    private const int MaxCachedStates = 100;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenoParasiteComponent, GetDrawDepthEvent>(OnGetParasiteDrawDepth, before: [typeof(XenoHideSystem)]);
        SubscribeLocalEvent<XenoParasiteComponent, AppearanceChangeEvent>(OnParasiteAppearanceChange);
        SubscribeLocalEvent<CCMRoyalParasiteComponent, AppearanceChangeEvent>(OnRoyalParasiteAppearanceChange);
    }

    private void OnGetParasiteDrawDepth(Entity<XenoParasiteComponent> parasite, ref GetDrawDepthEvent args)
    {
        if (_tags.HasTag(parasite, ParasiteIsPreparingLeapProtoID) ||
            HasComp<XenoLeapingComponent>(parasite))
        {
            args.DrawDepth = Shared.DrawDepth.DrawDepth.Overdoors;
        }
        else
        {
            args.DrawDepth = Shared.DrawDepth.DrawDepth.Mobs;
        }
    }

    private void OnParasiteAppearanceChange(Entity<XenoParasiteComponent> parasite, ref AppearanceChangeEvent args)
    {
        UpdateParasiteMaskSprite(parasite.Owner, args.Sprite);
    }

    private void OnRoyalParasiteAppearanceChange(Entity<CCMRoyalParasiteComponent> parasite, ref AppearanceChangeEvent args)
    {
        UpdateParasiteMaskSprite(parasite.Owner, args.Sprite);
    }

    private void UpdateParasiteMaskSprite(EntityUid parasite, SpriteComponent? sprite)
    {
        if (sprite == null)
            return;

        if (!TryComp<CCMParasiteSpriteComponent>(parasite, out var spriteComp))
            return;

        if (!sprite.LayerMapTryGet(XenoVisualLayers.Base, out var layer))
            return;

        var inMask = false;
        if (_appearance.TryGetData(parasite, CCMXenoParasiteMaskVisuals.InMask, out bool maskData))
            inMask = maskData;

        if (_lastInMaskState.TryGetValue(parasite, out var lastState) && lastState == inMask)
            return;

        if (_lastInMaskState.Count >= MaxCachedStates)
            _lastInMaskState.Clear();

        _lastInMaskState[parasite] = inMask;

        if (inMask)
        {
            sprite.LayerSetRSI(layer, new ResPath(spriteComp.MaskRsi));
            sprite.LayerSetState(layer, spriteComp.MaskInventoryState);
        }
        else
        {
            sprite.LayerSetRSI(layer, new ResPath(spriteComp.NormalRsi));
            sprite.LayerSetState(layer, spriteComp.NormalState);
        }
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = EntityQueryEnumerator<XenoComponent, ThrownItemComponent, SpriteComponent, AppearanceComponent>();
        while (query.MoveNext(out var uid, out _, out var thrown, out var sprite, out var appearance))
        {
            _xenoVisualizer.UpdateSprite((uid, sprite, null, appearance, null, thrown));
        }
    }
}
