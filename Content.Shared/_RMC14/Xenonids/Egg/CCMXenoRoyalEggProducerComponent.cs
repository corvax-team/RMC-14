using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._RMC14.Xenonids.Egg;

[RegisterComponent, NetworkedComponent]
[Access(typeof(XenoEggSystem))]
[AutoGenerateComponentState]
[AutoGenerateComponentPause]
public sealed partial class CCMXenoRoyalEggProducerComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan RoyalEggInterval = TimeSpan.FromMinutes(5);

    [DataField, AutoNetworkedField]
    public EntProtoId RoyalEggPrototype = "CCMXenoRoyalEgg";

    [DataField, AutoNetworkedField]
    public Vector2 RoyalEggOffset = new(-1, -1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan? NextRoyalEggAt { get; set; }
}
