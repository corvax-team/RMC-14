using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Crusher.Charge;

public sealed partial class MCXenoChargeActionEvent : InstantActionEvent
{
    [DataField]
    public FixedPoint2 PlasmaCost = 40;
}
