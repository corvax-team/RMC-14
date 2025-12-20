using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Vehicle.Components;

/// <summary>
/// A <see cref="CCMVehicleComponent"/> whose operator must be inside a specified container.
/// Note that the operator is the first to enter the container and won't be removed until they exit the container.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(VehicleSystem))]
public sealed partial class CCMContainerVehicleComponent : Component
{
    /// <summary>
    /// The ID of the container for the operator.
    /// </summary>
    [DataField(required: true)]
    public string ContainerId;
}
