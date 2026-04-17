using Robust.Shared.GameObjects;

namespace Content.Shared._RMC14.Xenonids.Parasite;

[RegisterComponent]
public sealed partial class CCMParasiteSpriteComponent : Component
{
    [DataField]
    public string MobRsi = "Sprites/_RMC14/Mobs/Xenonids/Parasite/parasite.rsi";

    [DataField]
    public string InventoryRsi = "Sprites/_RMC14/Mobs/Xenonids/Parasite/parasite_inventory.rsi";

    [DataField]
    public string InventoryState = "icon";
}
