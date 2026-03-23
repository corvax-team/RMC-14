using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Raptor;

[RegisterComponent]
public sealed partial class RaptorObserverComponent : Component
{
    [DataField("ejectActionId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string EjectActionPrototypeId = "ActionRaptorEject";

    [DataField("ejectAction")]
    public EntityUid? EjectAction;

    [DataField("control")]
    public EntityUid Control;
}