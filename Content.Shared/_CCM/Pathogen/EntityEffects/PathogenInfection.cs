using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.Pathogen.EntityEffects;

[DataDefinition]
public sealed partial class PathogenInfection : EventEntityEffect<PathogenInfection>
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-emp-reaction-effect", ("chance", Probability)); // TODO сделать гайдбук для патогена

    [DataField]
    public float Amount = 1f;

    [DataField]
    public float Seconds = 1f;

    [DataField]
    public FixedPoint2? Rate;
}

