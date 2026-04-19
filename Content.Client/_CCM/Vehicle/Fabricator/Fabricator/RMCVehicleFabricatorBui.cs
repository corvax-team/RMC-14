using Content.Shared._CCM.Vehicle.Fabricator;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._CCM.Vehicle.Fabricator.Fabricator;

public sealed class RMCVehicleFabricatorBui : BoundUserInterface
{
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    private readonly RMCVehicleFabricatorSystem _system;

    private RMCVehicleFabricatorWindow? _window;

    public RMCVehicleFabricatorBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
        _system = EntMan.System<RMCVehicleFabricatorSystem>();
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<RMCVehicleFabricatorWindow>();
        _window.OnClose += Close;
        _window.OnCategorySelected += OnCategorySelected;
        _window.OnVehicleSelected += OnVehicleSelected;
        _window.OnPrint += OnPrint;
        _window.OpenCentered();

        Refresh();
    }

    public void Refresh()
    {
        if (_window == null || !EntMan.TryGetComponent(Owner, out RMCVehicleFabricatorComponent? fabricator))
            return;

        _window.SetPoints(fabricator.Points);

        string? printingName = null;
        TimeSpan? printAt = null;
        var printDelay = TimeSpan.Zero;

        if (fabricator.Printing != null && _prototype.TryIndex(fabricator.Printing.Value, out var proto))
        {
            printingName = proto.Name;
            printAt = fabricator.PrintAt;

            if (proto.TryGetComponent(out RMCVehicleFabricatorPrintableComponent? printable, _compFactory))
            {
                printDelay = printable.Delay;
            }
        }

        _window.SetPrinting(printingName, printAt, printDelay);

        UpdatePrintables();
    }

    private void OnCategorySelected(RMCVehicleFabricatorCategory category)
    {
        _window?.SetCategory(category);
        UpdatePrintables();
    }

    private void OnVehicleSelected(RMCVehicleType vehicle)
    {
        _window?.SetVehicle(vehicle);
        UpdatePrintables();
    }

    private void OnPrint(EntProtoId id)
    {
        SendMessage(new RMCVehicleFabricatorPrintMsg(id));
    }

    private void UpdatePrintables()
    {
        if (_window == null)
            return;

        var printables = new List<RMCVehicleFabricatorPrintableDisplayData>();
        foreach (var printableId in _system.Printables)
        {
            if (!_prototype.TryIndex(printableId, out var proto) ||
                !printableId.TryGet(out var printable, _prototype, _compFactory))
                continue;

            if (!printable.Enabled)
                continue;

            if (printable.Category != _window.SelectedCategory)
                continue;

            if (printable.Vehicle != RMCVehicleType.None &&
                (printable.Vehicle & _window.SelectedVehicle) == 0)
                continue;

            printables.Add(new RMCVehicleFabricatorPrintableDisplayData(
                printableId,
                proto.Name,
                proto.Description,
                printable.Cost));
        }

        _window.SetPrintables(printables);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _window?.Orphan();
        base.Dispose(disposing);
    }
}