using System;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Intel.Tech;

[DataDefinition, NetSerializable, Serializable]
public sealed partial class TechUnlockVehicleEvent
{
    [DataField]
    public string Unlock = string.Empty;
}
