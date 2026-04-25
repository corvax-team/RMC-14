using System.Collections.Immutable;
using System.Linq;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared._RMC14.Vehicle;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;

namespace Content.Shared._CCM.Vehicle.Fabricator;

public sealed class VehicleFabricatorSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SkillsSystem _skills = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    private ImmutableArray<EntProtoId> Printables { get; set; }
    private readonly Dictionary<EntProtoId, (VehicleFabricatorCategory Category, VehicleType Vehicle)> _printableInfoCache = new();
    private Dictionary<VehicleType, HashSet<VehicleFabricatorCategory>>? _availableCategoriesCache;

    private readonly Dictionary<EntProtoId, CachedModulePrintableInfo> _cachedModulePrintableInfos = new();
    private readonly HashSet<EntityUid> _activeFabricators = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        SubscribeLocalEvent<VehicleFabricatorComponent, MapInitEvent>(OnFabricatorMapInit);
        SubscribeLocalEvent<VehicleFabricatorComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<VehicleFabricatorComponent, VehicleFabricatorRecycleDoafterEvent>(OnVehiclePartRecycled);
        SubscribeLocalEvent<VehicleFabricatorComponent, ComponentShutdown>(OnFabricatorShutdown);

        Subs.BuiEvents<VehicleFabricatorComponent>(VehicleFabricatorUi.Key,
            subs =>
            {
                subs.Event<VehicleFabricatorPrintMsg>(OnPrintMsg);
            });

        ReloadPrototypes();
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs ev)
    {
        if (ev.WasModified<EntityPrototype>())
        {
            ReloadPrototypes();
            _availableCategoriesCache = null;
        }
    }

    private void OnFabricatorMapInit(Entity<VehicleFabricatorComponent> ent, ref MapInitEvent args)
    {
        if (!_net.IsServer || TerminatingOrDeleted(ent))
            return;

        SendUIState(ent, ent.Comp);
    }

    private void OnFabricatorShutdown(Entity<VehicleFabricatorComponent> ent, ref ComponentShutdown args)
    {
        _activeFabricators.Remove(ent);
    }

    private void OnInteractUsing(Entity<VehicleFabricatorComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || TerminatingOrDeleted(ent))
            return;

        if (!TryGetPrintableInfo(args.Used, ent.Comp, out var printable))
            return;

        args.Handled = true;

        var delay = printable.Delay;
        var multiplier = _skills.GetSkillDelayMultiplier(args.User, printable.RecycleSkill);
        delay = TimeSpan.FromSeconds(delay.TotalSeconds * multiplier);

        var ev = new VehicleFabricatorRecycleDoafterEvent();
        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, delay, ev, ent, ent, args.Used)
        {
            BreakOnMove = true,
            DuplicateCondition = DuplicateConditions.SameEvent,
            NeedHand = true
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private bool TryGetPrintableInfo(EntityUid entity, VehicleFabricatorComponent comp, out VehicleFabricatorPrintableInfo info)
    {
        info = default;
        if (!TryComp(entity, out MetaDataComponent? meta))
            return false;

        return TryGetPrintableInfoForProto(meta.EntityPrototype, comp, out info);
    }

    private readonly record struct VehicleFabricatorPrintableInfo(
        TimeSpan Delay,
        EntProtoId<SkillDefinitionComponent> RecycleSkill,
        VehicleFabricatorCategory Category,
        VehicleType Vehicle);

    private void OnVehiclePartRecycled(Entity<VehicleFabricatorComponent> ent, ref VehicleFabricatorRecycleDoafterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Used == null || TerminatingOrDeleted(ent))
            return;

        if (!TryGetPrintableInfo(args.Used.Value, ent.Comp, out var printable))
            return;

        args.Handled = true;
        Del(args.Used.Value);

        _audio.PlayPvs(ent.Comp.RecycleSound, ent);
        _popup.PopupEntity(Loc.GetString("rmc-vehicle-fabricator-recycled"), ent, args.User);

        var limitKey = VehicleFabricatorUtils.GetLimitKey(printable.Category, printable.Vehicle);
        ent.Comp.PrintedModules[limitKey] = Math.Max(0, ent.Comp.PrintedModules.GetValueOrDefault(limitKey, 0) - 1);

        Dirty(ent);
        SendUIState(ent.Owner, ent.Comp);
    }

    private void OnPrintMsg(Entity<VehicleFabricatorComponent> ent, ref VehicleFabricatorPrintMsg args)
    {
        if (args.Id == default || TerminatingOrDeleted(ent) || !_prototypes.TryIndex(args.Id, out var proto))
            return;

        if (!TryGetPrintableInfoForProto(proto, ent.Comp, out var printable))
            return;

        var actor = args.Actor;
        if (ent.Comp.Printing != null)
        {
            _popup.PopupClient(Loc.GetString("rmc-vehicle-fabricator-busy"), actor, actor, PopupType.SmallCaution);
            return;
        }

        var limitKey = VehicleFabricatorUtils.GetLimitKey(printable.Category, printable.Vehicle);
        if (ent.Comp.PrintedModules.GetValueOrDefault(limitKey, 0) >= 1)
        {
            _popup.PopupClient(Loc.GetString("rmc-vehicle-fabricator-limit-reached"), actor, actor, PopupType.SmallCaution);
            return;
        }

        ent.Comp.Printing = proto.ID;
        ent.Comp.PrintAt = _timing.CurTime + printable.Delay;
        Dirty(ent);

        _activeFabricators.Add(ent);
        _appearance.SetData(ent, VehicleFabricatorVisuals.State, VehicleFabricatorState.Fabricating);
        _audio.PlayPvs(ent.Comp.ClickSound, ent);
        SendUIState(ent.Owner, ent.Comp);
    }

    private bool TryGetPrintableInfoForProto(EntityPrototype? proto, VehicleFabricatorComponent comp, out VehicleFabricatorPrintableInfo info)
    {
        info = default;
        if (proto == null)
            return false;

        if (!TryGetAutoPrintableInfo(proto, out var autoInfo))
            return false;

        info = new VehicleFabricatorPrintableInfo(
            comp.DefaultPrintDelay,
            comp.RecycleSkillId,
            autoInfo.Category,
            autoInfo.Vehicle
        );
        return true;
    }

    private sealed record CachedModulePrintableInfo(
        VehicleType Vehicle,
        string? HardpointItemType,
        ImmutableArray<VehicleFabricatorCategory> SubSlotCategories);

    private void ReloadPrototypes()
    {
        _printableInfoCache.Clear();
        _cachedModulePrintableInfos.Clear();

        var printables = CollectPrintablePrototypes();
        SortAndCachePrintables(printables);
    }

    private List<(EntityPrototype Proto, (VehicleFabricatorCategory Category, VehicleType Vehicle) Info)> CollectPrintablePrototypes()
    {
        var prototypes = _prototypes.EnumeratePrototypes<EntityPrototype>();
        var printables = new List<(EntityPrototype Proto, (VehicleFabricatorCategory Category, VehicleType Vehicle) Info)>();

        foreach (var proto in prototypes)
        {
            if (TryGetAutoPrintableInfo(proto, out var info))
            {
                printables.Add((proto, info));
                CacheModulePrintableInfo(proto, info);
            }
        }

        return printables;
    }

    private void CacheModulePrintableInfo(EntityPrototype proto, (VehicleFabricatorCategory Category, VehicleType Vehicle) info)
    {
        string? hardpointItemType = null;
        if (proto.TryGetComponent(out HardpointItemComponent? hardpointItem, _compFactory))
        {
            hardpointItemType = hardpointItem.HardpointType;
        }

        var subSlotCategories = ExtractSubSlotCategories(proto);

        _cachedModulePrintableInfos[new EntProtoId(proto.ID)] = new CachedModulePrintableInfo(
            info.Vehicle,
            hardpointItemType,
            subSlotCategories
        );
    }

    private ImmutableArray<VehicleFabricatorCategory> ExtractSubSlotCategories(EntityPrototype proto)
    {
        if (!proto.TryGetComponent(out HardpointSlotsComponent? moduleSlots, _compFactory))
            return ImmutableArray<VehicleFabricatorCategory>.Empty;

        var categories = new HashSet<VehicleFabricatorCategory>();
        foreach (var subSlot in moduleSlots.Slots)
        {
            var category = VehicleFabricatorUtils.GetCategoryFromHardpointType(subSlot.HardpointType);
            categories.Add(category);
        }
        return categories.ToImmutableArray();
    }

    private void SortAndCachePrintables(List<(EntityPrototype Proto, (VehicleFabricatorCategory Category, VehicleType Vehicle) Info)> printables)
    {
        printables.Sort((a, b) => string.Compare(a.Proto.Name, b.Proto.Name, StringComparison.OrdinalIgnoreCase));
        Printables = printables.Select(e => new EntProtoId(e.Proto.ID)).ToImmutableArray();

        foreach (var (proto, info) in printables)
        {
            _printableInfoCache[new EntProtoId(proto.ID)] = info;
        }
    }

    private bool TryGetAutoPrintableInfo(EntityPrototype? prototype, out (VehicleFabricatorCategory Category, VehicleType Vehicle) info)
    {
        info = default;
        if (prototype == null || prototype.Abstract)
            return false;

        if (!prototype.TryGetComponent(out HardpointItemComponent? hardpointItem, _compFactory))
            return false;

        if (!TryGetCategoryFromHardpointType(hardpointItem.HardpointType, out var category))
            return false;

        GetVehicleFromFamily(hardpointItem.VehicleFamily, out var vehicle);

        info = (category, vehicle);
        return true;
    }

    private bool IsPrintableHidden(EntityPrototype? prototype, VehicleFabricatorComponent fabricatorComp)
    {
        if (prototype == null)
            return false;

        if (fabricatorComp.HiddenCompatibilityIds.Count == 0)
            return false;

        if (!prototype.TryGetComponent(out HardpointItemComponent? hardpointItem, _compFactory))
            return false;

        if (string.IsNullOrEmpty(hardpointItem.CompatibilityId))
            return false;

        return fabricatorComp.HiddenCompatibilityIds.Contains(hardpointItem.CompatibilityId);
    }

    private bool TryGetCategoryFromHardpointType(string? hardpointType, out VehicleFabricatorCategory category)
    {
        category = VehicleFabricatorCategory.Support;
        if (string.IsNullOrEmpty(hardpointType))
            return false;

        category = VehicleFabricatorUtils.GetCategoryFromHardpointType(hardpointType);
        return true;
    }

    private void GetVehicleFromFamily(ProtoId<HardpointVehicleFamilyPrototype>? vehicleFamily,
        out VehicleType vehicle)
    {
        vehicle = VehicleType.None;
        if (vehicleFamily == null) return;

        var familyStr = vehicleFamily.Value.ToString();
        vehicle = familyStr switch
        {
            "humvee" => VehicleType.Humvee,
            "Humvee" => VehicleType.Humvee,
            "apc" => VehicleType.APC,
            "APC" => VehicleType.APC,
            "tank" => VehicleType.Tank,
            "Tank" => VehicleType.Tank,
            "van" => VehicleType.Van,
            "Van" => VehicleType.Van,
            _ => VehicleType.None
        };
    }

    private Dictionary<VehicleType, HashSet<VehicleFabricatorCategory>> GetAvailableCategoriesForAllVehicles()
    {
        if (_availableCategoriesCache != null)
        {
            var copy = new Dictionary<VehicleType, HashSet<VehicleFabricatorCategory>>();
            foreach (var (vehicleType, categories) in _availableCategoriesCache)
            {
                copy[vehicleType] = new HashSet<VehicleFabricatorCategory>(categories);
            }
            return copy;
        }

        var result = new Dictionary<VehicleType, HashSet<VehicleFabricatorCategory>>();

        foreach (var vehicleType in Enum.GetValues<VehicleType>())
        {
            if (vehicleType == VehicleType.None)
                continue;

            var protoId = VehicleFabricatorUtils.GetVehicleProtoId(vehicleType);
            if (protoId == null || !_prototypes.TryIndex(protoId.Value, out var proto))
                continue;

            var categories = GetCategoriesFromVehiclePrototype(proto);
            result[vehicleType] = categories;
        }

        _availableCategoriesCache = result;
        return result;
    }

    private HashSet<VehicleFabricatorCategory> GetCategoriesFromVehiclePrototype(EntityPrototype proto)
    {
        var categories = new HashSet<VehicleFabricatorCategory>();

        if (!proto.TryGetComponent(out HardpointSlotsComponent? hardpointSlots, _compFactory))
            return categories;

        var vehicleFamily = hardpointSlots.VehicleFamily;

        foreach (var slot in hardpointSlots.Slots)
        {
            var category = VehicleFabricatorUtils.GetCategoryFromHardpointType(slot.HardpointType);
            categories.Add(category);

            var moduleCategories = GetCategoriesFromModuleSlot(slot.HardpointType, vehicleFamily);
            foreach (var moduleCategory in moduleCategories)
            {
                categories.Add(moduleCategory);
            }
        }

        return categories;
    }

    private HashSet<VehicleFabricatorCategory> GetCategoriesFromModuleSlot(string hardpointType, ProtoId<HardpointVehicleFamilyPrototype>? vehicleFamily)
    {
        var categories = new HashSet<VehicleFabricatorCategory>();
        GetVehicleFromFamily(vehicleFamily, out var vehicleType);

        foreach (var (_, cachedInfo) in _cachedModulePrintableInfos)
        {
            if (cachedInfo.Vehicle != vehicleType)
                continue;

            if (string.IsNullOrEmpty(cachedInfo.HardpointItemType) ||
                !cachedInfo.HardpointItemType.Equals(hardpointType, StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (var subCategory in cachedInfo.SubSlotCategories)
            {
                categories.Add(subCategory);
            }
        }

        return categories;
    }


    private void SendUIState(EntityUid uid, VehicleFabricatorComponent comp)
    {
        string? printingName = null;
        TimeSpan? printAt = null;
        var printDelay = TimeSpan.Zero;
        var printables = new List<VehicleFabricatorPrintableEntry>();

        if (comp.Printing != null && _prototypes.TryIndex(comp.Printing.Value, out var proto))
        {
            printingName = proto.Name;
            printAt = comp.PrintAt;
            printDelay = comp.DefaultPrintDelay;
        }

        foreach (var printableId in Printables)
        {
            if (!_prototypes.TryIndex(printableId, out var printableProto))
                continue;

            if (!_printableInfoCache.TryGetValue(printableId, out var autoInfo))
                continue;

            if (IsPrintableHidden(printableProto, comp))
                continue;

            printables.Add(new VehicleFabricatorPrintableEntry(
                printableId,
                printableProto.Name,
                printableProto.Description,
                autoInfo.Category,
                autoInfo.Vehicle
            ));
        }

        var state = new VehicleFabricatorBuiState(
            printingName,
            printAt,
            printDelay,
            printables,
            comp.PrintedModules,
            GetAvailableCategoriesForAllVehicles()
        );

        _ui.SetUiState(uid, VehicleFabricatorUi.Key, state);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_net.IsClient)
            return;

        var time = _timing.CurTime;
        var toRemove = new List<EntityUid>();

        foreach (var uid in _activeFabricators)
        {
            if (TerminatingOrDeleted(uid) || !TryComp(uid, out VehicleFabricatorComponent? comp))
            {
                toRemove.Add(uid);
                continue;
            }

            if (comp.Printing == null || time < comp.PrintAt)
                continue;

            var printedId = comp.Printing.Value;
            SpawnAtPosition(printedId, uid.ToCoordinates());

            _audio.PlayPvs(comp.PrintSound, uid);

            if (_printableInfoCache.TryGetValue(printedId, out var info))
            {
                var limitKey = VehicleFabricatorUtils.GetLimitKey(info.Category, info.Vehicle);
                comp.PrintedModules[limitKey] = comp.PrintedModules.GetValueOrDefault(limitKey, 0) + 1;
            }

            comp.Printing = null;
            Dirty(uid, comp);

            _appearance.SetData(uid, VehicleFabricatorVisuals.State, VehicleFabricatorState.Idle);
            SendUIState(uid, comp);
            toRemove.Add(uid);
        }

        foreach (var uid in toRemove)
        {
            _activeFabricators.Remove(uid);
        }
    }
}

[Serializable, NetSerializable]
public sealed partial class VehicleFabricatorRecycleDoafterEvent : SimpleDoAfterEvent;
