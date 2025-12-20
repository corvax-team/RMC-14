using Robust.Shared.GameObjects;

namespace Content.Shared._RMC14.Xenonids.Egg;

[RegisterComponent]
public sealed partial class CCMRoyalEggProducerComponent : Component
{
    [DataField]
    public EntityUid? Producer;
}
