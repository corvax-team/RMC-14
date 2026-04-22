using Content.Shared._CCM.Vehicle.Fabricator;

namespace Content.Client._CCM.Vehicle.Fabricator.Fabricator;

public sealed class VehicleFabricatorUISystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VehicleFabricatorComponent, AfterAutoHandleStateEvent>(OnFabricatorState);
    }

    private void OnFabricatorState(Entity<VehicleFabricatorComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp(ent, out UserInterfaceComponent? ui))
            return;

        foreach (var open in ui.ClientOpenInterfaces.Values)
        {
            if (open is VehicleFabricatorBui bui)
                bui.Refresh();
        }
    }
}