using System.Linq;
using Content.Shared._CCM.Pathogen.EntityEffects;
using Content.Shared._RMC14.Chemistry.Reagent;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;

namespace Content.Shared._CCM.EntityEffects;

public sealed class CCMEntityEffectSystem : EntitySystem
{
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly RMCReagentSystem _reagentSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    private ISawmill _sawmill = default!;
    private List<(EntityUid, ReagentId)> _infected = new List<(EntityUid, ReagentId)>();
    public override void Initialize()
    {
        SubscribeLocalEvent<ExecuteEntityEffectEvent<PathogenInfection>>(OnExecutePathogenInfection);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<BirthBloodburster>>(OnExecuteBirthBloodburster);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChangeMetabolismRate>>(OnExecuteMetabolismRate);
    }

    private void OnExecutePathogenInfection(ref ExecuteEntityEffectEvent<PathogenInfection> args)
    {
        _sawmill = _log.GetSawmill("CCMEntityEffectSystem.OnExecutePathogenInfection");

        if (!TryComp<SolutionContainerManagerComponent>(args.Args.TargetEntity, out var manager))
        {
            _sawmill.Debug($"Entity {args.Args.TargetEntity}, don't have SolutionContainerManagerComponent");
            return;
        }
        _sawmill.Debug($"Entity {args.Args.TargetEntity}, successful find SolutionContainerManagerComponent");

        var rate = args.Effect.Amount / args.Effect.Seconds;

        foreach (var (solutionName, solutionEntity) in _solution.EnumerateSolutions((args.Args.TargetEntity, manager)))
        {
            _sawmill.Debug($"Entity {args.Args.TargetEntity}, solutionName: {solutionName}, solutionEntity: {solutionEntity}");

            var solution = solutionEntity.Comp.Solution;

            foreach (var reagent in solution.Contents)
            {
                var reagentId = reagent.Reagent;
                if (!_reagentSystem.TryIndex(reagentId, out var index) ||
                    index.Metabolisms == null)
                    break;

                foreach (var (_, effects) in index.Metabolisms.ToList())
                {
                    var isPathogenInfection = false;
                    foreach (var effect in effects.Effects.ToList())
                    {
                        _sawmill.Debug($"find effect {effect}");
                        if (isPathogenInfection)
                            continue;
                        if (effect.GetType().Name == args.Effect.GetType().Name)
                            isPathogenInfection = true;
                    }
                    if (isPathogenInfection)
                    {
                        if (!_infected.Contains((args.Args.TargetEntity, reagentId)))
                            _infected.Add((args.Args.TargetEntity, reagentId));
                        _sawmill.Debug($"Entity {args.Args.TargetEntity} with infection ({reagent}, {reagentId}), set rate to {rate} from {effects.MetabolismRate}");
                        effects.MetabolismRate = FixedPoint2.New(rate);
                        _sawmill.Debug($"Current metabolismRate is {effects.MetabolismRate}");
                    }
                    else
                    {
                        _sawmill.Debug($"Entity {args.Args.TargetEntity} without infection. {reagent}, {reagentId}");
                    }
                }
            }
        }
    }

    private void OnExecuteBirthBloodburster(ref ExecuteEntityEffectEvent<BirthBloodburster> args)
    {
    }

    private void OnExecuteMetabolismRate(ref ExecuteEntityEffectEvent<ChangeMetabolismRate> args)
    {
    }

    // public override void Update(float frameTime)
    // {
    //     _sawmill = _log.GetSawmill("CCMEntityEffectSystem.Update");
    //     base.Update(frameTime);
    //     foreach (var (ent, reagent) in _infected)
    //     {
    //         if (!_reagentSystem.TryIndex(reagent, out var index))
    //         {
    //             _sawmill.Debug($"Cant index {reagent} on entity {ent}");
    //             break;
    //         }
    //     }
    // }
}
