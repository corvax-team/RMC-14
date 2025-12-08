using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Vehicle.Components;

/// <summary>
/// Tracking component for handling the operator of a given <see cref="CCMVehicleComponent"/>
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(VehicleSystem))]
public sealed partial class CCMVehicleOperatorComponent : Component
{
    /// <summary>
    /// The vehicle we are currently operating.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Vehicle;
}
