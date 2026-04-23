using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.Vehicle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleTurretVisualComponent : Component
{
    [AutoNetworkedField]
    public NetEntity Turret;
}
