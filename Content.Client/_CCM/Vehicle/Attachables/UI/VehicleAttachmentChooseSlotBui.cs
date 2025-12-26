using Content.Shared._CCM.Attachables;
using Robust.Client.UserInterface;

namespace Content.Client._CCM.Vehicle.Attachables.UI;

public sealed class VehicleAttachmentChooseSlotBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private VehicleAttachableHolderChooseSlotMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<VehicleAttachableHolderChooseSlotMenu>();

        var metaQuery = EntMan.GetEntityQuery<MetaDataComponent>();
        if (metaQuery.TryGetComponent(Owner, out var metadata))
            _menu.Title = metadata.EntityName;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not VehicleAttachableHolderChooseSlotUserInterfaceState msg)
            return;

        if (_menu == null)
            return;

        _menu.UpdateMenu(msg.AttachableSlots,
            slotId =>
            {
                SendPredictedMessage(new VehicleAttachableHolderAttachToSlotMessage(slotId));
                _menu.Close();
            });
    }
}
