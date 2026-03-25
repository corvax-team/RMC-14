using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.FpvDrone;

public sealed partial class FpvDroneExplosiveEvent : InstantActionEvent;

public sealed partial class FpvDroneEjectEvent : InstantActionEvent;

[Serializable]
[NetSerializable]
public sealed partial class FpvDroneFoldableDoAfterEvent : SimpleDoAfterEvent
{
}