using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._CM14.FpvDrone;

[Serializable]
[NetSerializable]
public sealed class FpvDroneSetOverlayEvent(bool enable) : EntityEventArgs
{
    public bool Enable = enable;
}

public sealed partial class FpvDroneExplosiveEvent : InstantActionEvent;

public sealed partial class FpvDroneEjectEvent : InstantActionEvent;