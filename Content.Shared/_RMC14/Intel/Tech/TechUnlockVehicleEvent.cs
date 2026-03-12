using System;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Intel.Tech;

[Serializable, NetSerializable]
public sealed record TechUnlockVehicleEvent
{
    [DataField("unlock")]
    public string Unlock = null!;
}
