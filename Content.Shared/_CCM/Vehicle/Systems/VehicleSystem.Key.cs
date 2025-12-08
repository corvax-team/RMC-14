using Content.Shared._CCM.Vehicle.Components;
using Robust.Shared.Containers;

namespace Content.Shared._CCM.Vehicle;

public sealed partial class VehicleSystem
{
    public void InitializeKey()
    {
        SubscribeLocalEvent<CCMGenericKeyedVehicleComponent, ContainerIsInsertingAttemptEvent>(OnGenericKeyedInsertAttempt);
        SubscribeLocalEvent<CCMGenericKeyedVehicleComponent, EntInsertedIntoContainerMessage>(OnGenericKeyedEntInserted);
        SubscribeLocalEvent<CCMGenericKeyedVehicleComponent, EntRemovedFromContainerMessage>(OnGenericKeyedEntRemoved);
        SubscribeLocalEvent<CCMGenericKeyedVehicleComponent, VehicleCanRunEvent>(OnGenericKeyedCanRun);
    }

    private void OnGenericKeyedInsertAttempt(Entity<CCMGenericKeyedVehicleComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        if (args.Cancelled || !ent.Comp.PreventInvalidInsertion || args.Container.ID != ent.Comp.ContainerId)
            return;

        if (_entityWhitelist.IsWhitelistPass(ent.Comp.KeyWhitelist, args.EntityUid))
            return;

        args.Cancel();
    }

    private void OnGenericKeyedEntInserted(Entity<CCMGenericKeyedVehicleComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerId)
            return;
        RefreshCanRun(ent.Owner);
    }

    private void OnGenericKeyedEntRemoved(Entity<CCMGenericKeyedVehicleComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerId)
            return;
        RefreshCanRun(ent.Owner);
    }

    private void OnGenericKeyedCanRun(Entity<CCMGenericKeyedVehicleComponent> ent, ref VehicleCanRunEvent args)
    {
        if (!args.CanRun)
            return;
        // We cannot run by default
        args.CanRun = false;

        if (!_container.TryGetContainer(ent.Owner, ent.Comp.ContainerId, out var container))
            return;

        foreach (var contained in container.ContainedEntities)
        {
            if (_entityWhitelist.IsWhitelistFail(ent.Comp.KeyWhitelist, contained))
                continue;

            // If we find a valid key, permit running and exit early.
            args.CanRun = true;
            break;
        }
    }
}
