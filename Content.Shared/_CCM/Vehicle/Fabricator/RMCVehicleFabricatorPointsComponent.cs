using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CCM.Vehicle.Fabricator;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
[AutoGenerateComponentPause]
[Access(typeof(RMCVehicleFabricatorSystem))]
public sealed partial class RMCVehicleFabricatorPointsComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))] [AutoNetworkedField] [AutoPausedField]
    public TimeSpan NextPointsAt;

    [DataField] [AutoNetworkedField] public int Points;
}