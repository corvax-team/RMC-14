using Content.Shared._RMC14.Weapons.Ranged.Whitelist;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Silencer;

[RegisterComponent, NetworkedComponent]
[SpecialistSkillComponent("Silencer")]
public sealed partial class CCMSilencerWhitelistComponent : Component;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMSilencerUseWhitelistComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityWhitelist Whitelist = new()
    {
        Components = new[] { "CCMSilencerWhitelist" },
    };

    [DataField, AutoNetworkedField]
    public LocId DenyPopup = "ccm-silencer-unskilled";
}
