using System;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Intel.Tech;

[DataRecord] // CCM14
[Serializable, NetSerializable]
// CCM14-start
public sealed record TechUnlockVehicleEvent
{
    [DataField("unlock")]
    public string Unlock = default!;
}
// CCM14-end