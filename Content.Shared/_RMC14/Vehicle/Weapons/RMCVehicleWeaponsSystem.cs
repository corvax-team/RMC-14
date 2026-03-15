using System.Collections.Generic;
using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Movement.Systems;
using Content.Shared._RMC14.Weapons.Ranged;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Network;
using Robust.Shared.Containers;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Vehicle.Components;
using Robust.Shared.Timing;

namespace Content.Shared._RMC14.Vehicle;

public sealed class RMCVehicleWeaponsSystem : EntitySystem
{
    private const string HardpointSelectActionId = "ActionRMCVehicleSelectHardpoint";

    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SkillsSystem _skills = default!;
    [Dependency] private readonly RMCVehicleSystem _vehicleSystem = default!;
    [Dependency] private readonly VehicleTurretSystem _turretSystem = default!;
    [Dependency] private readonly RMCVehicleViewToggleSystem _viewToggle = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedContentEyeSystem _eyeSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VehicleWeaponsSeatComponent, StrapAttemptEvent>(OnWeaponSeatStrapAttempt);
        SubscribeLocalEvent<VehicleWeaponsSeatComponent, StrappedEvent>(OnWeaponSeatStrapped);
        SubscribeLocalEvent<VehicleWeaponsSeatComponent, UnstrappedEvent>(OnWeaponSeatUnstrapped);

        SubscribeLocalEvent<VehicleWeaponsSeatComponent, BoundUIOpenedEvent>(OnWeaponsUiOpened);
        SubscribeLocalEvent<VehicleWeaponsSeatComponent, BoundUIClosedEvent>(OnWeaponsUiClosed);
        SubscribeLocalEvent<VehicleWeaponsSeatComponent, RMCVehicleWeaponsSelectMessage>(OnWeaponsSelect);
        SubscribeLocalEvent<VehicleWeaponsSeatComponent, RMCVehicleWeaponsStabilizationMessage>(OnWeaponsStabilization);
        SubscribeLocalEvent<VehicleWeaponsSeatComponent, RMCVehicleWeaponsAutoModeMessage>(OnWeaponsAutoMode);
        SubscribeLocalEvent<VehicleWeaponsOperatorComponent, ComponentShutdown>(OnOperatorShutdown);
        SubscribeLocalEvent<VehicleWeaponsOperatorComponent, ShotAttemptedEvent>(OnOperatorShotAttempted);
        SubscribeLocalEvent<VehicleWeaponsOperatorComponent, RMCVehicleHardpointSelectActionEvent>(OnHardpointActionSelect);
        SubscribeLocalEvent<VehicleWeaponsOperatorComponent, RMCVehicleViewToggledEvent>(OnViewToggled);

        SubscribeLocalEvent<RMCHardpointSlotsChangedEvent>(OnHardpointSlotsChanged);

