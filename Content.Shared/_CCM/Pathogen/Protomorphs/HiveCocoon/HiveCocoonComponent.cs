using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CCM.Pathogen.Protomorphs.HiveCocoon;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
[Access(typeof(HiveCocoonSystem))]
public sealed partial class HiveCocoonComponent : Component
{
    [DataField, AutoNetworkedField]
    public HiveCocoonState State;

    [DataField, AutoNetworkedField]
    public Dictionary<HiveCocoonState, string?> VisualStates = new();

    [DataField, AutoNetworkedField]
    public TimeSpan OpeningTime = TimeSpan.Zero;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan OpeningAt;

    [DataField, AutoNetworkedField]
    public int MaxContainedMarines = 2;

    [DataField, AutoNetworkedField]
    public EntityWhitelist? InsertWhitelist;

    [DataField, AutoNetworkedField]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(5);

    [DataField, AutoNetworkedField]
    public string MarineContainerId = "ccm_cocoon_marine_container";

    [ViewVariables]
    public Container MarineContainer = default!;

    [DataField, AutoNetworkedField]
    public string EquipmentContainerId = "ccm_cocoon_equipment_container";

    [ViewVariables]
    public Container EquipmentContainer = default!;

    [DataField, AutoNetworkedField]
    public string BloodbursterSlotId = "ccm_cocoon_bloodburster_slot";

    [ViewVariables]
    public ContainerSlot BloodbursterSlot = default!;

    [DataField, AutoNetworkedField]
    public TimeSpan PupateTime = TimeSpan.FromSeconds(25);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan PupateAt;

    [DataField, AutoNetworkedField]
    public EntProtoId SpawnId = "CCMProtomorphBloodburster";
}

[Serializable, NetSerializable]
public enum HiveCocoonState
{
    Empty,
    Half,
    Full,
    Opening
}

[Serializable, NetSerializable]
public enum HiveCocoonLayers
{
    Base
}
