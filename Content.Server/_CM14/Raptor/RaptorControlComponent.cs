using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Raptor;

[RegisterComponent]
public sealed partial class RaptorControlComponent : Component
{
    [DataField("observerId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ObserverPrototypeId = "RaptorObserver";

    [DataField("observer")]
    public EntityUid? Observer = null;

    [DataField("pilot")]
    public EntityUid? Pilot = null;
}