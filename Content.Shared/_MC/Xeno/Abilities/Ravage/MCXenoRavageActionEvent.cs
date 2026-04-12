using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Ravage;

public sealed partial class MCXenoRavageActionEvent : InstantActionEvent
{
    [DataField]
    public FixedPoint2 PlasmaCost = 200;
}
