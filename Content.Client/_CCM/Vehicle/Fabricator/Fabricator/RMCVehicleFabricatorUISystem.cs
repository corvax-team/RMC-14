using Content.Shared._CCM.Vehicle.Fabricator;

namespace Content.Client._CCM.Vehicle.Fabricator.Fabricator;

public sealed class RMCVehicleFabricatorUISystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RMCVehicleFabricatorComponent, AfterAutoHandleStateEvent>(OnFabricatorState);
    }

    private void OnFabricatorState(Entity<RMCVehicleFabricatorComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp(ent, out UserInterfaceComponent? ui))
            return;

        foreach (var open in ui.ClientOpenInterfaces.Values)
        {
            if (open is RMCVehicleFabricatorBui bui)
                bui.Refresh();
        }
    }
}