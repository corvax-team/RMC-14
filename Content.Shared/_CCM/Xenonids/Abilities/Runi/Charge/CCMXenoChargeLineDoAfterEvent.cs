using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM14.Xeno.Abilities.ChargeLine;

[Serializable, NetSerializable]
public sealed partial class CCMXenoChargeLineDoAfterEvent : SimpleDoAfterEvent
{
    // intentionally empty — no EntityUid, no Action refs
}