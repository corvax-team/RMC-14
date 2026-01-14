using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.Pathogen.EntityEffects;

[DataDefinition]
public sealed partial class PathogenInfection : EventEntityEffect<PathogenInfection>
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-emp-reaction-effect", ("chance", Probability)); // TODO сделать гайдбук для патогена
}
