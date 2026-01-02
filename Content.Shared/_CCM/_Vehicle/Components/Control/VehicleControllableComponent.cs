using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Vehicle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleControllableComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Id = string.Empty;
}