        SubscribeLocalEvent<VehicleTurretComponent, GunShotEvent>(OnTurretGunShot);
    }

    private void OnWeaponSeatStrapAttempt(Entity<VehicleWeaponsSeatComponent> ent, ref StrapAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (_skills.HasSkills(args.Buckle.Owner, ent.Comp.Skills))
            return;

        if (args.Popup)
            _popup.PopupClient(Loc.GetString("rmc-skills-cant-operate", ("target", ent)), args.Buckle, args.User);
    }

    private void OnWeaponSeatStrapped(Entity<VehicleWeaponsSeatComponent> ent, ref StrappedEvent args)
    {
        if (_net.IsClient)
            return;

        if (!_vehicleSystem.TryGetVehicleFromInterior(ent.Owner, out var vehicle) || vehicle == null)
        {
            return;
        }

        var vehicleUid = vehicle.Value;
        var weapons = EnsureComp<RMCVehicleWeaponsComponent>(vehicleUid);
        ClearOperatorSelections(weapons, args.Buckle.Owner);
        if (ent.Comp.IsPrimaryOperatorSeat)
        {
            weapons.Operator = args.Buckle.Owner;
        }
        RecalculateSelectedWeapon(vehicleUid, weapons);
        Dirty(vehicleUid, weapons);

        var operatorComp = EnsureComp<VehicleWeaponsOperatorComponent>(args.Buckle.Owner);
        operatorComp.Vehicle = vehicle;
        operatorComp.SelectedWeapon = null;
        operatorComp.HardpointActions.Clear();
        Dirty(args.Buckle.Owner, operatorComp);

        RefreshOperatorSelectedWeapons(vehicleUid, weapons);
        RefreshHardpointActions(args.Buckle.Owner, vehicleUid, weapons, operatorComp);

        if (HasComp<VehicleEnterComponent>(vehicleUid))
        {
            _eye.SetTarget(args.Buckle.Owner, vehicleUid);
            _viewToggle.EnableViewToggle(args.Buckle.Owner, vehicleUid, ent.Owner, insideTarget: null, isOutside: true);
        }

        if (ent.Comp.IsPrimaryOperatorSeat)
            UpdateGunnerView(args.Buckle.Owner, vehicleUid);

        _ui.OpenUi(ent.Owner, RMCVehicleWeaponsUiKey.Key, args.Buckle.Owner);
        UpdateWeaponsUiForAllOperators(vehicleUid, weapons);
    }

    private void OnWeaponSeatUnstrapped(Entity<VehicleWeaponsSeatComponent> ent, ref UnstrappedEvent args)
    {
        if (_net.IsClient)
            return;

        if (TryComp(args.Buckle.Owner, out VehicleWeaponsOperatorComponent? operatorComp))
            ClearHardpointActions(args.Buckle.Owner, operatorComp);

        RemCompDeferred<VehicleWeaponsOperatorComponent>(args.Buckle.Owner);
        _ui.CloseUi(ent.Owner, RMCVehicleWeaponsUiKey.Key, args.Buckle.Owner);
        if (ent.Comp.IsPrimaryOperatorSeat)
            UpdateGunnerView(args.Buckle.Owner, ent.Owner, removeOnly: true);

        _viewToggle.DisableViewToggle(args.Buckle.Owner, ent.Owner);

        if (!_vehicleSystem.TryGetVehicleFromInterior(ent.Owner, out var vehicle) || vehicle == null)
            return;

        var vehicleUid = vehicle.Value;
        if (TryComp(vehicleUid, out RMCVehicleWeaponsComponent? weapons) &&
            ent.Comp.IsPrimaryOperatorSeat &&
            weapons.Operator == args.Buckle.Owner)
        {
            weapons.Operator = null;
            ClearOperatorSelections(weapons, args.Buckle.Owner);
            RecalculateSelectedWeapon(vehicleUid, weapons);
            Dirty(vehicleUid, weapons);
        }
        else if (TryComp(vehicleUid, out RMCVehicleWeaponsComponent? otherWeapons))
        {
            ClearOperatorSelections(otherWeapons, args.Buckle.Owner);
            RecalculateSelectedWeapon(vehicleUid, otherWeapons);
            Dirty(vehicleUid, otherWeapons);
        }

        if (TryComp(vehicleUid, out RMCVehicleWeaponsComponent? selectionWeapons))
            RefreshOperatorSelectedWeapons(vehicleUid, selectionWeapons);

        if (TryComp(vehicleUid, out RMCVehicleWeaponsComponent? refreshedWeapons))
            UpdateWeaponsUiForAllOperators(vehicleUid, refreshedWeapons);

        if (TryComp(args.Buckle.Owner, out EyeComponent? eye) && eye.Target == vehicleUid)
            _eye.SetTarget(args.Buckle.Owner, null, eye);
    }

    private void OnWeaponsUiOpened(Entity<VehicleWeaponsSeatComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (!Equals(args.UiKey, RMCVehicleWeaponsUiKey.Key))
            return;


        if (!_vehicleSystem.TryGetVehicleFromInterior(ent.Owner, out var vehicle) || vehicle == null)
            return;

        var vehicleUid = vehicle.Value;
        if (!TryComp(vehicleUid, out RMCVehicleWeaponsComponent? weapons))
            return;

        var viewer = args.Actor;
        if (viewer == default || !Exists(viewer))
            return;

        UpdateWeaponsUi(ent.Owner, vehicleUid, weapons, operatorUid: viewer);
    }

    private void OnWeaponsUiClosed(Entity<VehicleWeaponsSeatComponent> ent, ref BoundUIClosedEvent args)
    {
        if (!Equals(args.UiKey, RMCVehicleWeaponsUiKey.Key))
            return;
    }

    private void OnWeaponsSelect(Entity<VehicleWeaponsSeatComponent> ent, ref RMCVehicleWeaponsSelectMessage args)
    {
        if (!Equals(args.UiKey, RMCVehicleWeaponsUiKey.Key))
            return;

        if (args.Actor == default || !Exists(args.Actor))
            return;

        if (!CanUseHardpointActions(args.Actor, forUi: true))
            return;

        TrySelectHardpoint(ent.Owner, args.Actor, args.SlotId, fromUi: true);
    }

    private void OnOperatorShutdown(Entity<VehicleWeaponsOperatorComponent> ent, ref ComponentShutdown args)
    {
        if (_net.IsClient)
            return;

        ClearHardpointActions(ent.Owner, ent.Comp);
    }

    private void OnOperatorShotAttempted(Entity<VehicleWeaponsOperatorComponent> ent, ref ShotAttemptedEvent args)
    {
        if (_net.IsClient)
            return;

        if (args.User != ent.Owner)
            return;

        if (ent.Comp.Vehicle is not { } vehicle ||
            !TryComp(vehicle, out RMCVehicleWeaponsComponent? weapons) ||
            !TryComp(vehicle, out ItemSlotsComponent? itemSlots) ||
            !CanUseHardpointActions(ent.Owner) ||
            !weapons.OperatorSelections.TryGetValue(ent.Owner, out var selectedSlot) ||
            !TryGetSlotItem(vehicle, selectedSlot, itemSlots, out var selectedWeapon) ||
            selectedWeapon != args.Used.Owner)
        {
            return;
        }

        var remaining = args.Used.Comp.NextFire - _timing.CurTime;
        if (remaining <= TimeSpan.Zero)
            return;

        if (_timing.CurTime < ent.Comp.NextCooldownFeedbackAt)
            return;

        ent.Comp.NextCooldownFeedbackAt = _timing.CurTime + TimeSpan.FromSeconds(0.25);

        if (!TryComp(ent.Owner, out BuckleComponent? buckle) ||
            buckle.BuckledTo is not { } seat ||
            !HasComp<VehicleWeaponsSeatComponent>(seat))
        {
            return;
        }

        _ui.ServerSendUiMessage(
            seat,
            RMCVehicleWeaponsUiKey.Key,
            new RMCVehicleWeaponsCooldownFeedbackMessage((float) remaining.TotalSeconds),
            ent.Owner);

        _audio.PlayPredicted(args.Used.Comp.SoundEmpty, args.Used.Owner, ent.Owner);
    }

    private void OnHardpointActionSelect(Entity<VehicleWeaponsOperatorComponent> ent, ref RMCVehicleHardpointSelectActionEvent args)
    {
        if (_net.IsClient || args.Handled)
            return;

        if (args.Performer == default || !Exists(args.Performer) || args.Performer != ent.Owner)
            return;

        if (!CanUseHardpointActions(args.Performer))
            return;

        if (!TryComp(args.Performer, out BuckleComponent? buckle) ||
            buckle.BuckledTo is not { } seat ||
            !HasComp<VehicleWeaponsSeatComponent>(seat))
        {
            return;
        }

        if (!TryComp(args.Action, out RMCVehicleHardpointActionComponent? hardpointAction))
            return;

        if (TrySelectHardpoint(seat, args.Performer, hardpointAction.SlotId, fromUi: false))
            args.Handled = true;
    }

    private bool TrySelectHardpoint(EntityUid seat, EntityUid actor, string? slotId, bool fromUi)
    {
        if (_net.IsClient)
            return false;

        if (!_vehicleSystem.TryGetVehicleFromInterior(seat, out var vehicle) || vehicle == null)
            return false;

        var vehicleUid = vehicle.Value;
        if (!TryComp(vehicleUid, out RMCVehicleWeaponsComponent? weapons))
            return false;

        if (!TryComp(actor, out BuckleComponent? buckle) ||
            buckle.BuckledTo != seat ||
            !TryComp(seat, out VehicleWeaponsSeatComponent? seatComp))
        {
            return false;
        }

        if (fromUi && !seatComp.AllowUiSelection)
            return false;

        if (!fromUi && !seatComp.AllowHotbarSelection)
            return false;

        if (TryComp(actor, out VehiclePortGunOperatorComponent? portGunOperator) &&
            portGunOperator.Gun != null)
        {
            _popup.PopupClient(Loc.GetString("rmc-vehicle-portgun-active"), seat, actor);
            return true;
        }

        if (!TryComp(vehicleUid, out RMCHardpointSlotsComponent? hardpoints) ||
            !TryComp(vehicleUid, out ItemSlotsComponent? itemSlots))
        {
            return false;
        }

        if (!TryComp(actor, out VehicleWeaponsOperatorComponent? operatorComp))
            return false;

        if (string.IsNullOrWhiteSpace(slotId))
        {
            ClearOperatorSelections(weapons, actor);
            RecalculateSelectedWeapon(vehicleUid, weapons, itemSlots);
            RefreshOperatorSelectedWeapons(vehicleUid, weapons, itemSlots);
            Dirty(vehicleUid, weapons);
            UpdateHardpointActionStates(actor, weapons, operatorComp);
            UpdateWeaponsUiForAllOperators(vehicleUid, weapons, hardpoints, itemSlots);
            return true;
        }

        if (!TryGetSlotHardpointType(vehicleUid, slotId, hardpoints, itemSlots, out var hardpointType) ||
            !IsHardpointTypeAllowed(seatComp, hardpointType))
        {
            return false;
        }

        if (!TryGetSlotItem(vehicleUid, slotId, itemSlots, out var item) ||
            !HasComp<VehicleTurretComponent>(item) ||
            !HasComp<GunComponent>(item))
        {
            ClearOperatorSelections(weapons, actor);
            RecalculateSelectedWeapon(vehicleUid, weapons, itemSlots);
            RefreshOperatorSelectedWeapons(vehicleUid, weapons, itemSlots);
            Dirty(vehicleUid, weapons);
            UpdateHardpointActionStates(actor, weapons, operatorComp);
            UpdateWeaponsUiForAllOperators(vehicleUid, weapons, hardpoints, itemSlots);
            return true;
        }

        var sharedSelection = IsSharedHardpointType(hardpointType);
        if (!sharedSelection &&
            weapons.HardpointOperators.TryGetValue(slotId, out var currentOperator) &&
            currentOperator != actor)
        {
            _popup.PopupClient(Loc.GetString("rmc-vehicle-weapons-ui-hardpoint-in-use", ("operator", currentOperator)), seat, actor);
            UpdateWeaponsUiForAllOperators(vehicleUid, weapons, hardpoints, itemSlots);
            return true;
        }

        var playSelectSound = !string.IsNullOrWhiteSpace(slotId) &&
                              (!weapons.OperatorSelections.TryGetValue(actor, out var priorSlot) ||
                               !string.Equals(priorSlot, slotId, StringComparison.OrdinalIgnoreCase));

        if (weapons.OperatorSelections.TryGetValue(actor, out var existingSlot) &&
            string.Equals(existingSlot, slotId, StringComparison.OrdinalIgnoreCase))
        {
            weapons.OperatorSelections.Remove(actor);
            if (!sharedSelection &&
                weapons.HardpointOperators.TryGetValue(slotId, out var existingOperator) &&
                existingOperator == actor)
            {
                weapons.HardpointOperators.Remove(slotId);
            }
        }
        else
        {
            if (existingSlot != null &&
                weapons.HardpointOperators.TryGetValue(existingSlot, out var existingOperator) &&
                existingOperator == actor)
            {
                weapons.HardpointOperators.Remove(existingSlot);
            }

            weapons.OperatorSelections[actor] = slotId;
            if (!sharedSelection)
                weapons.HardpointOperators[slotId] = actor;

            if (playSelectSound &&
                TryComp(item, out GunSpinupComponent? spinup) &&
                spinup.SelectSound != null)
            {
                _audio.PlayPredicted(spinup.SelectSound, item, actor);
            }
        }

        RecalculateSelectedWeapon(vehicleUid, weapons, itemSlots);
        RefreshOperatorSelectedWeapons(vehicleUid, weapons, itemSlots);
        Dirty(vehicleUid, weapons);
        UpdateHardpointActionStates(actor, weapons, operatorComp);
        UpdateWeaponsUiForAllOperators(vehicleUid, weapons, hardpoints, itemSlots);
        return true;
    }

    private void OnWeaponsStabilization(Entity<VehicleWeaponsSeatComponent> ent, ref RMCVehicleWeaponsStabilizationMessage args)
    {
        if (!Equals(args.UiKey, RMCVehicleWeaponsUiKey.Key))
            return;

        if (args.Actor == default || !Exists(args.Actor))
            return;

        if (!_vehicleSystem.TryGetVehicleFromInterior(ent.Owner, out var vehicle) || vehicle == null)
            return;

        var vehicleUid = vehicle.Value;
        if (!TryComp(vehicleUid, out RMCVehicleWeaponsComponent? weapons) || weapons.Operator != args.Actor)
            return;

        if (!TryComp(args.Actor, out BuckleComponent? buckle) || buckle.BuckledTo != ent.Owner)
            return;

        RMCHardpointSlotsComponent? hardpoints = null;
        ItemSlotsComponent? itemSlots = null;
        if (!Resolve(vehicleUid, ref hardpoints, logMissing: false) ||
            !Resolve(vehicleUid, ref itemSlots, logMissing: false))
        {
            return;
        }

        if (!weapons.OperatorSelections.TryGetValue(args.Actor, out var operatorSlot) ||
            string.IsNullOrWhiteSpace(operatorSlot))
        {
            return;
        }

        if (itemSlots == null ||
            !TryGetSlotItem(vehicleUid, operatorSlot, itemSlots, out var item))
            return;

        if (!TryComp(item, out VehicleTurretComponent? turret) ||
            !_turretSystem.TryResolveRotationTarget(item, out var targetUid, out var targetTurret))
            return;

        if (!targetTurret.RotateToCursor)
            return;

        targetTurret.StabilizedRotation = args.Enabled;
        var vehicleRot = _transform.GetWorldRotation(vehicleUid);
        var currentWorld = (targetTurret.WorldRotation + vehicleRot).Reduced();
        if (args.Enabled)
            targetTurret.TargetRotation = currentWorld;
        else
            targetTurret.TargetRotation = targetTurret.WorldRotation;
        Dirty(targetUid, targetTurret);

        UpdateWeaponsUiForAllOperators(vehicleUid, weapons, hardpoints, itemSlots);
    }

    private void OnWeaponsAutoMode(Entity<VehicleWeaponsSeatComponent> ent, ref RMCVehicleWeaponsAutoModeMessage args)
    {
        if (!Equals(args.UiKey, RMCVehicleWeaponsUiKey.Key))
            return;

        if (args.Actor == default || !Exists(args.Actor))
            return;

        if (!_vehicleSystem.TryGetVehicleFromInterior(ent.Owner, out var vehicle) || vehicle == null)
            return;

        var vehicleUid = vehicle.Value;
        if (!TryComp(vehicleUid, out RMCVehicleWeaponsComponent? weapons) || weapons.Operator != args.Actor)
            return;

        if (!TryComp(args.Actor, out BuckleComponent? buckle) || buckle.BuckledTo != ent.Owner)
            return;

        if (!TryComp(vehicleUid, out RMCVehicleDeployableComponent? deployable))
            return;

        deployable.AutoTurretEnabled = args.Enabled;
        Dirty(vehicleUid, deployable);

        RMCHardpointSlotsComponent? hardpoints = null;
        ItemSlotsComponent? itemSlots = null;
        UpdateWeaponsUiForAllOperators(vehicleUid, weapons, hardpoints, itemSlots);
    }

    private void OnHardpointSlotsChanged(RMCHardpointSlotsChangedEvent args)
    {
        if (_net.IsClient)
            return;

        if (!TryComp(args.Vehicle, out RMCVehicleWeaponsComponent? weapons))
            return;

        RMCHardpointSlotsComponent? hardpoints = null;
        ItemSlotsComponent? itemSlots = null;

        if (weapons.SelectedWeapon is { } selected &&
            Resolve(args.Vehicle, ref hardpoints, logMissing: false) &&
            Resolve(args.Vehicle, ref itemSlots, logMissing: false) &&
            !IsSelectedWeaponInstalled(args.Vehicle, selected, hardpoints, itemSlots))
        {
            weapons.SelectedWeapon = null;
            Dirty(args.Vehicle, weapons);
        }

        PruneHardpointOperators(args.Vehicle, weapons, hardpoints, itemSlots);
        RecalculateSelectedWeapon(args.Vehicle, weapons, itemSlots);
        RefreshOperatorSelectedWeapons(args.Vehicle, weapons, itemSlots);
        Dirty(args.Vehicle, weapons);

        UpdateWeaponsUiForAllOperators(args.Vehicle, weapons, hardpoints, itemSlots, refreshActions: true);
    }

    private void OnViewToggled(Entity<VehicleWeaponsOperatorComponent> ent, ref RMCVehicleViewToggledEvent args)
    {
        if (_net.IsClient)
            return;

        if (ent.Comp.Vehicle is not { } vehicle ||
            !TryComp(vehicle, out RMCVehicleWeaponsComponent? weapons))
        {
            return;
        }

        RefreshHardpointActions(ent.Owner, vehicle, weapons, ent.Comp);

        if (TryGetUserWeaponsSeat(ent.Owner, out var seat, out _))
            UpdateWeaponsUi(seat, vehicle, weapons, operatorUid: ent.Owner);
    }

    private void UpdateGunnerView(EntityUid user, EntityUid vehicle, bool removeOnly = false)
    {
        if (!removeOnly && TryComp(vehicle, out RMCVehicleGunnerViewComponent? gunnerView) && gunnerView.PvsScale > 0f)
        {
            var view = EnsureComp<RMCVehicleGunnerViewUserComponent>(user);
            view.PvsScale = gunnerView.PvsScale;
            view.CursorMaxOffset = gunnerView.CursorMaxOffset;
            view.CursorOffsetSpeed = gunnerView.CursorOffsetSpeed;
            view.CursorPvsIncrease = gunnerView.CursorPvsIncrease;
            Dirty(user, view);
            _eyeSystem.UpdatePvsScale(user);
            return;
        }

        if (RemCompDeferred<RMCVehicleGunnerViewUserComponent>(user))
            _eyeSystem.UpdatePvsScale(user);
    }

    private bool IsSelectedWeaponInstalled(EntityUid vehicle, EntityUid selected, RMCHardpointSlotsComponent hardpoints, ItemSlotsComponent itemSlots)
    {
        foreach (var slot in hardpoints.Slots)
        {
            if (string.IsNullOrWhiteSpace(slot.Id))
                continue;

            if (!_itemSlots.TryGetSlot(vehicle, slot.Id, out var slotData, itemSlots))
                continue;

            if (slotData.HasItem && slotData.Item == selected)
                return true;

            if (!slotData.HasItem || slotData.Item is not { } installed)
                continue;

            if (!TryComp(installed, out RMCHardpointSlotsComponent? turretSlots) ||
                !TryComp(installed, out ItemSlotsComponent? turretItemSlots))
            {
                continue;
            }

            foreach (var turretSlot in turretSlots.Slots)
            {
                if (string.IsNullOrWhiteSpace(turretSlot.Id))
                    continue;

                if (_itemSlots.TryGetSlot(installed, turretSlot.Id, out var turretItemSlot, turretItemSlots) &&
                    turretItemSlot.HasItem &&
                    turretItemSlot.Item == selected)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnTurretGunShot(Entity<VehicleTurretComponent> ent, ref GunShotEvent args)
    {
        if (_net.IsClient)
            return;

        if (!TryGetContainingVehicle(ent.Owner, out var vehicle))
            return;

        if (!TryComp(vehicle, out RMCVehicleWeaponsComponent? weapons))
            return;

        UpdateWeaponsUiForAllOperators(vehicle, weapons);
    }

    private bool TryGetContainingVehicle(EntityUid owner, out EntityUid vehicle)
    {
        vehicle = default;
        var current = owner;

        while (_container.TryGetContainingContainer(current, out var container))
        {
            var containerOwner = container.Owner;
            if (HasComp<VehicleComponent>(containerOwner) || HasComp<RMCVehicleWeaponsComponent>(containerOwner))
            {
                vehicle = containerOwner;
                return true;
            }

            current = containerOwner;
        }

        return false;
    }

    private void UpdateWeaponsUi(
        EntityUid seat,
        EntityUid vehicle,
        RMCVehicleWeaponsComponent? weapons = null,
        RMCHardpointSlotsComponent? hardpoints = null,
        ItemSlotsComponent? itemSlots = null,
        EntityUid? operatorUid = null)
    {
        if (_net.IsClient)
            return;

        if (!Resolve(vehicle, ref weapons, logMissing: false))
            return;

        if (!Resolve(vehicle, ref hardpoints, logMissing: false))
            return;

        if (!Resolve(vehicle, ref itemSlots, logMissing: false))
            return;

        if (operatorUid == null)
            operatorUid = weapons.Operator;

        VehicleWeaponsSeatComponent? operatorSeatComp = null;
        if (operatorUid != null)
            TryGetUserWeaponsSeat(operatorUid.Value, out _, out operatorSeatComp);

        string? operatorSlot = null;
        if (operatorUid != null &&
            weapons.OperatorSelections.TryGetValue(operatorUid.Value, out var operatorSelection))
        {
            operatorSlot = operatorSelection;
        }

        if (operatorSlot == null &&
            operatorUid != null &&
            operatorSeatComp != null &&
            !operatorSeatComp.AllowUiSelection &&
            weapons.Operator is { } primaryOperator &&
            weapons.OperatorSelections.TryGetValue(primaryOperator, out var primarySelection))
        {
            operatorSlot = primarySelection;
        }

        var entries = new List<RMCVehicleWeaponsUiEntry>(hardpoints.Slots.Count);
        var canUseHardpointActions = operatorUid == null || CanUseHardpointActions(operatorUid.Value, forUi: true);

        foreach (var slot in hardpoints.Slots)
        {
            if (string.IsNullOrWhiteSpace(slot.Id))
                continue;

            var slotAllowed = operatorSeatComp == null || IsHardpointTypeAllowed(operatorSeatComp, slot.HardpointType);
            var sharedSelection = IsSharedHardpointType(slot.HardpointType);

            var hasItem = _itemSlots.TryGetSlot(vehicle, slot.Id, out var itemSlot, itemSlots) && itemSlot.HasItem;
            EntityUid? item = hasItem ? itemSlot!.Item : null;
            string? installedName = null;
            NetEntity? installedEntity = null;

            if (item != null)
            {
                installedName = Name(item.Value);
                installedEntity = GetNetEntity(item.Value);
            }

            var operatorName = (string?) null;
            var operatorIsSelf = false;
            var hasOperator = weapons.HardpointOperators.TryGetValue(slot.Id, out var slotOperator);
            if (hasOperator)
            {
                operatorName = Name(slotOperator);
                operatorIsSelf = operatorUid != null && slotOperator == operatorUid.Value;
            }

            var selectable = canUseHardpointActions &&
                             slotAllowed &&
                             item != null &&
                             HasComp<VehicleTurretComponent>(item.Value);
            if (selectable && hasOperator && !operatorIsSelf && !sharedSelection)
                selectable = false;

            var selected = operatorSlot != null && string.Equals(operatorSlot, slot.Id, StringComparison.OrdinalIgnoreCase);

            var ammoCount = 0;
            var ammoCapacity = 0;
            var hasAmmo = false;
            var cooldownRemaining = 0f;
            var cooldownTotal = 0f;
            var isOnCooldown = false;

            if (item != null && TryComp(item.Value, out GunComponent? gun))
            {
                var ammoEv = new GetAmmoCountEvent();
                RaiseLocalEvent(item.Value, ref ammoEv);
                ammoCount = ammoEv.Count;
                ammoCapacity = ammoEv.Capacity;
                hasAmmo = ammoEv.Capacity > 0;

                if (gun.FireRateModified > 0f)
                    cooldownTotal = 1f / gun.FireRateModified;

                var remaining = gun.NextFire - _timing.CurTime;
                if (remaining > TimeSpan.Zero)
                {
                    cooldownRemaining = (float) remaining.TotalSeconds;
                    isOnCooldown = cooldownRemaining > 0.001f;
                }
            }

            var magazineSize = 0;
            var storedMagazines = 0;
            var maxStoredMagazines = 0;
            var hasMagazineData = false;
            var integrity = 0f;
            var maxIntegrity = 0f;
            var hasIntegrity = false;

            if (item != null && TryComp(item.Value, out RMCVehicleHardpointAmmoComponent? hardpointAmmo))
            {
                magazineSize = Math.Max(1, hardpointAmmo.MagazineSize);
                storedMagazines = hardpointAmmo.StoredMagazines;
                maxStoredMagazines = hardpointAmmo.MaxStoredMagazines;
                hasMagazineData = hardpointAmmo.MagazineSize > 0 || hardpointAmmo.MaxStoredMagazines > 0;
            }

            if (item != null && TryComp(item.Value, out RMCHardpointIntegrityComponent? hardpointIntegrity))
            {
                integrity = hardpointIntegrity.Integrity;
                maxIntegrity = hardpointIntegrity.MaxIntegrity;
                hasIntegrity = true;
            }

            entries.Add(new RMCVehicleWeaponsUiEntry(
                slot.Id,
                slot.HardpointType,
                installedName,
                installedEntity,
                hasItem,
                selectable,
                selected,
                ammoCount,
                ammoCapacity,
                hasAmmo,
                magazineSize,
                storedMagazines,
                maxStoredMagazines,
                hasMagazineData,
                operatorName,
                operatorIsSelf,
                integrity,
                maxIntegrity,
                hasIntegrity,
                cooldownRemaining,
                cooldownTotal,
                isOnCooldown));

            if (item != null)
            {
                AppendTurretEntries(entries, item.Value, slot.Id, weapons, operatorUid, operatorSlot, canUseHardpointActions, operatorSeatComp);
            }
        }

        var canToggleStabilization = false;
        var stabilizationEnabled = false;
        var canToggleAuto = false;
        var autoEnabled = false;

        var canControlToggles = operatorUid != null && weapons.Operator == operatorUid;
        if (canControlToggles && operatorSlot != null &&
            TryGetSlotItem(vehicle, operatorSlot, itemSlots, out var selectedItem) &&
            TryComp(selectedItem, out VehicleTurretComponent? selectedTurret) &&
            _turretSystem.TryResolveRotationTarget(selectedItem, out var targetUid, out var targetTurret))
        {
            stabilizationEnabled = targetTurret.StabilizedRotation;
            canToggleStabilization = targetTurret.RotateToCursor;
        }

        if (canControlToggles &&
            TryComp(vehicle, out RMCVehicleDeployableComponent? deployable))
        {
            canToggleAuto = true;
            autoEnabled = deployable.AutoTurretEnabled;
        }

        _ui.SetUiState(seat, RMCVehicleWeaponsUiKey.Key,
            new RMCVehicleWeaponsUiState(
                GetNetEntity(vehicle),
                entries,
                canToggleStabilization,
                stabilizationEnabled,
                canToggleAuto,
                autoEnabled));
    }

    private void AppendTurretEntries(
        List<RMCVehicleWeaponsUiEntry> entries,
        EntityUid turretUid,
        string parentSlotId,
        RMCVehicleWeaponsComponent weapons,
        EntityUid? operatorUid,
        string? operatorSlot,
        bool canUseHardpointActions,
        VehicleWeaponsSeatComponent? operatorSeatComp)
    {
        if (!TryComp(turretUid, out RMCHardpointSlotsComponent? turretSlots) ||
            !TryComp(turretUid, out ItemSlotsComponent? turretItemSlots))
        {
            return;
        }

        foreach (var turretSlot in turretSlots.Slots)
        {
            if (string.IsNullOrWhiteSpace(turretSlot.Id))
                continue;

            var compositeId = RMCVehicleTurretSlotIds.Compose(parentSlotId, turretSlot.Id);
            var slotAllowed = operatorSeatComp == null || IsHardpointTypeAllowed(operatorSeatComp, turretSlot.HardpointType);
            var sharedSelection = IsSharedHardpointType(turretSlot.HardpointType);
            var hasItem = _itemSlots.TryGetSlot(turretUid, turretSlot.Id, out var turretItemSlot, turretItemSlots) &&
                          turretItemSlot.HasItem;
            EntityUid? item = hasItem ? turretItemSlot!.Item : null;
            string? installedName = null;
            NetEntity? installedEntity = null;

            if (item != null)
            {
                installedName = Name(item.Value);
                installedEntity = GetNetEntity(item.Value);
            }

            var operatorName = (string?) null;
            var operatorIsSelf = false;
            var hasOperator = weapons.HardpointOperators.TryGetValue(compositeId, out var slotOperator);
            if (hasOperator)
            {
                operatorName = Name(slotOperator);
                operatorIsSelf = operatorUid != null && slotOperator == operatorUid.Value;
            }

            var selectable = canUseHardpointActions &&
                             slotAllowed &&
                             item != null &&
                             HasComp<VehicleTurretComponent>(item.Value);
            if (selectable && hasOperator && !operatorIsSelf && !sharedSelection)
                selectable = false;

            var selected = operatorSlot != null && string.Equals(operatorSlot, compositeId, StringComparison.OrdinalIgnoreCase);

            var ammoCount = 0;
            var ammoCapacity = 0;
            var hasAmmo = false;
            var cooldownRemaining = 0f;
            var cooldownTotal = 0f;
            var isOnCooldown = false;

            if (item != null && TryComp(item.Value, out GunComponent? gun))
            {
                var ammoEv = new GetAmmoCountEvent();
                RaiseLocalEvent(item.Value, ref ammoEv);
                ammoCount = ammoEv.Count;
                ammoCapacity = ammoEv.Capacity;
                hasAmmo = ammoEv.Capacity > 0;

                if (gun.FireRateModified > 0f)
                    cooldownTotal = 1f / gun.FireRateModified;

                var remaining = gun.NextFire - _timing.CurTime;
                if (remaining > TimeSpan.Zero)
                {
                    cooldownRemaining = (float) remaining.TotalSeconds;
                    isOnCooldown = cooldownRemaining > 0.001f;
                }
            }

            var magazineSize = 0;
            var storedMagazines = 0;
            var maxStoredMagazines = 0;
            var hasMagazineData = false;
            var integrity = 0f;
            var maxIntegrity = 0f;
            var hasIntegrity = false;

            if (item != null && TryComp(item.Value, out RMCVehicleHardpointAmmoComponent? hardpointAmmo))
            {
                magazineSize = Math.Max(1, hardpointAmmo.MagazineSize);
                storedMagazines = hardpointAmmo.StoredMagazines;
                maxStoredMagazines = hardpointAmmo.MaxStoredMagazines;
                hasMagazineData = hardpointAmmo.MagazineSize > 0 || hardpointAmmo.MaxStoredMagazines > 0;
            }

            if (item != null && TryComp(item.Value, out RMCHardpointIntegrityComponent? hardpointIntegrity))
            {
                integrity = hardpointIntegrity.Integrity;
                maxIntegrity = hardpointIntegrity.MaxIntegrity;
                hasIntegrity = true;
            }

            entries.Add(new RMCVehicleWeaponsUiEntry(
                compositeId,
                turretSlot.HardpointType,
                installedName,
                installedEntity,
                hasItem,
                selectable,
                selected,
                ammoCount,
                ammoCapacity,
                hasAmmo,
                magazineSize,
                storedMagazines,
                maxStoredMagazines,
                hasMagazineData,
                operatorName,
                operatorIsSelf,
                integrity,
                maxIntegrity,
                hasIntegrity,
                cooldownRemaining,
                cooldownTotal,
                isOnCooldown));
        }
    }

    private readonly record struct HardpointActionSlot(string SlotId, EntityUid IconEntity, string DisplayName);

    private void RefreshHardpointActions(
        EntityUid user,
        EntityUid vehicle,
        RMCVehicleWeaponsComponent weapons,
        VehicleWeaponsOperatorComponent? operatorComp = null,
        RMCHardpointSlotsComponent? hardpoints = null,
        ItemSlotsComponent? itemSlots = null)
    {
        if (_net.IsClient)
            return;

        if (!Resolve(user, ref operatorComp, logMissing: false))
            return;

        if (!Resolve(vehicle, ref hardpoints, ref itemSlots, logMissing: false))
        {
            ClearHardpointActions(user, operatorComp);
            return;
        }

        var desired = CanUseHardpointActions(user)
            ? GetSelectableHardpointActionSlots(vehicle, user, weapons, hardpoints, itemSlots)
            : new List<HardpointActionSlot>();

        var desiredSlots = new HashSet<string>(desired.Select(slot => slot.SlotId));

        foreach (var pair in operatorComp.HardpointActions.ToArray())
        {
            if (!desiredSlots.Contains(pair.Key) || !Exists(pair.Value))
            {
                RemoveAndDeleteHardpointAction(user, pair.Value);
                operatorComp.HardpointActions.Remove(pair.Key);
            }
        }

        for (var i = 0; i < desired.Count; i++)
        {
            var desiredSlot = desired[i];
            if (operatorComp.HardpointActions.TryGetValue(desiredSlot.SlotId, out var existingAction) &&
                Exists(existingAction) &&
                TryComp(existingAction, out ActionComponent? existingActionComp) &&
                existingActionComp.Container == desiredSlot.IconEntity)
            {
                if (TryComp(existingAction, out RMCVehicleHardpointActionComponent? existingHardpointAction))
                {
                    existingHardpointAction.SlotId = desiredSlot.SlotId;
                    existingHardpointAction.SortOrder = i;
                    Dirty(existingAction, existingHardpointAction);
                }

                _actions.SetTemporary((existingAction, existingActionComp), false);
                _metaData.SetEntityName(existingAction, desiredSlot.DisplayName);

                continue;
            }

            if (operatorComp.HardpointActions.TryGetValue(desiredSlot.SlotId, out var staleAction) &&
                Exists(staleAction))
            {
                RemoveAndDeleteHardpointAction(user, staleAction);
                operatorComp.HardpointActions.Remove(desiredSlot.SlotId);
            }

            EntityUid? action = null;
            if (!_actions.AddAction(user, ref action, HardpointSelectActionId, container: desiredSlot.IconEntity) ||
                action == null)
            {
                continue;
            }

            var hardpointAction = EnsureComp<RMCVehicleHardpointActionComponent>(action.Value);
            hardpointAction.SlotId = desiredSlot.SlotId;
            hardpointAction.SortOrder = i;
            Dirty(action.Value, hardpointAction);
            _actions.SetTemporary((action.Value, Comp<ActionComponent>(action.Value)), false);
            _metaData.SetEntityName(action.Value, desiredSlot.DisplayName);
            operatorComp.HardpointActions[desiredSlot.SlotId] = action.Value;
        }

        UpdateHardpointActionStates(user, weapons, operatorComp);
    }

    private List<HardpointActionSlot> GetSelectableHardpointActionSlots(
        EntityUid vehicle,
        EntityUid user,
        RMCVehicleWeaponsComponent weapons,
        RMCHardpointSlotsComponent hardpoints,
        ItemSlotsComponent itemSlots)
    {
        var slots = new List<HardpointActionSlot>();
        if (!TryGetUserWeaponsSeat(user, out _, out var seatComp))
            return slots;

        foreach (var slot in hardpoints.Slots)
        {
            if (string.IsNullOrWhiteSpace(slot.Id))
                continue;

            if (!IsHardpointTypeAllowed(seatComp, slot.HardpointType))
                continue;

            var sharedSelection = IsSharedHardpointType(slot.HardpointType);

            if (_itemSlots.TryGetSlot(vehicle, slot.Id, out var itemSlot, itemSlots) &&
                itemSlot.HasItem &&
                itemSlot.Item is { } installed &&
                HasComp<VehicleTurretComponent>(installed) &&
                HasComp<GunComponent>(installed) &&
                (sharedSelection ||
                 !weapons.HardpointOperators.TryGetValue(slot.Id, out var slotOperator) ||
                 slotOperator == user))
            {
                slots.Add(new HardpointActionSlot(slot.Id, installed, Name(installed)));
            }

            if (itemSlot?.Item is not { } turret ||
                !TryComp(turret, out RMCHardpointSlotsComponent? turretSlots) ||
                !TryComp(turret, out ItemSlotsComponent? turretItemSlots))
            {
                continue;
            }

            foreach (var turretSlot in turretSlots.Slots)
            {
                if (string.IsNullOrWhiteSpace(turretSlot.Id))
                    continue;

                if (!IsHardpointTypeAllowed(seatComp, turretSlot.HardpointType))
                    continue;

                var compositeId = RMCVehicleTurretSlotIds.Compose(slot.Id, turretSlot.Id);
                var turretSharedSelection = IsSharedHardpointType(turretSlot.HardpointType);
                if (!_itemSlots.TryGetSlot(turret, turretSlot.Id, out var turretItemSlot, turretItemSlots) ||
                    !turretItemSlot.HasItem ||
                    turretItemSlot.Item is not { } turretItem ||
                    !HasComp<VehicleTurretComponent>(turretItem) ||
                    !HasComp<GunComponent>(turretItem))
                {
                    continue;
                }

                if (!turretSharedSelection &&
                    weapons.HardpointOperators.TryGetValue(compositeId, out var turretOperator) &&
                    turretOperator != user)
                {
                    continue;
                }

                slots.Add(new HardpointActionSlot(compositeId, turretItem, Name(turretItem)));
            }
        }

        return slots;
    }

    private void UpdateHardpointActionStates(
        EntityUid user,
        RMCVehicleWeaponsComponent weapons,
        VehicleWeaponsOperatorComponent? operatorComp = null)
    {
        if (_net.IsClient || !Resolve(user, ref operatorComp, logMissing: false))
            return;

        var canUseHardpointActions = CanUseHardpointActions(user);
        var selectedSlot = weapons.OperatorSelections.TryGetValue(user, out var slot)
            ? slot
            : null;

        foreach (var pair in operatorComp.HardpointActions)
        {
            _actions.SetEnabled(pair.Value, canUseHardpointActions);
            _actions.SetToggled(
                pair.Value,
                canUseHardpointActions &&
                selectedSlot != null &&
                string.Equals(pair.Key, selectedSlot, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void ClearHardpointActions(EntityUid user, VehicleWeaponsOperatorComponent? operatorComp = null)
    {
        if (_net.IsClient || !Resolve(user, ref operatorComp, logMissing: false))
            return;

        foreach (var action in operatorComp.HardpointActions.Values.ToArray())
        {
            if (Exists(action))
                RemoveAndDeleteHardpointAction(user, action);
        }

        operatorComp.HardpointActions.Clear();
    }

    private void RemoveAndDeleteHardpointAction(EntityUid user, EntityUid action)
    {
        if (!Exists(action))
            return;

        _actions.RemoveAction(user, action);

        if (Exists(action))
            QueueDel(action);
    }

    private void ClearOperatorSelections(RMCVehicleWeaponsComponent weapons, EntityUid operatorUid)
    {
        if (weapons.OperatorSelections.TryGetValue(operatorUid, out var slotId))
        {
            weapons.OperatorSelections.Remove(operatorUid);
            weapons.HardpointOperators.Remove(slotId);
        }

        foreach (var pair in weapons.HardpointOperators.ToArray())
        {
            if (pair.Value == operatorUid)
                weapons.HardpointOperators.Remove(pair.Key);
        }
    }

    private void PruneHardpointOperators(
        EntityUid vehicle,
        RMCVehicleWeaponsComponent weapons,
        RMCHardpointSlotsComponent? hardpoints,
        ItemSlotsComponent? itemSlots)
    {
        if (!Resolve(vehicle, ref hardpoints, logMissing: false))
            return;

        var validSlots = GetAllSlotIds(vehicle, hardpoints, itemSlots);

        foreach (var entry in weapons.HardpointOperators.ToArray())
        {
            if (!validSlots.Contains(entry.Key))
            {
                weapons.HardpointOperators.Remove(entry.Key);
                continue;
            }

            if (!Exists(entry.Value))
                weapons.HardpointOperators.Remove(entry.Key);
        }

        foreach (var entry in weapons.OperatorSelections.ToArray())
        {
            if (!validSlots.Contains(entry.Value))
                weapons.OperatorSelections.Remove(entry.Key);
        }
    }

    private HashSet<string> GetAllSlotIds(
        EntityUid vehicle,
        RMCHardpointSlotsComponent? hardpoints,
        ItemSlotsComponent? itemSlots)
    {
        var validSlots = new HashSet<string>();

        if (!Resolve(vehicle, ref hardpoints, ref itemSlots, logMissing: false))
            return validSlots;

        foreach (var slot in hardpoints.Slots)
        {
            if (string.IsNullOrWhiteSpace(slot.Id))
                continue;

            validSlots.Add(slot.Id);

            if (!_itemSlots.TryGetSlot(vehicle, slot.Id, out var itemSlot, itemSlots) || !itemSlot.HasItem)
                continue;

            var installed = itemSlot.Item!.Value;
            if (!TryComp(installed, out RMCHardpointSlotsComponent? turretSlots) ||
                !TryComp(installed, out ItemSlotsComponent? turretItemSlots))
            {
                continue;
            }

            foreach (var turretSlot in turretSlots.Slots)
            {
                if (string.IsNullOrWhiteSpace(turretSlot.Id))
                    continue;

                validSlots.Add(RMCVehicleTurretSlotIds.Compose(slot.Id, turretSlot.Id));
            }
        }

        return validSlots;
    }

    private bool TryGetSlotItem(
        EntityUid vehicle,
        string slotId,
        ItemSlotsComponent itemSlots,
        out EntityUid item)
    {
        item = default;

        if (RMCVehicleTurretSlotIds.TryParse(slotId, out var parentSlotId, out var childSlotId))
        {
            if (!_itemSlots.TryGetSlot(vehicle, parentSlotId, out var parentSlot, itemSlots) || !parentSlot.HasItem)
                return false;

            var turretUid = parentSlot.Item!.Value;
            if (!TryComp(turretUid, out ItemSlotsComponent? turretItemSlots))
                return false;

            if (!_itemSlots.TryGetSlot(turretUid, childSlotId, out var turretSlot, turretItemSlots) ||
                !turretSlot.HasItem)
            {
                return false;
            }

            item = turretSlot.Item!.Value;
            return true;
        }

        if (!_itemSlots.TryGetSlot(vehicle, slotId, out var slotData, itemSlots) || !slotData.HasItem)
            return false;

        item = slotData.Item!.Value;
        return true;
    }

    private bool CanUseHardpointActions(EntityUid user, bool forUi = false)
    {
        if (!TryGetUserWeaponsSeat(user, out _, out var seatComp))
            return false;

        if (forUi && !seatComp.AllowUiSelection)
            return false;

        if (!forUi && !seatComp.AllowHotbarSelection)
            return false;

        if (TryComp(user, out RMCVehicleViewToggleComponent? viewToggle) && !viewToggle.IsOutside)
            return false;

        return true;
    }

    private void UpdateWeaponsUiForAllOperators(
        EntityUid vehicle,
        RMCVehicleWeaponsComponent weapons,
        RMCHardpointSlotsComponent? hardpoints = null,
        ItemSlotsComponent? itemSlots = null,
        bool refreshActions = false)
    {
        var query = EntityQueryEnumerator<VehicleWeaponsOperatorComponent>();
        while (query.MoveNext(out var operatorUid, out var operatorComp))
        {
            if (operatorComp.Vehicle != vehicle)
                continue;

            if (!TryGetUserWeaponsSeat(operatorUid, out var seat, out _))
                continue;

            if (refreshActions)
                RefreshHardpointActions(operatorUid, vehicle, weapons, operatorComp, hardpoints, itemSlots);

            UpdateWeaponsUi(seat, vehicle, weapons, hardpoints, itemSlots, operatorUid);
        }
    }

    private bool TryGetUserWeaponsSeat(
        EntityUid user,
        out EntityUid seat,
        out VehicleWeaponsSeatComponent seatComp)
    {
        seat = default;
        seatComp = default!;

        if (!TryComp(user, out BuckleComponent? buckle) ||
            buckle.BuckledTo is not { } buckledSeat ||
            !TryComp(buckledSeat, out VehicleWeaponsSeatComponent? resolvedSeatComp))
        {
            return false;
        }

        seatComp = resolvedSeatComp;
        seat = buckledSeat;
        return true;
    }

    private bool TryGetSlotHardpointType(
        EntityUid vehicle,
        string slotId,
        RMCHardpointSlotsComponent hardpoints,
        ItemSlotsComponent itemSlots,
        out string hardpointType)
    {
        hardpointType = string.Empty;

        if (RMCVehicleTurretSlotIds.TryParse(slotId, out var parentSlotId, out var childSlotId))
        {
            if (!_itemSlots.TryGetSlot(vehicle, parentSlotId, out var parentSlot, itemSlots) ||
                !parentSlot.HasItem)
            {
                return false;
            }

            var turretUid = parentSlot.Item!.Value;
            if (!TryComp(turretUid, out RMCHardpointSlotsComponent? turretSlots))
                return false;

            foreach (var turretSlot in turretSlots.Slots)
            {
                if (!string.Equals(turretSlot.Id, childSlotId, StringComparison.OrdinalIgnoreCase))
                    continue;

                hardpointType = turretSlot.HardpointType;
                return true;
            }

            return false;
        }

        foreach (var slot in hardpoints.Slots)
        {
            if (!string.Equals(slot.Id, slotId, StringComparison.OrdinalIgnoreCase))
                continue;

            hardpointType = slot.HardpointType;
            return true;
        }

        return false;
    }

    private bool IsHardpointTypeAllowed(VehicleWeaponsSeatComponent seatComp, string hardpointType)
    {
        if (seatComp.AllowedHardpointTypes.Count == 0)
            return true;

        foreach (var allowed in seatComp.AllowedHardpointTypes)
        {
            if (string.Equals(allowed, hardpointType, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool IsSharedHardpointType(string hardpointType)
    {
        return string.Equals(hardpointType, "Support", StringComparison.OrdinalIgnoreCase);
    }

    private void RefreshOperatorSelectedWeapons(
        EntityUid vehicle,
        RMCVehicleWeaponsComponent weapons,
        ItemSlotsComponent? itemSlots = null)
    {
        if (_net.IsClient || !Resolve(vehicle, ref itemSlots, logMissing: false))
            return;

        var query = EntityQueryEnumerator<VehicleWeaponsOperatorComponent>();
        while (query.MoveNext(out var operatorUid, out var operatorComp))
        {
            if (operatorComp.Vehicle != vehicle)
                continue;

            EntityUid? selectedWeapon = null;
            if (weapons.OperatorSelections.TryGetValue(operatorUid, out var slotId) &&
                !string.IsNullOrWhiteSpace(slotId) &&
                TryGetSlotItem(vehicle, slotId, itemSlots, out var selectedUid) &&
                HasComp<GunComponent>(selectedUid))
            {
                selectedWeapon = selectedUid;
            }

            if (operatorComp.SelectedWeapon == selectedWeapon)
                continue;

            operatorComp.SelectedWeapon = selectedWeapon;
            Dirty(operatorUid, operatorComp);
        }
    }

    public bool TryGetSelectedWeaponForOperator(EntityUid vehicle, EntityUid operatorUid, out EntityUid weapon)
    {
        weapon = default;

        if (!TryComp(vehicle, out RMCVehicleWeaponsComponent? weapons))
        {
            return false;
        }

        if (TryComp(vehicle, out ItemSlotsComponent? itemSlots) &&
            weapons.OperatorSelections.TryGetValue(operatorUid, out var selectedSlot) &&
            !string.IsNullOrWhiteSpace(selectedSlot) &&
            TryGetSlotItem(vehicle, selectedSlot, itemSlots, out var selectedWeapon) &&
            HasComp<GunComponent>(selectedWeapon))
        {
            weapon = selectedWeapon;
            return true;
        }

        if (TryComp(operatorUid, out VehicleWeaponsOperatorComponent? operatorComp) &&
            operatorComp.Vehicle == vehicle &&
            operatorComp.SelectedWeapon is { } operatorWeapon &&
            Exists(operatorWeapon) &&
            HasComp<GunComponent>(operatorWeapon))
        {
            weapon = operatorWeapon;
            return true;
        }

        if (weapons.Operator == operatorUid &&
            weapons.SelectedWeapon is { } primaryWeapon &&
            Exists(primaryWeapon) &&
            HasComp<GunComponent>(primaryWeapon))
        {
            weapon = primaryWeapon;
            return true;
        }

        return false;
    }

    public bool TryGetOperatorForSelectedWeapon(EntityUid vehicle, EntityUid weapon, out EntityUid operatorUid)
    {
        operatorUid = default;

        if (!TryComp(vehicle, out RMCVehicleWeaponsComponent? weapons) ||
            !TryComp(vehicle, out ItemSlotsComponent? itemSlots))
        {
            return false;
        }

        foreach (var entry in weapons.OperatorSelections)
        {
            if (!Exists(entry.Key) ||
                string.IsNullOrWhiteSpace(entry.Value) ||
                !TryGetSlotItem(vehicle, entry.Value, itemSlots, out var selectedWeapon) ||
                selectedWeapon != weapon)
            {
                continue;
            }

            operatorUid = entry.Key;
            return true;
        }

        var query = EntityQueryEnumerator<VehicleWeaponsOperatorComponent>();
        while (query.MoveNext(out var candidateUid, out var operatorComp))
        {
            if (operatorComp.Vehicle != vehicle ||
                operatorComp.SelectedWeapon != weapon)
            {
                continue;
            }

            operatorUid = candidateUid;
            return true;
        }

        return false;
    }

    private void RecalculateSelectedWeapon(
        EntityUid vehicle,
        RMCVehicleWeaponsComponent weapons,
        ItemSlotsComponent? itemSlots = null)
    {
        if (weapons.Operator is not { } primaryOperator ||
            !weapons.OperatorSelections.TryGetValue(primaryOperator, out var selectedSlot) ||
            string.IsNullOrWhiteSpace(selectedSlot))
        {
            weapons.SelectedWeapon = null;
            return;
        }

        if (!Resolve(vehicle, ref itemSlots, logMissing: false) ||
            !TryGetSlotItem(vehicle, selectedSlot, itemSlots, out var selectedWeapon) ||
            !HasComp<VehicleTurretComponent>(selectedWeapon) ||
            !HasComp<GunComponent>(selectedWeapon))
        {
            weapons.SelectedWeapon = null;
            return;
        }

        weapons.SelectedWeapon = selectedWeapon;
    }
}

