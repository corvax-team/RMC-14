using System.Linq;
using System.Numerics;
using Content.Client.Message;
using Content.Shared._CCM.Vehicle.Fabricator;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._CCM.Vehicle.Fabricator.Fabricator;

public sealed class VehicleFabricatorWindow : DefaultWindow
{
    public event Action<VehicleFabricatorCategory>? OnCategorySelected;
    public event Action<VehicleType>? OnVehicleSelected;
    public event Action<EntProtoId>? OnPrint;

    public VehicleFabricatorCategory SelectedCategory { get; private set; } = VehicleFabricatorCategory.Primary;
    public VehicleType SelectedVehicle { get; private set; } = VehicleType.Tank;

    private Label PrintingLabel => FindControl<Label>("PrintingLabel");
    private ProgressBar PrintingBar => FindControl<ProgressBar>("PrintingBar");
    private EntityPrototypeView VehiclePreview => FindControl<EntityPrototypeView>("VehiclePreview");
    private BoxContainer PrintablesContainer => FindControl<BoxContainer>("PrintablesContainer");
    private RichTextLabel CategoryTitleLabel => FindControl<RichTextLabel>("CategoryTitleLabel");

    private readonly Dictionary<VehicleFabricatorCategory, Button> _categoryButtons = new();
    private readonly Dictionary<VehicleType, Button> _vehicleButtons = new();

    private string? _printingItemName;
    private TimeSpan? _printAt;
    private TimeSpan _printDelay;
    private Dictionary<string, int> _printedModules = new();
    private Dictionary<VehicleType, HashSet<VehicleFabricatorCategory>> _availableCategories = new();
    private readonly IGameTiming _timing = default!;

    public VehicleFabricatorWindow()
    {
        RobustXamlLoader.Load(this);
        IoCManager.Resolve(ref _timing);

        foreach (var category in Enum.GetValues<VehicleFabricatorCategory>())
        {
            var buttonName = $"{category}CategoryButton";
            var button = FindControl<Button>(buttonName);
            _categoryButtons[category] = button;
            button.OnPressed += _ =>
            {
                SelectedCategory = category;
                OnCategorySelected?.Invoke(SelectedCategory);
                UpdateCategoryButtonsState();
            };
        }

        foreach (var vehicle in Enum.GetValues<VehicleType>())
        {
            if (vehicle == VehicleType.None)
                continue;

            var buttonName = $"{vehicle}VehicleButton";
            var button = FindControl<Button>(buttonName);
            _vehicleButtons[vehicle] = button;
            button.OnPressed += _ =>
            {
                SelectedVehicle = vehicle;
                OnVehicleSelected?.Invoke(SelectedVehicle);
                UpdateVehicleButtonsState();
            };
        }

        _vehicleButtons[VehicleType.Tank].Pressed = true;
        _categoryButtons[VehicleFabricatorCategory.Primary].Pressed = true;

        UpdateCategoryButtonsVisibility();
        UpdateVehiclePreview();
        UpdateCategoryTitle();
    }

    private void UpdateCategoryButtonsState()
    {
        foreach (var (category, btn) in _categoryButtons)
        {
            btn.Pressed = category == SelectedCategory;
        }
        UpdateCategoryTitle();
    }

    private void UpdateCategoryButtonsVisibility()
    {
        var available = _availableCategories.GetValueOrDefault(SelectedVehicle, new HashSet<VehicleFabricatorCategory>());
        foreach (var (category, btn) in _categoryButtons)
        {
            btn.Visible = available.Contains(category);
        }

        if (!available.Contains(SelectedCategory))
        {
            SelectedCategory = available.FirstOrDefault(VehicleFabricatorCategory.Primary);
            UpdateCategoryButtonsState();
        }
    }

    private void UpdateVehicleButtonsState()
    {
        foreach (var (vehicle, btn) in _vehicleButtons)
        {
            btn.Pressed = vehicle == SelectedVehicle;
        }
        UpdateCategoryButtonsVisibility();
        UpdateCategoryTitle();
        UpdateVehiclePreview();
    }

