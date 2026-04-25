using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Miners.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MinerModuleComponent : Component
{
    [DataField("moduleType"), AutoNetworkedField]
    public MinerModuleType Type = MinerModuleType.Automation;
}
