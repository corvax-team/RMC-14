using Content.Shared._RMC14.Marines.Skills;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CCM.Vehicle.Fabricator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
[Access(typeof(VehicleFabricatorSystem))]
public sealed partial class VehicleFabricatorComponent : Component
{
    [DataField]
    public EntityUid? Account;

    [DataField, AutoNetworkedField]
    public int Points;

    [DataField, AutoNetworkedField]
    public EntProtoId? Printing;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan PrintAt;

    [DataField, AutoNetworkedField]
    public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier PrintSound = new SoundPathSpecifier("/Audio/_RMC14/Machines/print.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier RecycleSound = new SoundPathSpecifier("/Audio/_RMC14/Machines/fax.ogg");
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(VehicleFabricatorSystem))]
public sealed partial class VehicleFabricatorPrintableComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    [DataField, AutoNetworkedField]
    public int Cost = 50;

    [DataField, AutoNetworkedField]
    public float RecycleMultiplier = 0.8f;

    [DataField, AutoNetworkedField]
    public EntProtoId<SkillDefinitionComponent> RecycleSkill = "RMCSkillEngineer";

    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(3);

    [DataField, AutoNetworkedField]
    public VehicleFabricatorCategory Category;

    [DataField, AutoNetworkedField]
    public RMCVehicleType Vehicle = RMCVehicleType.None;
}

[Serializable, NetSerializable]
public enum VehicleFabricatorCategory : byte
{
    Primary,
    Secondary,
    Armor,
    Support,
    Chassis,
    Ammo,
}

[Flags, Serializable, NetSerializable]
public enum RMCVehicleType : byte
{
    None = 0,
    Tank = 1 << 0,
    APC = 1 << 1,
    Humvee = 1 << 2,
}
