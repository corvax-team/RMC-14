using Robust.Shared.GameObjects;

namespace Content.Shared._RMC14.Xenonids.Parasite;

[RegisterComponent]
public sealed partial class CCMParasiteSpriteComponent : Component
{
    [DataField]
    public string MaskRsi = "_RMC14/Mobs/Xenonids/Parasite/parasite_mask.rsi";

    [DataField]
    public string MaskInventoryState = "icon";

    [DataField]
    public string NormalRsi = "_RMC14/Mobs/Xenonids/Parasite/parasite.rsi";

    [DataField]
    public string NormalState = "alive";
}

