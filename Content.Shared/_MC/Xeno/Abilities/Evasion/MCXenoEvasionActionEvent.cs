using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Content.Shared.FixedPoint;

namespace Content.Shared._MC.Xeno.Abilities.Evasion;

public sealed partial class MCXenoEvasionActionEvent : InstantActionEvent
{
    [DataField]
    public FixedPoint2 PlasmaCost = 70;
}