    private void UpdateCategoryTitle()
    {
        var vehicle = SelectedVehicle.ToString().ToLowerInvariant();
        var category = SelectedCategory.ToString().ToLowerInvariant();
        var vehicleLoc = Loc.GetString($"rmc-vehicle-fabricator-vehicle-{vehicle}");
        var categoryLoc = Loc.GetString($"rmc-vehicle-fabricator-category-{category}");
        CategoryTitleLabel.SetMarkupPermissive($"[bold]{vehicleLoc} - {categoryLoc}[/bold]");
    }

    private void UpdateVehiclePreview()
    {
        var protoId = VehicleFabricatorUtils.GetVehicleProtoId(SelectedVehicle);
        if (protoId != null)
            VehiclePreview.SetPrototype(protoId);
    }

    public void SetPrinting(string? itemName, TimeSpan? printAt, TimeSpan printDelay)
    {
        _printingItemName = itemName;
        _printAt = printAt;
        _printDelay = printDelay;
        UpdatePrintingDisplay(0f);
    }

    private void UpdatePrintingDisplay(float progress)
    {
        var isPrinting = _printingItemName != null;
        PrintingLabel.Visible = isPrinting;
        PrintingLabel.Text = isPrinting
            ? Loc.GetString("rmc-vehicle-fabricator-printing", ("item", _printingItemName!))
            : string.Empty;

        PrintingBar.Visible = isPrinting;
        PrintingBar.Value = isPrinting ? progress : 0;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_printingItemName == null || _printAt == null || _printDelay <= TimeSpan.Zero)
            return;

        var printStart = _printAt.Value - _printDelay;
        var elapsed = (float)(_timing.CurTime - printStart).TotalSeconds;
        var total = (float)_printDelay.TotalSeconds;
        var progress = total > 0 ? Math.Clamp(elapsed / total, 0f, 1f) : 0f;
        UpdatePrintingDisplay(progress);
    }

    public void SetCategory(VehicleFabricatorCategory category)
    {
        SelectedCategory = category;
        UpdateCategoryButtonsState();
    }

    public void SetVehicle(VehicleType vehicle)
    {
        SelectedVehicle = vehicle;
        UpdateVehicleButtonsState();
    }

    public void SetPrintedModules(Dictionary<string, int> printedModules)
    {
        _printedModules = printedModules;
    }

    public void SetAvailableCategories(Dictionary<VehicleType, HashSet<VehicleFabricatorCategory>> availableCategories)
    {
        _availableCategories = availableCategories;
        UpdateCategoryButtonsVisibility();
    }

    public void SetPrintables(List<VehicleFabricatorPrintableEntry> printables)
    {
        PrintablesContainer.DisposeAllChildren();

        foreach (var printable in printables)
        {
            var card = new PanelContainer
            {
                HorizontalExpand = true,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = Color.FromHex("#0F1D2E"),
                    BorderColor = Color.FromHex("#1E3450"),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 6,
                    ContentMarginRightOverride = 6,
                    ContentMarginTopOverride = 6,
                    ContentMarginBottomOverride = 6,
                },
            };

            var box = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                HorizontalExpand = true,
            };

            var spritePreview = new EntityPrototypeView
            {
                MinSize = new Vector2(48, 48),
                MaxSize = new Vector2(48, 48),
                Stretch = SpriteView.StretchMode.Fit,
            };
            spritePreview.SetPrototype(printable.Id);
            box.AddChild(spritePreview);

            var labelBox = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                HorizontalExpand = true,
                Margin = new Thickness(6, 0, 0, 0),
            };

            labelBox.AddChild(new Label
            {
                Text = printable.Name,
                FontColorOverride = Color.FromHex("#C7D7EA"),
            });

            labelBox.AddChild(new Label
            {
                Text = printable.Description,
                FontColorOverride = Color.FromHex("#9DB5D1"),
            });

            box.AddChild(labelBox);

            var limitKey = VehicleFabricatorUtils.GetLimitKey(printable.Category, printable.Vehicle);
            var isLimited = _printedModules.GetValueOrDefault(limitKey, 0) >= 1;

            var button = new Button
            {
                Text = Loc.GetString("rmc-vehicle-fabricator-print"),
                MinWidth = 150,
                VerticalAlignment = VAlignment.Center,
                StyleClasses = { "OpenBoth" },
                Disabled = isLimited
            };
            if (!isLimited)
            {
                button.OnPressed += _ => OnPrint?.Invoke(printable.Id);
            }
            box.AddChild(button);

            card.AddChild(box);
            PrintablesContainer.AddChild(card);
        }
    }
}
