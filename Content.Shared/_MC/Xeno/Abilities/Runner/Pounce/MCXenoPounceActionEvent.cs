using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce;

public sealed partial class MCXenoPounceActionEvent : WorldTargetActionEvent
{
    [DataField]
    public FixedPoint2 PlasmaCost = 50;
}
