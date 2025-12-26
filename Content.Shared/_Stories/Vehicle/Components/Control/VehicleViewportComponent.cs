using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Vehicle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleViewportComponent : Component
{
	[DataField, AutoNetworkedField]
	public EntityUid? Watcher;
}
