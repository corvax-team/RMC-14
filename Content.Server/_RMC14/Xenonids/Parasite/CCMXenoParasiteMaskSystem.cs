using Content.Shared._RMC14.Xenonids.Parasite;
using Content.Shared.Clothing;
using Robust.Shared.GameObjects;

namespace Content.Server._RMC14.Xenonids.Parasite;

public sealed class CCMXenoParasiteMaskSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenoParasiteComponent, ClothingGotEquippedEvent>(OnParasiteEquipped);
        SubscribeLocalEvent<XenoParasiteComponent, ClothingGotUnequippedEvent>(OnParasiteUnequipped);
        SubscribeLocalEvent<CCMRoyalParasiteComponent, ClothingGotEquippedEvent>(OnRoyalParasiteEquipped);
        SubscribeLocalEvent<CCMRoyalParasiteComponent, ClothingGotUnequippedEvent>(OnRoyalParasiteUnequipped);
    }

    private void OnParasiteEquipped(Entity<XenoParasiteComponent> ent, ref ClothingGotEquippedEvent args)
    {
        if (args.Clothing.InSlot != "mask")
            return;
        UpdateMaskState(ent, args.Wearer, true);
    }

    private void OnParasiteUnequipped(Entity<XenoParasiteComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if (args.Clothing.InSlot != "mask")
            return;
        UpdateMaskState(ent, args.Wearer, false);
    }

    private void OnRoyalParasiteEquipped(Entity<CCMRoyalParasiteComponent> ent, ref ClothingGotEquippedEvent args)
    {
        if (args.Clothing.InSlot != "mask")
            return;
        UpdateMaskState(ent, args.Wearer, true);
    }

    private void OnRoyalParasiteUnequipped(Entity<CCMRoyalParasiteComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if (args.Clothing.InSlot != "mask")
            return;
        UpdateMaskState(ent, args.Wearer, false);
    }

    private void UpdateMaskState(EntityUid parasite, EntityUid victim, bool equipped)
    {
        UpdateState(parasite, CCMXenoParasiteMaskVisuals.InMask, equipped);
        UpdateState(victim, CCMXenoParasiteMaskVisuals.VictimInfected, equipped);
    }

    private void UpdateState(EntityUid ent, Enum key, bool value)
    {
        if (_appearance.TryGetData(ent, key, out bool currentState) && currentState == value)
            return;
        _appearance.SetData(ent, key, value);
    }
}
