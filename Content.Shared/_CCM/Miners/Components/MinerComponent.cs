using Content.Shared.FixedPoint;
using Content.Shared.Tools;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Miners.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MinerComponent : Component
{
    [DataField, AutoNetworkedField]
    public MinerState State = MinerState.Running;

    [DataField, AutoNetworkedField]
    public int MineralStored;

    [DataField, AutoNetworkedField]
    public HashSet<MinerModuleType> Modules = new();

    [DataField, AutoNetworkedField]
    public int MineralStorage = 1;

    [DataField, AutoNetworkedField]
    public TimeSpan MineralProductionTime = TimeSpan.FromMinutes(5);

    [DataField, AutoNetworkedField]
    public TimeSpan NextMineralProduction;

    [DataField, AutoNetworkedField]
    public float WeldingCost = 1f;

    [DataField, AutoNetworkedField]
    public TimeSpan BaseRepairDelay = TimeSpan.FromSeconds(10);

    [DataField, AutoNetworkedField]
    public TimeSpan BaseExtractionDelay = TimeSpan.FromSeconds(4);

    [DataField, AutoNetworkedField]
    public TimeSpan BaseModuleRemovalDelay = TimeSpan.FromSeconds(5);

    [DataField, AutoNetworkedField]
    public TimeSpan BaseModuleInstallDelay = TimeSpan.FromSeconds(5);

    [DataField, AutoNetworkedField]
    public EntProtoId OreCratePrototype = "CCMOreCratePhoron";

    [DataField, AutoNetworkedField]
    public SoundSpecifier AutoSellSound = new SoundPathSpecifier("/Audio/Effects/Cargo/beep.ogg");

    [DataField, AutoNetworkedField]
    public FixedPoint2 SmallDamageThreshold = 25;

    [DataField, AutoNetworkedField]
    public FixedPoint2 MediumDamageThreshold = 50;

    [DataField, AutoNetworkedField]
    public FixedPoint2 DestroyedThreshold = 100;

    [DataField]
    public ProtoId<ToolQualityPrototype> WeldingQuality = "Welding";

    [DataField]
    public ProtoId<ToolQualityPrototype> CuttingQuality = "Cutting";

    [DataField]
    public ProtoId<ToolQualityPrototype> WrenchQuality = "Anchoring";

    [DataField]
    public ProtoId<ToolQualityPrototype> PryingQuality = "Prying";

    [DataField]
    public EntProtoId AutomationModulePrototype = "CCMMinerModuleAutomation";

    [DataField]
    public EntProtoId SpeedModulePrototype = "CCMMinerModuleSpeed";

    [DataField]
    public EntProtoId ReinforcedModulePrototype = "CCMMinerModuleReinforced";
}

[Serializable, NetSerializable]
public enum MinerLayers : byte
{
    Base,
    AutomationOverlay,
    SpeedOverlay,
    ReinforcedOverlay
}

[Serializable, NetSerializable]
public enum MinerVisuals : byte
{
    State,
    Active,
    HasAutomation,
    HasSpeed,
    HasReinforced
}

[Serializable, NetSerializable]
public enum MinerState : byte
{
    Running,
    SmallDamage,
    MediumDamage,
    Destroyed
}

[Serializable, NetSerializable]
public enum MinerModuleType : byte
{
    Automation,
    Speed,
    Reinforced
}
