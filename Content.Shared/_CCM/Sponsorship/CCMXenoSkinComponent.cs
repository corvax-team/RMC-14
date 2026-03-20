using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Sponsorship;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CCMXenoSkinComponent : Component
{
    [DataField, AutoNetworkedField]
    public string RsiPath = string.Empty;
}
