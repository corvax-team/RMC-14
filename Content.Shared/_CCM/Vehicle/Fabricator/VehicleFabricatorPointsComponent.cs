using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CCM.Vehicle.Fabricator;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
[AutoGenerateComponentPause]
[Access(typeof(VehicleFabricatorSystem))]
public sealed partial class VehicleFabricatorPointsComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))] [AutoNetworkedField] [AutoPausedField]
    public TimeSpan NextPointsAt;

    [DataField] [AutoNetworkedField] public int Points;
}