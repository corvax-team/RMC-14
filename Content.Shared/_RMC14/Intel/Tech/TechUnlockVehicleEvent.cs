using System;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared._RMC14.Intel.Tech;

[DataDefinition, NetSerializable, Serializable]
public sealed partial class TechUnlockVehicleEvent
{
    [DataField]
    public string Unlock = string.Empty;
}
