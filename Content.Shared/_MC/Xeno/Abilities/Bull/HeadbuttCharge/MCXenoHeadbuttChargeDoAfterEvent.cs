using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Bull.HeadbuttCharge;

[NetSerializable, Serializable]
public sealed partial class MCXenoHeadbuttChargeDoAfterEvent : DoAfterEvent
{
    public NetEntity Action { get; }
    public NetCoordinates TargetCoordinates { get; }

    public MCXenoHeadbuttChargeDoAfterEvent(NetEntity action, NetCoordinates targetCoordinates)
    {
        Action = action;
        TargetCoordinates = targetCoordinates;
    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
