using Content.Shared.Buckle.Components;
using Content.Shared._CCM.Vehicle.Components;
using Robust.Shared.Containers;

namespace Content.Shared._CCM.Vehicle;

public sealed partial class VehicleSystem
{
    public void InitializeOperator()
    {
        SubscribeLocalEvent<CCMStrapVehicleComponent, StrappedEvent>(OnVehicleStrapped);
        SubscribeLocalEvent<CCMStrapVehicleComponent, UnstrappedEvent>(OnVehicleUnstrapped);

        SubscribeLocalEvent<CCMContainerVehicleComponent, EntInsertedIntoContainerMessage>(OnContainerEntInserted);
        SubscribeLocalEvent<CCMContainerVehicleComponent, EntRemovedFromContainerMessage>(OnContainerEntRemoved);
    }

    private void OnVehicleStrapped(Entity<CCMStrapVehicleComponent> ent, ref StrappedEvent args)
    {
        if (!TryComp<CCMVehicleComponent>(ent, out var vehicle))
            return;
        TrySetOperator((ent, vehicle), args.Buckle);
    }

    private void OnVehicleUnstrapped(Entity<CCMStrapVehicleComponent> ent, ref UnstrappedEvent args)
    {
        if (!TryComp<CCMVehicleComponent>(ent, out var vehicle))
            return;
        TrySetOperator((ent, vehicle), null);
    }

    private void OnContainerEntInserted(Entity<CCMContainerVehicleComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerId)
            return;

        if (!TryComp<CCMVehicleComponent>(ent, out var vehicle))
            return;

        TrySetOperator((ent, vehicle), args.Entity, removeExisting: false);
    }

    private void OnContainerEntRemoved(Entity<CCMContainerVehicleComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerId)
            return;

        if (!TryComp<CCMVehicleComponent>(ent, out var vehicle))
            return;

        if (vehicle.Operator != args.Entity)
            return;

        TryRemoveOperator((ent, vehicle));
    }
}
