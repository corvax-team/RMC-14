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

    [DataField, AutoNetworkedField]
    public List<string> HiddenCompatibilityIds = new();

    [DataField, AutoNetworkedField]
    public Dictionary<string, int> PrintedModules = new();

    [DataField, AutoNetworkedField]
    public TimeSpan DefaultPrintDelay = TimeSpan.FromSeconds(3);

    [DataField, AutoNetworkedField]
    public EntProtoId<SkillDefinitionComponent> RecycleSkillId = new("RMCSkillEngineer");
}

[Serializable, NetSerializable]
public enum VehicleFabricatorCategory : byte
{
    Primary,
    Secondary,
    Armor,
    Support,
    Chassis,
    RoofAttachment,
    FrontAttachment,
    Cannon,
    Launcher
}

[Serializable, NetSerializable]
public enum VehicleType : byte
{
    None,
    Tank,
    APC,
    Humvee,
    Van
}
