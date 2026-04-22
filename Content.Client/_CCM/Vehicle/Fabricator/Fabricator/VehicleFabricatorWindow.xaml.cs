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
    public event Action<RMCVehicleType>? OnVehicleSelected;
    public event Action<EntProtoId>? OnPrint;

    public VehicleFabricatorCategory SelectedCategory { get; private set; } = VehicleFabricatorCategory.Primary;
    public RMCVehicleType SelectedVehicle { get; private set; } = RMCVehicleType.Tank;

    private Label PointsLabel => FindControl<Label>("PointsLabel");
    private Label PrintingLabel => FindControl<Label>("PrintingLabel");
    private ProgressBar PrintingBar => FindControl<ProgressBar>("PrintingBar");
    private EntityPrototypeView VehiclePreview => FindControl<EntityPrototypeView>("VehiclePreview");
    private BoxContainer PrintablesContainer => FindControl<BoxContainer>("PrintablesContainer");
    private RichTextLabel CategoryTitleLabel => FindControl<RichTextLabel>("CategoryTitleLabel");

    private string? _printingItemName;
    private TimeSpan? _printAt;
    private TimeSpan _printDelay;
    private IGameTiming _timing = default!;

    public VehicleFabricatorWindow()
    {
        RobustXamlLoader.Load(this);
        IoCManager.Resolve(ref _timing);

        var primaryBtn = FindControl<Button>("PrimaryCategoryButton");
        var secondaryBtn = FindControl<Button>("SecondaryCategoryButton");
        var armorBtn = FindControl<Button>("ArmorCategoryButton");
        var supportBtn = FindControl<Button>("SupportCategoryButton");
        var chassisBtn = FindControl<Button>("ChassisCategoryButton");
        var ammoBtn = FindControl<Button>("AmmoCategoryButton");

        var tankBtn = FindControl<Button>("TankVehicleButton");
        var apcBtn = FindControl<Button>("APCVehicleButton");
        var humveeBtn = FindControl<Button>("HumveeVehicleButton");

        primaryBtn.OnPressed += _ => { SelectedCategory = VehicleFabricatorCategory.Primary; OnCategorySelected?.Invoke(SelectedCategory); UpdateCategoryButtons(primaryBtn); };
        secondaryBtn.OnPressed += _ => { SelectedCategory = VehicleFabricatorCategory.Secondary; OnCategorySelected?.Invoke(SelectedCategory); UpdateCategoryButtons(secondaryBtn); };
        armorBtn.OnPressed += _ => { SelectedCategory = VehicleFabricatorCategory.Armor; OnCategorySelected?.Invoke(SelectedCategory); UpdateCategoryButtons(armorBtn); };
        supportBtn.OnPressed += _ => { SelectedCategory = VehicleFabricatorCategory.Support; OnCategorySelected?.Invoke(SelectedCategory); UpdateCategoryButtons(supportBtn); };
        chassisBtn.OnPressed += _ => { SelectedCategory = VehicleFabricatorCategory.Chassis; OnCategorySelected?.Invoke(SelectedCategory); UpdateCategoryButtons(chassisBtn); };
        ammoBtn.OnPressed += _ => { SelectedCategory = VehicleFabricatorCategory.Ammo; OnCategorySelected?.Invoke(SelectedCategory); UpdateCategoryButtons(ammoBtn); };

        tankBtn.OnPressed += _ => { SelectedVehicle = RMCVehicleType.Tank; OnVehicleSelected?.Invoke(SelectedVehicle); UpdateVehicleButtons(tankBtn); };
        apcBtn.OnPressed += _ => { SelectedVehicle = RMCVehicleType.APC; OnVehicleSelected?.Invoke(SelectedVehicle); UpdateVehicleButtons(apcBtn); };
        humveeBtn.OnPressed += _ => { SelectedVehicle = RMCVehicleType.Humvee; OnVehicleSelected?.Invoke(SelectedVehicle); UpdateVehicleButtons(humveeBtn); };

        tankBtn.Pressed = true;
        primaryBtn.Pressed = true;
        UpdateVehiclePreview();
        UpdateCategoryTitle();
    }

    private void UpdateCategoryButtons(Button pressed)
    {
        var categories = new[] { "Primary", "Secondary", "Armor", "Support", "Chassis", "Ammo" };
        foreach (var name in categories)
        {
            var btn = FindControl<Button>($"{name}CategoryButton");
            btn.Pressed = btn == pressed;
        }
        UpdateCategoryTitle();
    }

    private void UpdateVehicleButtons(Button pressed)
    {
        var vehicles = new[] { "Tank", "APC", "Humvee" };
        foreach (var name in vehicles)
        {
            var btn = FindControl<Button>($"{name}VehicleButton");
            btn.Pressed = btn == pressed;
        }
        UpdateCategoryTitle();
        UpdateVehiclePreview();
    }

    private void UpdateCategoryTitle()
    {
        var vehicle = GetVehicleKey(SelectedVehicle);
        var category = GetCategoryKey(SelectedCategory);
        var vehicleLoc = Loc.GetString($"rmc-vehicle-fabricator-vehicle-{vehicle}");
        var categoryLoc = Loc.GetString($"rmc-vehicle-fabricator-category-{category}");
        CategoryTitleLabel.SetMarkupPermissive($"[bold]{vehicleLoc} - {categoryLoc}[/bold]");
    }

    private static string GetVehicleKey(RMCVehicleType type) => type switch
    {
        RMCVehicleType.Tank => "tank",
        RMCVehicleType.APC => "apc",
        RMCVehicleType.Humvee => "humvee",
        _ => type.ToString().ToLowerInvariant(),
    };

    private static string GetVehicleProtoId(RMCVehicleType type) => type switch
    {
        RMCVehicleType.Tank => "RMCVehicleTank",
        RMCVehicleType.APC => "RMCVehicleAPC",
        RMCVehicleType.Humvee => "RMCVehicleHumvee",
        _ => "RMCVehicleTank",
    };

    private void UpdateVehiclePreview()
    {
        var protoId = GetVehicleProtoId(SelectedVehicle);
        VehiclePreview.SetPrototype(protoId);
    }

    private static string GetCategoryKey(VehicleFabricatorCategory category) => category switch
    {
        VehicleFabricatorCategory.Primary => "primary",
        VehicleFabricatorCategory.Secondary => "secondary",
        VehicleFabricatorCategory.Armor => "armor",
        VehicleFabricatorCategory.Support => "support",
        VehicleFabricatorCategory.Chassis => "chassis",
        VehicleFabricatorCategory.Ammo => "ammo",
        _ => category.ToString().ToLowerInvariant(),
    };

    public void SetPoints(int points)
    {
        PointsLabel.Text = Loc.GetString("rmc-vehicle-fabricator-points", ("points", points));
    }

    public void SetPrinting(string? itemName, float progress = 0f)
    {
        _printingItemName = itemName;
        _printAt = null;
        _printDelay = TimeSpan.Zero;
        UpdatePrintingDisplay(progress);
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
        var categoryNames = new[] { "Primary", "Secondary", "Armor", "Support", "Chassis", "Ammo" };
        foreach (var name in categoryNames)
        {
            var btn = FindControl<Button>($"{name}CategoryButton");
            btn.Pressed = GetCategoryKey(category) == name.ToLowerInvariant();
        }
        UpdateCategoryTitle();
    }

    public void SetVehicle(RMCVehicleType vehicle)
    {
        SelectedVehicle = vehicle;
        var vehicleNames = new[] { "Tank", "APC", "Humvee" };
        foreach (var name in vehicleNames)
        {
            var btn = FindControl<Button>($"{name}VehicleButton");
            btn.Pressed = GetVehicleKey(vehicle) == name.ToLowerInvariant();
        }
        UpdateCategoryTitle();
        UpdateVehiclePreview();
    }

    public void SetPrintables(List<VehicleFabricatorPrintableDisplayData> printables)
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

            var button = new Button
            {
                Text = Loc.GetString("rmc-vehicle-fabricator-print", ("cost", printable.Cost)),
                MinWidth = 150,
                VerticalAlignment = VAlignment.Center,
                StyleClasses = { "OpenBoth" }
            };
            button.OnPressed += _ => OnPrint?.Invoke(printable.Id);
            box.AddChild(button);

            card.AddChild(box);
            PrintablesContainer.AddChild(card);
        }
    }
}
