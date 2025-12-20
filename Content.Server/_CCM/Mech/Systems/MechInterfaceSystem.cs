using Content.Server._CCM.Mech.Systems;
using Content.Server._CCM.Mech.Components;
using Content.Shared._CCM.Mech;
using Content.Shared._CCM.Mech.Components;
using Content.Shared._CCM.Mech.EntitySystems;
using Content.Server.PowerCell;
using Robust.Server.GameObjects;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.FixedPoint;
using Content.Server.Atmos;
using System.Linq;
using Robust.Shared.Timing;
using Content.Shared.Power.Generator;

namespace Content.Server._CCM.Mech.Systems;

/// <summary>
/// Handles logic for the mech interface.
/// </summary>
/// <remarks>
/// <para>
/// This system is responsible for updating the mech UI state and handling UI interactions.
/// It is not responsible for any mech logic on its own, it merely provides UI functionality.
/// </para>
/// </remarks>
public sealed class MechInterfaceSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = null!;
    [Dependency] private readonly PowerCellSystem _powerCell = null!;
    [Dependency] private readonly ContainerSystem _container = null!;
    [Dependency] private readonly IGameTiming _gameTiming = null!;


    // TODO: make it work to delay value updates
    private static readonly TimeSpan VisualsChangeDelay = TimeSpan.FromSeconds(0.5f);

    public override void Initialize()
    {
        SubscribeLocalEvent<CCMMechComponent, UpdateMechUiEvent>(OnUpdateMechUi);

        Subs.BuiEvents<CCMMechComponent>(CCMMechUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnMechUiOpened);
        });

        Subs.BuiEvents<CCMMechComponent>(
            CCMMechUiKey.Key,
            subs =>
            {
                subs.Event<MechEquipmentRemoveMessage>(HandleEquipmentRemove);
                subs.Event<MechModuleRemoveMessage>(HandleModuleRemove);

                subs.Event<MechEquipmentUiMessage>(HandleEquipmentUiMessageRelay);
                subs.Event<MechGrabberEjectMessage>(HandleEquipmentUiMessageRelay);
                subs.Event<MechSoundboardPlayMessage>(HandleEquipmentUiMessageRelay);
                subs.Event<MechGeneratorEjectFuelMessage>(HandleEquipmentUiMessageRelay);
            });
    }

    private void OnMechUiOpened(Entity<CCMMechComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUI(ent.Owner, ent.Comp);
    }

    private void RelayEquipmentUiMessage(Entity<CCMMechComponent> ent, MechEquipmentUiMessage msg)
    {
        var equipment = GetEntity(msg.Equipment);
        RaiseLocalEvent(equipment, new MechEquipmentUiMessageRelayEvent(msg));
    }

    private void HandleEquipmentUiMessageRelay(Entity<CCMMechComponent> ent, ref MechEquipmentUiMessage args)
    {
        RelayEquipmentUiMessage(ent, args);
    }

    private void HandleEquipmentUiMessageRelay(Entity<CCMMechComponent> ent, ref MechGrabberEjectMessage args)
    {
        RelayEquipmentUiMessage(ent, args);
    }

    private void HandleEquipmentUiMessageRelay(Entity<CCMMechComponent> ent, ref MechSoundboardPlayMessage args)
    {
        RelayEquipmentUiMessage(ent, args);
    }

    private void HandleEquipmentUiMessageRelay(Entity<CCMMechComponent> ent, ref MechGeneratorEjectFuelMessage args)
    {
        RelayEquipmentUiMessage(ent, args);
    }

    private void HandleEquipmentRemove(Entity<CCMMechComponent> ent, ref MechEquipmentRemoveMessage args)
    {
        var equipment = GetEntity(args.Equipment);
        if (!ent.Comp.EquipmentContainer.Contains(equipment))
            return;

        _container.Remove(equipment, ent.Comp.EquipmentContainer);
        UpdateMechUI(ent.Owner);
    }

    private void HandleModuleRemove(Entity<CCMMechComponent> ent, ref MechModuleRemoveMessage args)
    {
        var module = GetEntity(args.Module);
        if (!ent.Comp.ModuleContainer.Contains(module))
            return;

        _container.Remove(module, ent.Comp.ModuleContainer);
        UpdateMechUI(ent.Owner);
    }

    private void OnUpdateMechUi(EntityUid uid, CCMMechComponent component, UpdateMechUiEvent args)
    {
        UpdateUI(uid, component);
    }

    private void UpdateUI(EntityUid uid, CCMMechComponent mechComp)
    {
        if (!_uiSystem.IsUiOpen(uid, CCMMechUiKey.Key))
            return;

        mechComp.LastUiUpdate = _gameTiming.CurTime;

        var equipment = new List<NetEntity>();
        foreach (var ent in mechComp.EquipmentContainer.ContainedEntities)
        {
            equipment.Add(GetNetEntity(ent));
        }

        var modules = new List<NetEntity>();
        foreach (var ent in mechComp.ModuleContainer.ContainedEntities)
        {
            modules.Add(GetNetEntity(ent));
        }

        var moduleUsed = 0;

        // Compute energy from battery
        float energy = 0f;
        float maxEnergy = 0f;
        if (_powerCell.TryGetBatteryFromSlot(uid, out var battery))
        {
            energy = battery.CurrentCharge;
            maxEnergy = battery.MaxCharge;
        }

        var state = new MechBoundUiState
        {
            Equipment = equipment,
            Modules = modules,
            ModuleSpaceMax = mechComp.MaxModuleAmount,
            ModuleSpaceUsed = moduleUsed,
            PilotPresent = mechComp.PilotSlot.ContainedEntity != null,
            Integrity = mechComp.Integrity.Float(),
            MaxIntegrity = mechComp.MaxIntegrity.Float(),
            Energy = energy,
            MaxEnergy = maxEnergy,
            EquipmentUsed = mechComp.EquipmentContainer.ContainedEntities.Count,
            MaxEquipmentAmount = mechComp.MaxEquipmentAmount,
            IsBroken = mechComp.Broken,
        };

        // Collect equipment and module UI states
        CollectEquipmentUiStates(mechComp.EquipmentContainer.ContainedEntities, state.EquipmentUiStates);
        CollectEquipmentUiStates(mechComp.ModuleContainer.ContainedEntities, state.EquipmentUiStates);

        _uiSystem.SetUiState(uid, CCMMechUiKey.Key, state);
    }

    private void UpdateMechUI(EntityUid uid)
    {
        RaiseLocalEvent(uid, new UpdateMechUiEvent());
    }

    private void CollectEquipmentUiStates(IEnumerable<EntityUid> entities, Dictionary<NetEntity, BoundUserInterfaceState> states)
    {
        foreach (var entity in entities)
        {
            var evt = new MechEquipmentUiStateReadyEvent();
            RaiseLocalEvent(entity, evt);
            if (evt.States.Count == 0)
                continue;

            foreach (var (netEntity, state) in evt.States)
            {
                states[netEntity] = state;
            }
        }
    }
}
