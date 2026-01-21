using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.EntityEffects;

[DataDefinition]
public sealed partial class ChangeMetabolismRate : EventEntityEffect<ChangeMetabolismRate>
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-rate-effect"); // TODO сделать гайдбук для изменения рейта усваивания

    public float Amount = 1f;
    public float Seconds = 1f;
    public FixedPoint2 Rate;
}
