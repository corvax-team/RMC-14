using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Bull.HeadbuttCharge;

[NetSerializable, Serializable]
public sealed partial class MCXenoHeadbuttChargeDoAfterEvent : DoAfterEvent
{
    public NetEntity Action { get; }

    public MCXenoHeadbuttChargeDoAfterEvent(NetEntity action)
    {
        Action = action;
    }

    public override DoAfterEvent Clone()
    {
        return new MCXenoHeadbuttChargeDoAfterEvent(Action);
    }
}