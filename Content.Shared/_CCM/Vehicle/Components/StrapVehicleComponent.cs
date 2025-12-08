using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Vehicle.Components;

/// <summary>
/// A <see cref="CCMVehicleComponent"/> whose operator must be buckled to it.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(VehicleSystem))]
public sealed partial class CCMStrapVehicleComponent : Component;
