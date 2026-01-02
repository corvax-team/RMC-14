using Content.Client._CCM.Mech.Ui;
using Content.Shared._CCM.Mech;
using Content.Shared._CCM.Mech.Components;
using Robust.Client.UserInterface;

namespace Content.Client._CCM.Mech.Systems;

public sealed class MechEquipmentRadialSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CCMMechComponent, MechOpenEquipmentRadialEvent>(OnOpenEquipmentRadial);
    }
    private void OnOpenEquipmentRadial(Entity<CCMMechComponent> ent, ref MechOpenEquipmentRadialEvent args)
    {
        var controller = _uiManager.GetUIController<MechEquipmentRadialUIController>();
        controller.OpenRadialMenu(ent.Owner);
    }
}
