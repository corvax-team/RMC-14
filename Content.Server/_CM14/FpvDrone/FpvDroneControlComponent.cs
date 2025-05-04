using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.FpvDrone;

[RegisterComponent]
public sealed partial class FpvDroneControlComponent : Component
{
    [DataField("observerId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ObserverPrototypeId = "FpvDroneObserver";

    [DataField("observer")]
    public EntityUid? Observer = null;

    [DataField("pilot")]
    public EntityUid? Pilot = null;

    [DataField]
    public bool Used;
}