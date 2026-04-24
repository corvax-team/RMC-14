using Robust.Shared.GameStates;

namespace Content.Shared._CCM.Xenonids.Parasite;

[RegisterComponent]
public sealed partial class CCMParasiteMultiLarvaComponent : Component
{
    [DataField("larvaCount")]
    public int LarvaCount = 1;
}
