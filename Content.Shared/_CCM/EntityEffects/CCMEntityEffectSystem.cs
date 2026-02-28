using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Content.Shared._CCM.Pathogen.Actions;
using Content.Shared._CCM.Pathogen.EntityEffects;
using Content.Shared._CCM.Pathogen.Protomorphs.Components;
using Content.Shared._RMC14.Chemistry.Reagent;
using Content.Shared.Actions;
using Content.Shared.Body.Prototypes;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.EntityEffects;

public sealed class CCMEntityEffectSystem : EntitySystem
{
    [Dependency] private readonly RMCReagentSystem _reagentSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    private List<(EntityUid, ReagentId)> _infected = new List<(EntityUid, ReagentId)>();
    private Dictionary<ReagentPrototype, (ProtoId<MetabolismGroupPrototype>, ReagentEffectsEntry)> _pathogen = new Dictionary<ReagentPrototype, (ProtoId<MetabolismGroupPrototype>, ReagentEffectsEntry)>();
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<PathogenInfection>>(OnExecutePathogenInfection);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<BirthBloodburster>>(OnExecuteBirthBloodburster);
        SubscribeLocalEvent<BloodbursterComponent, BirthBloodbursterActionEvent>(OnActionTrigged);

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReload);
    }

    private void OnExecutePathogenInfection(ref ExecuteEntityEffectEvent<PathogenInfection> args)
    {
        /// we just add this, for nothing

        // var rate = args.Effect.Amount / args.Effect.Seconds;

        // if (!ReagentEntry(args.Args.TargetEntity, args.Effect, out var entry))
        //     return;

        // foreach (var (_, _, _, reagentEntry) in entry)
        // {
        //     reagentEntry.MetabolismRate = rate;
        // }
    }

    private void OnExecuteBirthBloodburster(ref ExecuteEntityEffectEvent<BirthBloodburster> args)
    {
        if (args.Args is not EntityEffectReagentArgs reagentArgs)
            return;

        if (!TryGetReagentEntryFromEffect(args.Args.TargetEntity, args.Effect, out var entry))
            return;

        foreach (var (reagentId, solution, group, reagentEntry) in entry)
        {
            var quantity = new ReagentQuantity(reagentId, _solution.GetTotalPrototypeQuantity(args.Args.TargetEntity, reagentId.Prototype) + 1);
            solution.Comp.Solution.RemoveReagent(quantity, ignoreReagentData: true);
        }

        var target = args.Args.TargetEntity;
        _container.EnsureContainer<ContainerSlot>(target, "bloodburster");
        Log.Debug($"Added container 'bloodburster' to entity {ToPrettyString(target)}");
        var bloodburster = Spawn(args.Effect.Bloodburster);
        Log.Debug($"Spawned {ToPrettyString(bloodburster)} into void");
        if (!_container.TryGetContainer(target, "bloodburster", out var container))
        {
            Log.Debug($"Cannot take container 'bloodburster' from {ToPrettyString(target)}");
            return;
        }
        if (!_container.Insert(bloodburster, container))
        {
            Log.Debug($"Cannot insert {ToPrettyString(bloodburster)} to {ToPrettyString(target)} with container {ToPrettyString(EntityUid.Parse(container.ID))}");
            return;
        }

        var comp = EnsureComp<BloodbursterComponent>(bloodburster);
        comp.ActionId = _action.AddAction(bloodburster, comp.Action);
    }

    private void OnActionTrigged(EntityUid uid, BloodbursterComponent comp, ref BirthBloodbursterActionEvent args)
    {
        if (!_container.TryGetContainingContainer(uid, out var container))
            return;

        if (!_container.Remove(uid, container))
            return;

        _damage.TryChangeDamage(container.Owner, comp.Damage, ignoreResistances: true);
        _action.RemoveAction(args.Performer, comp.ActionId);

        comp.ActionId = null;
    }

    private void OnPrototypesReload(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<ReagentPrototype>())
            return;

        _pathogen.Clear();
        foreach (var reagent in IoCManager.Resolve<IPrototypeManager>().EnumeratePrototypes<ReagentPrototype>())
        {
            if (reagent.Metabolisms == null)
                continue;

            foreach (var (metabolismGroup, reagentEntry) in reagent.Metabolisms)
            {
                foreach (var effect in reagentEntry.Effects)
                {
                    if (effect.GetType().Name == "PathogenInfection")
                        _pathogen.Add(reagent, (metabolismGroup, reagentEntry));
                }
            }
        }
    }

    private bool TryGetReagentEntryFromEffect(EntityUid uid, EntityEffect findEffect, [NotNullWhen(true)] out List<(ReagentId, Entity<SolutionComponent>, ProtoId<MetabolismGroupPrototype>, ReagentEffectsEntry)>? entry)
    {
        entry = null;

        if (!TryComp<SolutionContainerManagerComponent>(uid, out var manager))
        {
            Log.Debug($"Entity {uid}, don't have SolutionContainerManagerComponent");
            return false;
        }

        foreach (var (solutionName, solutionEntity) in _solution.EnumerateSolutions((uid, manager)))
        {
            foreach (var reagent in solutionEntity.Comp.Solution.Contents)
            {
                var reagentId = reagent.Reagent;
                if (!_reagentSystem.TryIndex(reagentId, out var index) ||
                    index.Metabolisms == null)
                    continue;
                foreach (var (group, reagentEntry) in index.Metabolisms)
                {
                    foreach (var effect in reagentEntry.Effects)
                    {
                        if (effect.GetType().Name == findEffect.GetType().Name)
                        {
                            if (entry == null)
                                entry = new List<(ReagentId, Entity<SolutionComponent>, ProtoId<MetabolismGroupPrototype>, ReagentEffectsEntry)>();
                            entry.Add((reagentId, solutionEntity, group, reagentEntry));
                        }
                    }
                }
            }
        }
        if (entry == null)
            return false;
        return true;
    }
}
