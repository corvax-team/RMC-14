using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.Attachables;

[Prototype, Serializable]
public sealed partial class HardpointTypePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public float RepairPerSecond = 0f;

    [DataField]
    public float AttachDelay = 0f;
}
