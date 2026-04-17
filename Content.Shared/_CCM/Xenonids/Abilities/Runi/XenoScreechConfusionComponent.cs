using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using System;

namespace Content.Shared._CCM14.Xenonids.Screech;

[RegisterComponent]
public sealed partial class XenoScreechConfusionComponent : Component
{
    [DataField] public TimeSpan ExpireAt;
    [DataField] public float FailChance = 0.3f;
    [DataField] public TimeSpan NextAllowedAction;
}