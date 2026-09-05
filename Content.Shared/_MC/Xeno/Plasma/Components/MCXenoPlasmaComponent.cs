using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Plasma.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPlasmaComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool CanBeGivenPlasma = true;
}
