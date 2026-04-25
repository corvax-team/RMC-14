using System.Linq;
using Content.Shared._CCM.Vehicle.Fabricator;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._CCM.Vehicle.Fabricator.Fabricator;

public sealed class VehicleFabricatorBui : BoundUserInterface
{
    private VehicleFabricatorWindow? _window;
    private List<VehicleFabricatorPrintableEntry>? _allPrintables;

    public VehicleFabricatorBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<VehicleFabricatorWindow>();
        _window.OnClose += Close;
        _window.OnCategorySelected += OnCategorySelected;
        _window.OnVehicleSelected += OnVehicleSelected;
        _window.OnPrint += OnPrint;
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not VehicleFabricatorBuiState fabricatorState)
            return;

        if (_window == null)
            return;

        _allPrintables = fabricatorState.Printables;
        _window.SetPrinting(fabricatorState.PrintingName, fabricatorState.PrintAt, fabricatorState.PrintDelay);
        _window.SetPrintedModules(fabricatorState.PrintedModules);
        _window.SetAvailableCategories(fabricatorState.AvailableCategories);
        UpdatePrintables();
    }

    private void OnCategorySelected(VehicleFabricatorCategory category)
    {
        _window?.SetCategory(category);
        UpdatePrintables();
    }

    private void OnVehicleSelected(VehicleType vehicle)
    {
        _window?.SetVehicle(vehicle);
        UpdatePrintables();
    }

    private void OnPrint(EntProtoId id)
    {
        SendMessage(new VehicleFabricatorPrintMsg(id));
    }

    private void UpdatePrintables()
    {
        if (_window == null || _allPrintables == null)
            return;

        var selectedVehicle = _window.SelectedVehicle;
        var printables = _allPrintables
            .Where(printable => printable.Category == _window.SelectedCategory)
            .Where(printable => printable.Vehicle == VehicleType.None || printable.Vehicle == selectedVehicle)
            .ToList();

        _window.SetPrintables(printables);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _window?.Orphan();
        base.Dispose(disposing);
    }
}
