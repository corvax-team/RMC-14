using Content.Shared.Actions;
using Content.Shared.FixedPoint;

namespace Content.Shared._CCM.Xenonids.Abilities.Runi.Charge;

public sealed partial class CCMXenoChargeLineActionEvent : InstantActionEvent
{
    [DataField]
    public FixedPoint2 PlasmaCost = 80;
}
