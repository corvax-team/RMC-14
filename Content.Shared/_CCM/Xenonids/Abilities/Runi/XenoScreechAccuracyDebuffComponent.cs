using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM14.Xenonids.Screech;

[RegisterComponent]
public sealed partial class XenoScreechAccuracyDebuffComponent : Component
{
    [DataField]
    public float Multiplier = 1f;

    [DataField]
    public TimeSpan ExpireAt;
}