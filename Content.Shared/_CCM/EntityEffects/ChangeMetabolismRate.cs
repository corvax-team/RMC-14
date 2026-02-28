using System.Linq;
using Content.Shared._RMC14.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.EntityEffects;

[DataDefinition]
public sealed partial class ChangeMetabolismRate : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-metabolism-rate-effect"); // TODO сделать гайдбук для изменения рейта усваивания

    [DataField]
    public FixedPoint2 Amount = FixedPoint2.Epsilon;

    [DataField]
    public FixedPoint2 Seconds = FixedPoint2.Epsilon;

    [DataField]
    public FixedPoint2? Rate;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs reagentArgs)
            return;

        var rate = Rate.HasValue ? Rate.Value : Amount / Seconds;
        // var metabolizerRate = TimeSpan.FromSeconds(1.5f);
        // if (!TryComp<Metabol>)

        if (reagentArgs.Reagent is null)
            return;

        var reagent = reagentArgs.Reagent;
        var reagentSystem = args.EntityManager.System<RMCReagentSystem>();

        if (!reagentSystem.TryIndex(reagent, out var proto) ||
            proto.Metabolisms == null)
            return;

        foreach (var (_, effects) in proto.Metabolisms.ToList())
        {
            var find = false;
            foreach (var effect in effects.Effects.ToList())
            {
                if (find)
                    continue;
                if (effect.GetType().Name is "ChangeMetabolismRate")
                    find = true;
            }

            if (find)
                effects.MetabolismRate = rate;
        }
    }
}
