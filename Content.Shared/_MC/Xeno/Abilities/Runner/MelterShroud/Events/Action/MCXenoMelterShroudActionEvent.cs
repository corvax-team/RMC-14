using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Runner.MelterShroud.Events.Action;

public sealed partial class MCXenoMelterShroudActionEvent : InstantActionEvent
{
    [DataField]
    public FixedPoint2 PlasmaCost = 0;

    [DataField]
    public int EnergyCost = 0;
}
