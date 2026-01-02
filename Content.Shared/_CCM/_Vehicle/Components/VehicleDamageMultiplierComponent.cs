using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Vehicle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleDamageMultiplierComponent : Component
{
	[DataField, AutoNetworkedField]
	public float Mult = 1f;
}
