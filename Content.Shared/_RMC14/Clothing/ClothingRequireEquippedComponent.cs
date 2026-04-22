using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.Clothing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RMCClothingSystem))]
public sealed partial class ClothingRequireEquippedComponent : Component
{
    // CCM14-start
    [DataField, AutoNetworkedField]
    public bool Enabled = false;
    // CCM14-end
    [DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist;

    [DataField, AutoNetworkedField]
    public string DenyReason = "rmc-wear-smart-gun-required";

    [DataField, AutoNetworkedField]
    public bool AutoUnequip = false;
}
