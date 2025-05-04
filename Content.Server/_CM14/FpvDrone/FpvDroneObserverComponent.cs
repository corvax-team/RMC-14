using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.FpvDrone;

[RegisterComponent]
public sealed partial class FpvDroneObserverComponent : Component
{
    [DataField("ejectActionId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string EjectActionPrototypeId = "ActionRaptorEject";

    [DataField("ejectAction")]
    public EntityUid? EjectAction;

    [DataField("control")]
    public EntityUid Control;
}

[RegisterComponent]
public sealed partial class FpvDroneGogglesComponent : Component
{
}

[RegisterComponent]
public sealed partial class FpvDroneScreenOverlayComponent : Component
{
}