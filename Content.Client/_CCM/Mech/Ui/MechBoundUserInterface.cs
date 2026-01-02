using Content.Client.UserInterface;
using Content.Client.UserInterface.Fragments;
using Content.Shared._CCM.Mech;
using Content.Shared._CCM.Mech.Components;
using Content.Shared._CCM.Mech.EntitySystems;
using JetBrains.Annotations;
using Robust.Client.Timing;
using Robust.Client.UserInterface;

namespace Content.Client._CCM.Mech.Ui;

[UsedImplicitly]
public sealed class MechBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClientGameTiming _gameTiming = null!;

    [ViewVariables]
    private CCMMechMenu? _menu;
    private BuiPredictionState? _pred;
    public MechBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _pred = new BuiPredictionState(this, _gameTiming);

        _menu = this.CreateWindowCenteredLeft<CCMMechMenu>();
        _menu.SetEntity(Owner);
        _menu.SetParentBui(this);

        // Equipment and module removal
        _menu.OnRemoveButtonPressed += uid =>
        {
            _pred!.SendMessage(new MechEquipmentRemoveMessage(EntMan.GetNetEntity(uid)));
        };
        _menu.OnRemoveModuleButtonPressed += uid =>
        {
            _pred!.SendMessage(new MechModuleRemoveMessage(EntMan.GetNetEntity(uid)));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not MechBoundUiState mechState)
            return;

        _menu?.UpdateState(mechState);
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        base.ReceiveMessage(message);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _menu?.Close();
        _menu = null;
    }
}
