using Content.Shared._CCM.Miners.Components;
using Content.Shared._CCM.Miners.Events;
using Content.Shared._CCM.Miners.Systems;
using Content.Shared._RMC14.Requisitions;
using Content.Shared._RMC14.Requisitions.Components;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CCM.Miners.Systems;

public sealed class MinerSystem : SharedMinerSystem
{
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly IPrototypeManager _prototype = null!;
    [Dependency] private readonly SharedRequisitionsSystem _requisitions = null!;
    [Dependency] private readonly AudioSystem _audio = null!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambient = null!;
    private readonly Dictionary<EntProtoId, int?> _crateRewardCache = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MinerComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<MinerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<MinerComponent> entity, ref MapInitEvent args)
    {
        entity.Comp.NextMineralProduction = Timing.CurTime + GetProductionTime(entity.Comp);
        _ambient.SetAmbience(entity, IsActivelyDrilling(entity.Comp));
        UpdateAllIcons(entity);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MinerComponent>();
        while (query.MoveNext(out var uid, out var miner))
        {
            _ambient.SetAmbience(uid, IsActivelyDrilling(miner));

            if (miner.State != MinerState.Running)
                continue;

            var didChange = false;
            while (miner.State == MinerState.Running && Timing.CurTime >= miner.NextMineralProduction)
            {
                var maxStorage = GetMaxStorage(miner);

                if (TryProcessAutomationSale(uid, miner, maxStorage))
                {
                    didChange = true;
                    miner.NextMineralProduction = Timing.CurTime + GetProductionTime(miner);
                    continue;
                }

                if (miner.MineralStored >= maxStorage)
                    break;

                var productionTime = GetProductionTime(miner);
                miner.MineralStored++;
                miner.NextMineralProduction += productionTime;
                didChange = true;
            }

            if (didChange)
            {
                Dirty(uid, miner);
                UpdateAllIcons((uid, miner));
            }
        }
    }

    protected override void OnExtractionDoAfter(Entity<MinerComponent> entity, ref MinerExtractionDoAfterEvent args)
    {
        var maxStorage = GetMaxStorage(entity.Comp);
        if (args.Cancelled || args.Handled || entity.Comp.MineralStored < maxStorage)
            return;

        args.Handled = true;

        var xform = Transform(entity);
        var offset = xform.LocalRotation.ToWorldVec();

        Spawn(entity.Comp.OreCratePrototype, xform.Coordinates.Offset(offset));

        entity.Comp.MineralStored = 0;
        entity.Comp.NextMineralProduction = Timing.CurTime + GetProductionTime(entity.Comp);

        Dirty(entity);
        UpdateAllIcons(entity);
    }

    protected override void OnModuleRemoveDoAfter(Entity<MinerComponent> entity, ref MinerModuleRemoveDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !entity.Comp.Modules.Contains(args.ModuleType))
            return;

        args.Handled = true;

        var moduleId = args.ModuleType switch
        {
            MinerModuleType.Automation => entity.Comp.AutomationModulePrototype,
            MinerModuleType.Speed => entity.Comp.SpeedModulePrototype,
            MinerModuleType.Reinforced => entity.Comp.ReinforcedModulePrototype,
            _ => (EntProtoId?) null
        };

        if (moduleId != null) Spawn(moduleId.Value, Transform(entity).Coordinates);

        entity.Comp.Modules.Remove(args.ModuleType);
        RecalculateState(entity);

        Dirty(entity);
        UpdateAllIcons(entity);
        Popup.PopupEntity(Loc.GetString("miner-module-removed", ("miner", entity.Owner)), entity.Owner, args.User);
    }

    protected override void OnModuleInstallDoAfter(Entity<MinerComponent> entity, ref MinerModuleInstallDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || entity.Comp.State != MinerState.Running || entity.Comp.Modules.Contains(args.ModuleType))
            return;

        if (args.Used is not { } used ||
            !TryComp<MinerModuleComponent>(used, out var module) ||
            module.Type != args.ModuleType)
        {
            return;
        }

        args.Handled = true;
        entity.Comp.Modules.Add(module.Type);

        RecalculateState(entity);

        Dirty(entity);
        UpdateAllIcons(entity);
        Popup.PopupEntity(Loc.GetString("miner-module-installed", ("module", used), ("miner", entity.Owner)),
            entity.Owner, args.User);
        QueueDel(used);
    }

    protected override void OnRepairDoAfter(Entity<MinerComponent> entity, ref MinerRepairDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || entity.Comp.State != args.State)
            return;

        args.Handled = true;

        var repairedState = args.State switch
        {
            MinerState.Destroyed => MinerState.MediumDamage,
            MinerState.MediumDamage => MinerState.SmallDamage,
            MinerState.SmallDamage => MinerState.Running,
            _ => entity.Comp.State
        };

        if (TryComp(entity, out DamageableComponent? damageable))
        {
            var multiplier = entity.Comp.Modules.Contains(MinerModuleType.Reinforced) ? 2 : 1;
            var targetDamage = repairedState switch
            {
                MinerState.Running => FixedPoint2.Zero,
                MinerState.SmallDamage => entity.Comp.SmallDamageThreshold * multiplier + 5,
                MinerState.MediumDamage => entity.Comp.MediumDamageThreshold * multiplier + 5,
                _ => damageable.TotalDamage
            };

            SetTargetTotalDamage(entity, damageable, targetDamage);
            RecalculateState(entity);
        }
        else
            entity.Comp.State = repairedState;

        if (entity.Comp.State == MinerState.Running)
            entity.Comp.NextMineralProduction = Timing.CurTime + GetProductionTime(entity.Comp);

        Dirty(entity);
        UpdateAllIcons(entity);
    }

    private void OnDamageChanged(Entity<MinerComponent> entity, ref DamageChangedEvent args)
    {
        RecalculateState(entity);
    }

    private void SetTargetTotalDamage(EntityUid uid, DamageableComponent damageable, FixedPoint2 targetTotalDamage)
    {
        if (targetTotalDamage <= FixedPoint2.Zero)
        {
            _damageable.SetAllDamage(uid, damageable, FixedPoint2.Zero);
            return;
        }

        if (damageable.TotalDamage <= FixedPoint2.Zero)
        {
            _damageable.SetAllDamage(uid, damageable, FixedPoint2.Zero);

            foreach (var damageType in damageable.Damage.DamageDict.Keys)
            {
                var newDamage = new DamageSpecifier
                {
                    DamageDict =
                    {
                        [damageType] = targetTotalDamage
                    }
                };
                _damageable.TryChangeDamage(uid, newDamage, true, false);
                break;
            }

            return;
        }

        var scale = targetTotalDamage.Float() / damageable.TotalDamage.Float();
        _damageable.SetDamage(uid, damageable, damageable.Damage * scale);
    }

    private bool TryGetCrateReward(EntProtoId cratePrototype, out int reward)
    {
        if (_crateRewardCache.TryGetValue(cratePrototype, out var cachedReward))
        {
            reward = cachedReward ?? 0;
            return cachedReward != null;
        }

        reward = 0;
        if (!_prototype.TryIndex<EntityPrototype>(cratePrototype, out var crateEntityPrototype) ||
            !crateEntityPrototype.TryGetComponent<RequisitionsCrateComponent>(out var crateComp, EntityManager.ComponentFactory))
        {
            _crateRewardCache[cratePrototype] = null;
            return false;
        }

        reward = crateComp.Reward;
        _crateRewardCache[cratePrototype] = reward;
        return true;
    }

    private bool TryProcessAutomationSale(EntityUid uid, MinerComponent miner, int maxStorage)
    {
        if (miner.MineralStored < maxStorage || !miner.Modules.Contains(MinerModuleType.Automation))
            return false;

        if (TryGetCrateReward(miner.OreCratePrototype, out var reward))
        {
            _requisitions.ChangeBudget(reward);
            _audio.PlayPvs(miner.AutoSellSound, uid);
        }

        miner.MineralStored = 0;
        return true;
    }

    private bool IsActivelyDrilling(MinerComponent miner)
    {
        return miner.State == MinerState.Running && miner.MineralStored < GetMaxStorage(miner);
    }
}
