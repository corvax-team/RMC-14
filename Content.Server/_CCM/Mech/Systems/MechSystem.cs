using Content.Server.Construction.Components;
using Content.Server.Construction;
using Content.Server._CCM.Mech.Components;
using Content.Server._CCM.Mech.Events;
using Content.Server._CCM.Mech.Equipment.Components;
using Content.Shared.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Alert;
using Content.Shared.Actions.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared._CCM.Mech.Components;
using Content.Shared._CCM.Mech.EntitySystems;
using Content.Shared._CCM.Mech;
using Content.Shared.Popups;
using System.Linq;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Components;
using Content.Shared.Tools;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Content.Shared.Verbs;
using Content.Shared.Wires;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.Body.Events;
using Robust.Shared.Audio.Systems;
using Content.Shared._CCM.Vehicle;
using Content.Shared.PowerCell;
using Content.Server.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Shared.Materials;
using Content.Server.Materials;
using Content.Shared.Containers.ItemSlots;
using System.Numerics;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;

namespace Content.Server._CCM.Mech.Systems;

/// <inheritdoc/>
public sealed partial class MechSystem : SharedMechSystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly MaterialStorageSystem _material = default!;
    [Dependency] private readonly ConstructionSystem _construction = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    private static readonly ProtoId<ToolQualityPrototype> PryingQuality = "Prying";

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CCMMechComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CCMMechComponent, MechEntryEvent>(OnMechEntry);
        SubscribeLocalEvent<CCMMechComponent, MechExitEvent>(OnMechExit);
        SubscribeLocalEvent<CCMMechComponent, MechOpenUiEvent>(OnOpenUi);

        SubscribeLocalEvent<CCMMechComponent, EntInsertedIntoContainerMessage>(OnContainerChanged);
        SubscribeLocalEvent<CCMMechComponent, EntRemovedFromContainerMessage>(OnContainerChanged);
        SubscribeLocalEvent<CCMMechComponent, RemoveBatteryEvent>(OnRemoveBattery);
        SubscribeLocalEvent<CCMMechComponent, InteractUsingEvent>(OnInteractUsing);

        SubscribeLocalEvent<CCMMechComponent, UpdateCanMoveEvent>(OnMechCanMoveEvent);
        SubscribeLocalEvent<CCMMechComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<CCMMechComponent, RepairMechEvent>(OnRepairMechEvent);
        SubscribeLocalEvent<CCMMechComponent, PowerCellChangedEvent>(OnBatteryChanged);

        SubscribeLocalEvent<CCMMechComponent, MechBrokenSoundEvent>(OnMechBrokenSound);
        SubscribeLocalEvent<CCMMechComponent, MechEntrySuccessSoundEvent>(OnMechEntrySuccessSound);

        SubscribeAllEvent<RequestMechEquipmentSelectEvent>(OnEquipmentSelectRequest);
        SubscribeLocalEvent<CCMMechPilotComponent, ToolUserAttemptUseEvent>(OnToolUseAttempt);
        SubscribeLocalEvent<CCMMechComponent, BeingGibbedEvent>(OnBeingGibbed);
    }

    private void OnMapInit(EntityUid uid, CCMMechComponent component, MapInitEvent args)
    {
        var xform = Transform(uid);

        foreach (var equipment in component.StartingEquipment)
        {
            var ent = Spawn(equipment, xform.Coordinates);
            InsertEquipment(uid, ent, component);
        }

        foreach (var module in component.StartingModules)
        {
            var ent = Spawn(module, xform.Coordinates);
            InsertEquipment(uid, ent, component);
        }

        component.Integrity = component.MaxIntegrity;

        SetIntegrity(uid, component.MaxIntegrity, component);
        _actionBlocker.UpdateCanMove(uid);

        UpdateUserInterface(uid, component);
        UpdateHealthAlert((uid, component));
    }

    private void OnMechEntry(EntityUid uid, CCMMechComponent component, MechEntryEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!Vehicle.CanOperate(uid, args.User))
        {
            _popup.PopupEntity(Loc.GetString("mech-no-enter-popup", ("item", uid)), args.User);
            return;
        }

        TryInsert(uid, args.Args.User, component);
        args.Handled = true;

        UpdateUserInterface(uid, component);
        UpdateBatteryAlert((uid, component));
        UpdateHealthAlert((uid, component));

        // Ensure pilot has required components
        var pilotActions = EnsureComp<ActionsComponent>(args.User);
        var pilotAlerts = EnsureComp<AlertsComponent>(args.User);

        // Setup actions relay
        var actionsRelay = EnsureComp<CCMActionsDisplayRelayComponent>(args.User);
        actionsRelay.Source = uid;
        actionsRelay.InteractAsSource = true;

        // Setup alerts relay
        var alertsRelay = EnsureComp<CCMAlertsDisplayRelayComponent>(args.User);
        alertsRelay.Source = uid;
        alertsRelay.InteractAsSource = true;

        // Notify client of changes
        Dirty(args.User, pilotActions);
        Dirty(args.User, pilotAlerts);
        Dirty(args.User, actionsRelay);
        Dirty(args.User, alertsRelay);
    }

    private void OnMechExit(EntityUid uid, CCMMechComponent component, MechExitEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var pilot = Vehicle.GetOperatorOrNull(uid);

        TryEject(uid, component);

        args.Handled = true;

        UpdateUserInterface(uid, component);

        if (pilot.HasValue)
            _actionBlocker.UpdateCanMove(pilot.Value);
    }

    private void OnOpenUi(EntityUid uid, CCMMechComponent component, MechOpenUiEvent args)
    {
        // UI can always be opened, access control is handled in the UI itself
        args.Handled = true;
        ToggleMechUi(uid, component);
    }

    private void OnContainerChanged(EntityUid uid, CCMMechComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container == component.BatterySlot)
        {
            Dirty(uid, component);
            _actionBlocker.UpdateCanMove(uid);

            UpdateUserInterface(uid, component);
            UpdateBatteryAlert((uid, component));
        }
        else if (args.Container == component.PilotSlot)
        {
            UpdateBatteryAlert((uid, component));
            UpdateHealthAlert((uid, component));

            if (TryComp<ItemSlotsComponent>(uid, out var slots))
                _itemSlots.SetLock(uid, component.BatterySlotId, true, slots);
        }
    }

    private void OnContainerChanged(EntityUid uid, CCMMechComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container == component.BatterySlot)
        {
            Dirty(uid, component);
            _actionBlocker.UpdateCanMove(uid);

            UpdateUserInterface(uid, component);
        }
        else if (args.Container == component.PilotSlot)
        {
            if (TryComp<ItemSlotsComponent>(uid, out var slots))
                _itemSlots.SetLock(uid, component.BatterySlotId, false, slots);
        }
        else if (args.Container == component.EquipmentContainer)
        {
            if (!TryComp<CCMMechEquipmentComponent>(args.Entity, out var eq))
                return;

            if (eq.EquipmentOwner == uid)
                eq.EquipmentOwner = null;
        }
    }

    private void OnRemoveBattery(EntityUid uid, CCMMechComponent component, RemoveBatteryEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        RemoveBattery(uid, component);
        _actionBlocker.UpdateCanMove(uid);

        args.Handled = true;

        UpdateUserInterface(uid, component);
    }

    private void OnInteractUsing(EntityUid uid, CCMMechComponent component, InteractUsingEvent args)
    {
        // Allow prying removal when a battery is present
        if (_toolSystem.HasQuality(args.Used, PryingQuality) && component.BatterySlot.ContainedEntity != null)
        {
            if (Vehicle.HasOperator(uid))
            {
                _popup.PopupEntity(Loc.GetString("mech-cannot-modify-closed-popup"), args.User);
                return;
            }

            var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, component.BatteryRemovalDelay,
                new RemoveBatteryEvent(), uid, target: uid, used: args.Target)
            {
                BreakOnMove = true
            };

            _doAfter.TryStartDoAfter(doAfterEventArgs);
            return;
        }

        // Try forwarding material sheets into generator module storage when hatch is open
        if (!Vehicle.HasOperator(uid) && TryComp<MaterialComponent>(args.Used, out var materialComp))
        {
            foreach (var mod in component.ModuleContainer.ContainedEntities)
            {
                if (TryComp<MaterialStorageComponent>(mod, out var storage))
                {
                    if (_material.TryInsertMaterialEntity(args.User, args.Used, mod, storage))
                    {
                        args.Handled = true;
                        return;
                    }
                }
            }
        }
    }

    private void OnMechCanMoveEvent(EntityUid uid, CCMMechComponent component, UpdateCanMoveEvent args)
    {
        // Block movement if mech is in broken state or has no energy/integrity
        var hasCharge = _powerCell.TryGetBatteryFromSlot(uid, out var battery) && battery.CurrentCharge > 0;
        if (component.Broken || component.Integrity <= 0 || !hasCharge)
        {
            args.Cancel();
            return;
        }

        // Block movement if the pilot has no hands
        if (Vehicle.TryGetOperator(uid, out var operatorEnt))
        {
            if (!HasComp<HandsComponent>(operatorEnt))
            {
                args.Cancel();
                return;
            }
        }
    }

    private void OnDamageChanged(EntityUid uid, CCMMechComponent component, DamageChangedEvent args)
    {
        var integrity = component.MaxIntegrity - args.Damageable.TotalDamage;
        SetIntegrity(uid, integrity, component);

        // Sync construction graph with mech state
        var cc = EnsureComp<ConstructionComponent>(uid);
        if (component.Broken)
        {
            if (_construction.ChangeGraph(uid, null, "MechRepair", "start", performActions: false, cc))
                _construction.SetPathfindingTarget(uid, "repaired", cc);
        }

        UpdateUserInterface(uid, component);
        UpdateHealthAlert((uid, component));
    }

    private void OnRepairMechEvent(EntityUid uid, CCMMechComponent component, RepairMechEvent args)
    {
        RepairMech(uid, component);

        // Restore prototype-declared disassembly graph after successful repair
        var cc = EnsureComp<ConstructionComponent>(uid);
        _construction.ChangeGraph(uid, null, "MechDisassemble", "start", performActions: false, cc);
    }

    private void OnBatteryChanged(EntityUid uid, CCMMechComponent component, PowerCellChangedEvent args)
    {
        // Battery changed, update UI and alerts
        Dirty(uid, component);
        UpdateUserInterface(uid, component);
        UpdateBatteryAlert((uid, component));
    }

    private void OnMechBrokenSound(EntityUid uid, CCMMechComponent component, MechBrokenSoundEvent args)
    {
        _audio.PlayPvs(args.Sound, uid);
    }

    private void OnMechEntrySuccessSound(EntityUid uid, CCMMechComponent component, MechEntrySuccessSoundEvent args)
    {
        var pilot = Vehicle.GetOperatorOrNull(uid);
        if (!pilot.HasValue)
            return;

        _audio.PlayEntity(args.Sound, Filter.Entities(pilot.Value), uid, false);
    }

    private void OnEquipmentSelectRequest(RequestMechEquipmentSelectEvent args, EntitySessionEventArgs session)
    {
        var user = session.SenderSession.AttachedEntity;
        if (user == null)
            return;
        if (!TryComp<CCMMechPilotComponent>(user.Value, out var pilot))
            return;
        var mech = pilot.Mech;
        if (!TryComp<CCMMechComponent>(mech, out var mechComp))
            return;

        if (args.Equipment == null)
        {
            mechComp.CurrentSelectedEquipment = null;
            _popup.PopupEntity(Loc.GetString("mech-select-none-popup"), mech);
        }
        else
        {
            var equipment = GetEntity(args.Equipment);
            if (Exists(equipment) && mechComp.EquipmentContainer.ContainedEntities.Any(e => e == equipment))
            {
                mechComp.CurrentSelectedEquipment = equipment;
                _popup.PopupEntity(Loc.GetString("mech-select-popup", ("item", equipment)), mech);
            }
        }

        Dirty(mech, mechComp);
        RefreshPilotHandVirtualItems(mech, mechComp);
    }

    private void OnToolUseAttempt(EntityUid uid, CCMMechPilotComponent component, ref ToolUserAttemptUseEvent args)
    {
        if (args.Target == component.Mech)
            args.Cancelled = true;
    }

    private void OnBeingGibbed(EntityUid uid, CCMMechComponent component, ref BeingGibbedEvent args)
    {
        // Eject pilot if present
        if (component.PilotSlot.ContainedEntity != null)
            TryEject(uid, component);

        if (component.PilotSlot.ContainedEntity != null)
            args.GibbedParts.Add(component.PilotSlot.ContainedEntity.Value);

        // TODO: Parts should fall out
        QueueDel(uid);
    }

    private void ToggleMechUi(EntityUid uid, CCMMechComponent? component = null, EntityUid? user = null)
    {
        if (!Resolve(uid, ref component))
            return;

        user ??= Vehicle.GetOperatorOrNull(uid);
        if (user == null)
            return;

        if (!TryComp<ActorComponent>(user, out var actor))
            return;

        // Open UI using UserInterfaceSystem
        _ui.TryToggleUi(uid, CCMMechUiKey.Key, actor.PlayerSession);
    }

    public bool TryChangeEnergy(EntityUid uid, FixedPoint2 delta, CCMMechComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (delta > 0)
            return false;

        var amount = MathF.Abs(delta.Float());
        if (!_powerCell.TryUseCharge(uid, amount))
            return false;

        UpdateUserInterface(uid, component);
        UpdateBatteryAlert((uid, component));

        return true;
    }

    private void UpdateBatteryAlert(Entity<CCMMechComponent> ent)
    {
        if (!_powerCell.TryGetBatteryFromSlot(ent, out var batt))
        {
            _alerts.ClearAlert(ent.Owner, ent.Comp.BatteryAlert);
            _alerts.ShowAlert(ent.Owner, ent.Comp.NoBatteryAlert);
            return;
        }

        var max = MathF.Max(batt.MaxCharge, 0.0001f);
        var chargePercent = (short)MathF.Round(batt.CurrentCharge / max * 10f);

        // we make sure 0 only shows if they have absolutely no battery.
        // also account for floating point imprecision
        if (chargePercent == 0 && batt.CurrentCharge > 0)
            chargePercent = 1;

        _alerts.ClearAlert(ent.Owner, ent.Comp.NoBatteryAlert);
        _alerts.ShowAlert(ent.Owner, ent.Comp.BatteryAlert, chargePercent);
    }

    private void UpdateHealthAlert(Entity<CCMMechComponent> ent)
    {
        if (ent.Comp.Broken)
        {
            // Mech is broken
            _alerts.ClearAlert(ent.Owner, ent.Comp.HealthAlert);
            _alerts.ShowAlert(ent.Owner, ent.Comp.BrokenAlert);
        }
        else
        {
            // Mech is healthy, show health percentage
            _alerts.ClearAlert(ent.Owner, ent.Comp.BrokenAlert);

            var integrity = ent.Comp.Integrity.Float();
            var maxIntegrity = ent.Comp.MaxIntegrity.Float();
            var healthPercent = (short)MathF.Round((1f - integrity / maxIntegrity) * 4f);
            _alerts.ShowAlert(ent.Owner, ent.Comp.HealthAlert, healthPercent);
        }
    }

    public void RemoveBattery(EntityUid uid, CCMMechComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        _container.EmptyContainer(component.BatterySlot);

        _actionBlocker.UpdateCanMove(uid);
        Dirty(uid, component);
        UpdateUserInterface(uid, component);
    }

    public override bool CanInsert(EntityUid uid, EntityUid toInsert, 
        CCMMechComponent? component = null,
        CCMMechEquipmentComponent? equipmentComponent = null, 
        CCMMechModuleComponent? moduleComponent = null)
    {
        if (!base.CanInsert(uid, toInsert, component, equipmentComponent, moduleComponent))
            return false;

        if (!Resolve(uid, ref component))
            return false;

        return base.CanInsert(uid, toInsert, component) && _actionBlocker.CanMove(toInsert);
    }
}
