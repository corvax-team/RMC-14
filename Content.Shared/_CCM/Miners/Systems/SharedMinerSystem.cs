using Content.Shared._CCM.Miners.Components;
using Content.Shared._CCM.Miners.Events;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared._RMC14.TacticalMap;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tools.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._CCM.Miners.Systems;

public abstract class SharedMinerSystem : EntitySystem
{
    private static readonly EntProtoId<SkillDefinitionComponent> EngineerSkill = "RMCSkillEngineer";
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SkillsSystem _rmcSkills = null!;
    [Dependency] private readonly SharedToolSystem _tool = null!;
    [Dependency] protected readonly SharedPopupSystem Popup = null!;
    [Dependency] protected readonly IGameTiming Timing = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MinerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MinerComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<MinerComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<MinerComponent, MinerRepairDoAfterEvent>(OnRepairDoAfter);
        SubscribeLocalEvent<MinerComponent, MinerExtractionDoAfterEvent>(OnExtractionDoAfter);
        SubscribeLocalEvent<MinerComponent, MinerModuleInstallDoAfterEvent>(OnModuleInstallDoAfter);
        SubscribeLocalEvent<MinerComponent, MinerModuleRemoveDoAfterEvent>(OnModuleRemoveDoAfter);
    }

    protected int GetMaxStorage(MinerComponent component)
    {
        return component.MineralStorage;
    }

    protected TimeSpan GetProductionTime(MinerComponent component)
    {
        return component.Modules.Contains(MinerModuleType.Speed)
            ? component.MineralProductionTime / 2
            : component.MineralProductionTime;
    }

    protected void RecalculateState(Entity<MinerComponent> entity)
    {
        if (!TryComp(entity, out DamageableComponent? damageable))
            return;

        var total = damageable.TotalDamage;
        var multiplier = entity.Comp.Modules.Contains(MinerModuleType.Reinforced) ? 2 : 1;

        var small = entity.Comp.SmallDamageThreshold * multiplier;
        var medium = entity.Comp.MediumDamageThreshold * multiplier;
        var destroyed = entity.Comp.DestroyedThreshold * multiplier;

        var newState = MinerState.Running;

        if (total >= destroyed)
            newState = MinerState.Destroyed;
        else if (total >= medium)
            newState = MinerState.MediumDamage;
        else if (total >= small)
            newState = MinerState.SmallDamage;

        if (newState == entity.Comp.State)
            return;

        entity.Comp.State = newState;
        Dirty(entity);
        UpdateAllIcons(entity);
    }

    private void OnExamined(Entity<MinerComponent> entity, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(MinerComponent)))
        {
            var maxStorage = GetMaxStorage(entity.Comp);
            args.PushMarkup(Loc.GetString("miner-examine-storage", ("count", entity.Comp.MineralStored),
                ("max", maxStorage)));

            foreach (var module in entity.Comp.Modules)
            {
                var moduleLoc = module switch
                {
                    MinerModuleType.Automation => "miner-module-automation",
                    MinerModuleType.Speed => "miner-module-speed",
                    MinerModuleType.Reinforced => "miner-module-reinforced",
                    _ => "miner-module-unknown"
                };
                args.PushMarkup(Loc.GetString("miner-examine-module", ("module", Loc.GetString(moduleLoc))));
            }

            if (entity.Comp.State != MinerState.Running)
            {
                var repairMessage = entity.Comp.State switch
                {
                    MinerState.Destroyed => "miner-examine-repair-destroyed",
                    MinerState.MediumDamage => "miner-examine-repair-medium",
                    _ => "miner-examine-repair-small"
                };

                args.PushMarkup(Loc.GetString(repairMessage, ("miner", entity.Owner)));
                return;
            }

            if (entity.Comp.MineralStored >= maxStorage)
                args.PushMarkup(Loc.GetString("miner-examine-full", ("miner", entity.Owner)));
        }
    }

    private void OnInteractHand(Entity<MinerComponent> entity, ref InteractHandEvent args)
    {
        if (entity.Comp.State != MinerState.Running || args.Handled)
            return;

        if (HasComp<ActiveDoAfterComponent>(args.User))
            return;

        args.Handled = true;

        var maxStorage = GetMaxStorage(entity.Comp);
        if (entity.Comp.MineralStored < maxStorage)
        {
            OnEmptyInteract(entity);
            return;
        }

        var delay = entity.Comp.BaseExtractionDelay * _rmcSkills.GetSkillDelayMultiplier(args.User, EngineerSkill);

        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, delay, new MinerExtractionDoAfterEvent(),
            entity.Owner, entity.Owner)
        {
            BreakOnMove = true,
            NeedHand = true,
            BreakOnHandChange = true
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnInteractUsing(Entity<MinerComponent> entity, ref InteractUsingEvent args)
    {
        var user = args.User;
        var used = args.Used;

        if (TryComp<MinerModuleComponent>(used, out var module))
        {
            args.Handled = true;

            if (entity.Comp.State != MinerState.Running)
            {
                Popup.PopupClient(Loc.GetString("miner-module-broken", ("miner", entity.Owner)), entity, user,
                    PopupType.LargeCaution);
                return;
            }

            if (entity.Comp.Modules.Contains(module.Type))
            {
                Popup.PopupClient(Loc.GetString("miner-module-already-installed", ("miner", entity.Owner)), entity,
                    user, PopupType.LargeCaution);
                return;
            }

            var delay = entity.Comp.BaseModuleInstallDelay * _rmcSkills.GetSkillDelayMultiplier(user, EngineerSkill);
            var doAfterArgs = new DoAfterArgs(EntityManager, user, delay,
                new MinerModuleInstallDoAfterEvent(module.Type), entity.Owner, entity.Owner, used: used)
            {
                BreakOnMove = true,
                NeedHand = true,
                BreakOnHandChange = true
            };

            _doAfter.TryStartDoAfter(doAfterArgs);
            return;
        }

        if (_tool.HasQuality(used, entity.Comp.PryingQuality))
        {
            if (!TryGetModuleForRemoval(entity.Comp, out var moduleToRemove))
                return;

            args.Handled = true;

            var delay = entity.Comp.BaseModuleRemovalDelay * _rmcSkills.GetSkillDelayMultiplier(user, EngineerSkill);
            var doAfterArgs = new DoAfterArgs(EntityManager, user, delay,
                new MinerModuleRemoveDoAfterEvent(moduleToRemove), entity.Owner, entity.Owner)
            {
                BreakOnMove = true,
                NeedHand = true,
                BreakOnHandChange = true
            };

            if (_doAfter.TryStartDoAfter(doAfterArgs))
            {
                Popup.PopupClient(Loc.GetString("miner-module-removal-start", ("miner", entity.Owner)), entity.Owner,
                    user);
            }

            return;
        }

        if (_tool.HasQuality(used, entity.Comp.WeldingQuality))
        {
            args.Handled = true;
            TryRepair(entity, user, used, MinerState.Destroyed);
            return;
        }

        if (_tool.HasQuality(used, entity.Comp.CuttingQuality))
        {
            args.Handled = true;
            TryRepair(entity, user, used, MinerState.MediumDamage);
            return;
        }

        if (_tool.HasQuality(used, entity.Comp.WrenchQuality))
        {
            args.Handled = true;
            TryRepair(entity, user, used, MinerState.SmallDamage);
        }
    }

    private bool TryGetModuleForRemoval(MinerComponent component, out MinerModuleType module)
    {
        if (component.Modules.Contains(MinerModuleType.Reinforced))
        {
            module = MinerModuleType.Reinforced;
            return true;
        }

        if (component.Modules.Contains(MinerModuleType.Speed))
        {
            module = MinerModuleType.Speed;
            return true;
        }

        if (component.Modules.Contains(MinerModuleType.Automation))
        {
            module = MinerModuleType.Automation;
            return true;
        }

        module = default;
        return false;
    }

    protected abstract void OnRepairDoAfter(Entity<MinerComponent> entity, ref MinerRepairDoAfterEvent args);
    protected abstract void OnExtractionDoAfter(Entity<MinerComponent> entity, ref MinerExtractionDoAfterEvent args);
    protected abstract void OnModuleInstallDoAfter(Entity<MinerComponent> entity, ref MinerModuleInstallDoAfterEvent args);

    protected abstract void
        OnModuleRemoveDoAfter(Entity<MinerComponent> entity, ref MinerModuleRemoveDoAfterEvent args);

    protected virtual void OnEmptyInteract(Entity<MinerComponent> entity)
    {
    }

    protected void UpdateAllIcons(Entity<MinerComponent> entity)
    {
        UpdateAppearance(entity);
        UpdateIcon(entity);
    }

    private void UpdateAppearance(Entity<MinerComponent> entity)
    {
        var maxStorage = GetMaxStorage(entity.Comp);
        var active = entity.Comp.State == MinerState.Running && entity.Comp.MineralStored < maxStorage;

        _appearance.SetData(entity, MinerVisuals.State, entity.Comp.State);
        _appearance.SetData(entity, MinerVisuals.Active, active);
        _appearance.SetData(entity, MinerVisuals.HasAutomation,
            entity.Comp.Modules.Contains(MinerModuleType.Automation));
        _appearance.SetData(entity, MinerVisuals.HasSpeed, entity.Comp.Modules.Contains(MinerModuleType.Speed));
        _appearance.SetData(entity, MinerVisuals.HasReinforced,
            entity.Comp.Modules.Contains(MinerModuleType.Reinforced));
    }

    private void UpdateIcon(Entity<MinerComponent> entity)
    {
        if (TryComp<TacticalMapIconComponent>(entity, out var iconComponent) && iconComponent.Icon is { } rsi)
        {
            var ensure = EnsureComp<MapBlipIconOverrideComponent>(entity);
            var baseState = rsi.RsiState.Replace("-on", "");
            var maxStorage = GetMaxStorage(entity.Comp);
            var state = entity.Comp.State == MinerState.Running && entity.Comp.MineralStored < maxStorage
                ? $"{baseState}-on"
                : baseState;
            ensure.Icon = new SpriteSpecifier.Rsi(rsi.RsiPath, state);
            Dirty(entity, ensure);
        }
    }

    private void TryRepair(Entity<MinerComponent> entity, EntityUid user, EntityUid used, MinerState state)
    {
        if (entity.Comp.State == MinerState.Running)
        {
            Popup.PopupClient(Loc.GetString("miner-repair-not-needed", ("miner", entity.Owner)), entity, user,
                PopupType.LargeCaution);
            return;
        }

        if (entity.Comp.State != state)
        {
            Popup.PopupClient(Loc.GetString("miner-repair-different-tool", ("miner", entity.Owner)), entity, user,
                PopupType.LargeCaution);
            return;
        }

        var quality = state switch
        {
            MinerState.Destroyed => entity.Comp.WeldingQuality,
            MinerState.MediumDamage => entity.Comp.CuttingQuality,
            MinerState.SmallDamage => entity.Comp.WrenchQuality,
            _ => throw new ArgumentOutOfRangeException()
        };

        var delay = entity.Comp.BaseRepairDelay * _rmcSkills.GetSkillDelayMultiplier(user, EngineerSkill);

        _tool.UseTool(used, user, entity, (float)delay.TotalSeconds, quality, new MinerRepairDoAfterEvent(state),
            entity.Comp.WeldingCost);
    }
}
