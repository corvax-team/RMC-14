// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.Client._CCM.UserInterface.Controls;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Shared._RMC14.CCVar;
using Content.Shared._CCM.Sponsorship;
using Robust.Shared.Configuration;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.IoC;
using Robust.Shared.Input;
using Robust.Shared.Maths;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._CCM.Sponsorship;

public sealed partial class CCMCustomizationWindow : DefaultCMWindow
{
    private const float DefaultWindowWidth = 1100f;
    private const float DefaultWindowHeight = 970f;
    private const float CompactMinWidth = 780f;
    private const float CompactMinHeight = 640f;

    private enum CustomizationPage : byte
    {
        Xeno,
        Marines,
        Misc,
    }

    private readonly record struct CustomOption(string Id, string NameKey, string? PreviewTexturePath = null);

    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    private readonly Dictionary<string, CCMOptionButton> _selectors = new();
    private readonly Dictionary<string, TextureRect> _xenoPreviewTextures = new();
    private readonly Dictionary<string, TextureRect> _dynamicPreviewTextures = new();
    private readonly Dictionary<string, Label> _camoPreviewLabels = new();
    private readonly List<(Control Overlay, Func<bool> Visible)> _availabilityOverlays = new();
    private readonly Dictionary<CustomizationPage, Button> _pageButtons = new();
    private readonly List<Action> _themeRefreshActions = new();
    private readonly Label _statusLabel;
    private readonly Label _statusHintLabel;
    private readonly Label _heroTitleLabel;
    private readonly Label _heroWipLabel;
    private readonly Label _saveStateLabel;
    private readonly Label _tagPreviewLabel;
    private readonly Label _oocColorPreviewLabel;
    private readonly Label _loocColorPreviewLabel;
    private readonly Button _saveButton;
    private readonly CCMOptionButton _oocTagSelector;
    private readonly CCMOptionButton _oocColorSelector;
    private readonly CCMOptionButton _loocColorSelector;
    private readonly LineEdit _customTagEdit;
    private readonly Control _xenoPage;
    private readonly Control _marinesPage;
    private readonly Control _miscPage;
    private CCMSponsorshipStatusSnapshot? _status;
    private CCMCustomizationSnapshot? _savedSnapshot;
    private CustomizationPage _currentPage = CustomizationPage.Marines;
    private bool _suppressAutoSave;

    public event Action<CCMCustomizationSnapshot>? SaveRequested;

    private static readonly Dictionary<string, CustomOption[]> SlotOptions = new()
    {
        ["xeno_defender"] =
        [
            new("default", "ccm-customization-default"),
            new("ccm_defender_skin", "ccm-customization-xeno-defender", "/Textures/_CCM14/Mobs/Xenonids/Skins/Defender/first.png"),
        ],
        ["xeno_drone"] =
        [
            new("default", "ccm-customization-default"),
            new("ccm_drone_skin", "ccm-customization-xeno-drone", "/Textures/_CCM14/Mobs/Xenonids/Skins/Drone/first.png"),
        ],
        ["xeno_queen"] =
        [
            new("default", "ccm-customization-default"),
            new("ccm_queen_skin", "ccm-customization-xeno-queen", "/Textures/_CCM14/Mobs/Xenonids/Skins/Queen/first.png"),
        ],
        ["xeno_runner"] =
        [
            new("default", "ccm-customization-default"),
            new("ccm_runner_skin", "ccm-customization-xeno-runner", "/Textures/_CCM14/Mobs/Xenonids/Skins/Runner/first.png"),
        ],
        ["xeno_sentinel"] =
        [
            new("default", "ccm-customization-default"),
            new("ccm_sentinel_skin", "ccm-customization-xeno-sentinel", "/Textures/_CCM14/Mobs/Xenonids/Skins/Sentinel/first.png"),
        ],
        ["ghost"] =
        [
            new("default", "ccm-customization-default"),
            new("holo_green", "ccm-customization-ghost-holo-green", "/Textures/Mobs/Ghosts/ghost_human.rsi/icon.png"),
            new("holo_blue", "ccm-customization-ghost-holo-blue", "/Textures/Mobs/Ghosts/ghost_human.rsi/icon.png"),
            new("holo_violet", "ccm-customization-ghost-holo-violet", "/Textures/Mobs/Ghosts/ghost_human.rsi/icon.png"),
            new("holo_amber", "ccm-customization-ghost-holo-amber", "/Textures/Mobs/Ghosts/ghost_human.rsi/icon.png"),
            new("holo_crimson", "ccm-customization-ghost-holo-crimson", "/Textures/Mobs/Ghosts/ghost_human.rsi/icon.png"),
            new("holo_teal", "ccm-customization-ghost-holo-teal", "/Textures/Mobs/Ghosts/ghost_human.rsi/icon.png"),
            new("sponsor_pretor", "ccm-customization-ghost-sponsor-pretor", "/Textures/_CCM14/Mobs/Ghost/sponsorGhostPretor.rsi/animated.png"),
            new("sponsor_runi", "ccm-customization-ghost-sponsor-runi", "/Textures/_CCM14/Mobs/Ghost/sponsorGhostRuni.rsi/animated.png"),
            new("sponsor_queen", "ccm-customization-ghost-sponsor-queen", "/Textures/_CCM14/Mobs/Ghost/sponsorGhostQueen.rsi/animated.png"),
            new("sponsor_facehugger", "ccm-customization-ghost-sponsor-facehugger", "/Textures/_CCM14/Mobs/Ghost/sponsorGhostFacehugger.rsi/animated.png"),
        ],
        ["weapon_spray"] =
        [
            new(CCMCustomizationCamouflageIds.Default, "ccm-customization-default"),
            new(CCMCustomizationCamouflageIds.Jungle, "ccm-customization-camo-jungle"),
            new(CCMCustomizationCamouflageIds.Desert, "ccm-customization-camo-desert"),
            new(CCMCustomizationCamouflageIds.Snow, "ccm-customization-camo-snow"),
            new(CCMCustomizationCamouflageIds.Classic, "ccm-customization-camo-classic"),
            new(CCMCustomizationCamouflageIds.Urban, "ccm-customization-camo-urban"),
        ],
        ["armor_palette"] =
        [
            new(CCMCustomizationCamouflageIds.Default, "ccm-customization-default"),
            new(CCMCustomizationCamouflageIds.Jungle, "ccm-customization-camo-jungle"),
            new(CCMCustomizationCamouflageIds.Desert, "ccm-customization-camo-desert"),
            new(CCMCustomizationCamouflageIds.Snow, "ccm-customization-camo-snow"),
            new(CCMCustomizationCamouflageIds.Classic, "ccm-customization-camo-classic"),
            new(CCMCustomizationCamouflageIds.Urban, "ccm-customization-camo-urban"),
        ],
        ["armor_variant"] =
        [
            new(CCMCustomizationArmorVariantIds.None, "ccm-customization-none"),
            new(CCMCustomizationArmorVariantIds.Padded, "ccm-customization-armor-variant-padded"),
            new(CCMCustomizationArmorVariantIds.Padless, "ccm-customization-armor-variant-padless"),
            new(CCMCustomizationArmorVariantIds.Ridged, "ccm-customization-armor-variant-ridged"),
            new(CCMCustomizationArmorVariantIds.Carrier, "ccm-customization-armor-variant-carrier"),
            new(CCMCustomizationArmorVariantIds.Skull, "ccm-customization-armor-variant-skull"),
            new(CCMCustomizationArmorVariantIds.Smooth, "ccm-customization-armor-variant-smooth"),
        ],
        ["armor_paint"] =
        [
            new("default", "ccm-customization-default"),
            new("skull", "ccm-customization-armor-skull"),
            new("heart", "ccm-customization-armor-heart"),
            new("medic", "ccm-customization-armor-medic"),
            new("un", "ccm-customization-armor-un"),
            new("target", "ccm-customization-armor-target"),
            new("smiley", "ccm-customization-armor-smiley"),
            new("neutral", "ccm-customization-armor-neutral"),
            new("cross", "ccm-customization-armor-cross"),
            new("inscription", "ccm-customization-armor-inscription"),
            new("mixtape", "ccm-customization-armor-mixtape"),
        ],
    };

    private static readonly Dictionary<string, string> DefaultXenoPreviewPaths = new()
    {
        ["xeno_defender"] = "/Textures/_RMC14/Mobs/Xenonids/Defender/defender.rsi/defender.png",
        ["xeno_drone"] = "/Textures/_RMC14/Mobs/Xenonids/Drone/drone.rsi/drone.png",
        ["xeno_queen"] = "/Textures/_RMC14/Mobs/Xenonids/Queen/queen.rsi/queen.png",
        ["xeno_runner"] = "/Textures/_RMC14/Mobs/Xenonids/Runner/runner.rsi/runner.png",
        ["xeno_sentinel"] = "/Textures/_RMC14/Mobs/Xenonids/Sentinel/sentinel.rsi/sentinel.png",
    };

    private static readonly CustomOption[] OocTagOptions =
    [
        new(CCMOocTags.None, "ccm-customization-tag-none"),
        new("predator", "ccm-customization-tag-predator"),
        new("medic", "ccm-customization-tag-medic"),
        new("engineer", "ccm-customization-tag-engineer"),
        new("veteran", "ccm-customization-tag-veteran"),
        new("recon", "ccm-customization-tag-recon"),
        new("assault", "ccm-customization-tag-assault"),
        new("hive", "ccm-customization-tag-hive"),
        new(CCMOocTags.Custom, "ccm-customization-tag-custom"),
    ];

    private static readonly CustomOption[] ChatColorOptions =
    [
        new(CCMChatColorPresets.Default, "ccm-customization-color-default"),
        new("mint", "ccm-customization-color-mint"),
        new("azure", "ccm-customization-color-azure"),
        new("amber", "ccm-customization-color-amber"),
        new("rose", "ccm-customization-color-rose"),
        new("violet", "ccm-customization-color-violet"),
        new("crimson", "ccm-customization-color-crimson"),
    ];

    private static readonly Dictionary<string, int> GhostPreviewFrameSizes = new()
    {
        ["sponsor_pretor"] = 64,
        ["sponsor_runi"] = 64,
        ["sponsor_queen"] = 64,
        ["sponsor_facehugger"] = 48,
    };

    public CCMCustomizationWindow()
    {
        IoCManager.InjectDependencies(this);
        Stylesheet = IoCManager.Resolve<IStylesheetManager>().SheetNano;

        Title = string.Empty;
        MinSize = SetSize = new Vector2(DefaultWindowWidth, DefaultWindowHeight);
        WindowTitleLabel.Visible = false;
        HeaderPanel.MinSize = new Vector2(0, 26);
        HeaderPanel.Margin = new Thickness(10, 6, 10, 0);
        BodyPanel.Margin = new Thickness(10, -1, 10, 10);

        ApplyWindowTheme();

        _statusLabel = new Label
        {
            FontColorOverride = GetWindowAccent(),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 12),
        };
        _statusHintLabel = new Label
        {
            FontColorOverride = Color.FromHex("#B7C3CE"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 11),
            HorizontalExpand = true,
        };
        _heroTitleLabel = new Label
        {
            Text = Loc.GetString("ccm-customization-header"),
            FontColorOverride = GetWindowAccent(),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 24),
        };
        _heroWipLabel = new Label
        {
            Text = Loc.GetString("ccm-customization-wip"),
            FontColorOverride = GetThemeAccent(0.22f),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 10),
        };
        _saveStateLabel = new Label
        {
            FontColorOverride = Color.FromHex("#8FA2B5"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 11),
            HorizontalExpand = true,
            VerticalAlignment = VAlignment.Center,
        };
        _tagPreviewLabel = new Label
        {
            FontColorOverride = Color.White,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 14),
            HorizontalExpand = true,
            VerticalAlignment = VAlignment.Center,
        };
        _oocColorPreviewLabel = new Label
        {
            FontColorOverride = Color.White,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13),
            HorizontalExpand = true,
            VerticalAlignment = VAlignment.Center,
        };
        _loocColorPreviewLabel = new Label
        {
            FontColorOverride = Color.White,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13),
            HorizontalExpand = true,
            VerticalAlignment = VAlignment.Center,
        };

        _oocTagSelector = MakeOocTagSelector();
        _oocColorSelector = MakeChatColorSelector(false);
        _loocColorSelector = MakeChatColorSelector(true);
        _customTagEdit = MakeCustomTagEdit();

        _saveButton = new Button
        {
            Text = Loc.GetString("ccm-customization-save"),
            MinSize = new Vector2(174, 30),
        };
        _saveButton.OnPressed += _ =>
        {
            if (_saveButton.Disabled)
                return;

            SaveRequested?.Invoke(BuildSnapshot());
        };
        _saveButton.OnMouseEntered += _ => ApplySaveButtonStyle(hovered: true);
        _saveButton.OnMouseExited += _ => ApplySaveButtonStyle();
        _saveButton.OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplySaveButtonStyle(pressed: true);
        };
        _saveButton.OnKeyBindUp += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplySaveButtonStyle();
        };

        _xenoPage = BuildXenoPage();
        _marinesPage = BuildMarinesPage();
        _miscPage = BuildMiscPage();

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 10,
            Margin = new Thickness(12, 2, 12, 12),
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        root.AddChild(BuildHeroPanel());
        root.AddChild(BuildPageTabs());

        var scroll = new ScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            HScrollEnabled = false,
        };

        var content = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 0,
            HorizontalExpand = true,
        };

        content.AddChild(_marinesPage);
        content.AddChild(_xenoPage);
        content.AddChild(_miscPage);

        scroll.AddChild(content);
        root.AddChild(scroll);
        root.AddChild(BuildBottomActionBar());

        Contents.AddChild(root);
        ApplyThemeRefreshActions();
        UpdateStatusText();
        UpdateOocTagControls();
        UpdateTagPreview();
        UpdatePageState();
        UpdateAllXenoPreviewSelections();
        UpdateDynamicPreviews();
        _savedSnapshot = BuildSnapshot();
        UpdateSaveState();
        _config.OnValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged, false);
        _config.OnValueChanged(RMCCVars.RMCLobbyUiStyle, OnThemeChanged, false);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        UpdateResponsiveLayout();
        base.FrameUpdate(args);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _config.UnsubValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged);
            _config.UnsubValueChanged(RMCCVars.RMCLobbyUiStyle, OnThemeChanged);
    }

    private void OnThemeChanged(string _)
    {
        ApplyWindowTheme();
        ApplyThemeRefreshActions();
        _statusLabel.FontColorOverride = GetWindowAccent();
        _statusHintLabel.FontColorOverride = Color.FromHex("#B7C3CE");
        _heroTitleLabel.FontColorOverride = GetWindowAccent();
        _heroWipLabel.FontColorOverride = GetThemeAccent(0.22f);
        UpdatePageState();
        UpdateSaveState();
    }

    private void UpdateResponsiveLayout()
    {
        var viewport = Parent?.Size ?? Vector2.Zero;
        if (viewport.X <= 1f || viewport.Y <= 1f)
            return;

        var maxWidth = MathF.Min(DefaultWindowWidth, viewport.X * 0.90f);
        var maxHeight = MathF.Min(DefaultWindowHeight, viewport.Y * 0.88f);
        var minWidth = MathF.Min(CompactMinWidth, maxWidth);
        var minHeight = MathF.Min(CompactMinHeight, maxHeight);
        var responsiveMin = new Vector2(minWidth, minHeight);

        if (Vector2.DistanceSquared(MinSize, responsiveMin) > 1f)
            MinSize = responsiveMin;

        var clampedSize = new Vector2(
            Math.Clamp(SetSize.X, minWidth, maxWidth),
            Math.Clamp(SetSize.Y, minHeight, maxHeight));

        if (Vector2.DistanceSquared(SetSize, clampedSize) > 1f)
            SetSize = clampedSize;
    }

    public void SetStatus(CCMSponsorshipStatusSnapshot snapshot)
    {
        _status = snapshot;
        UpdateStatusText();
        UpdateAvailability();
    }

    public void SetSnapshot(CCMCustomizationSnapshot snapshot)
    {
        _suppressAutoSave = true;

        foreach (var selection in snapshot.Selections)
        {
            if (!_selectors.TryGetValue(selection.SlotId, out var selector))
                continue;

            var options = SlotOptions[selection.SlotId];
            var index = Array.FindIndex(options, option => option.Id == NormalizeValue(selection.SlotId, selection.ValueId));
            selector.SelectId(index >= 0 ? index : 0);
        }

        var tagIndex = Array.FindIndex(OocTagOptions, option => option.Id == snapshot.SelectedOocTagId);
        _oocTagSelector.SelectId(tagIndex >= 0 ? tagIndex : 0);
        var oocColorIndex = Array.FindIndex(ChatColorOptions, option => option.Id == snapshot.SelectedOocColorId);
        _oocColorSelector.SelectId(oocColorIndex >= 0 ? oocColorIndex : 0);
        var loocColorIndex = Array.FindIndex(ChatColorOptions, option => option.Id == snapshot.SelectedLoocColorId);
        _loocColorSelector.SelectId(loocColorIndex >= 0 ? loocColorIndex : 0);
        _customTagEdit.Text = snapshot.CustomOocTagText;
        ApplyWindowTheme();
        ApplyThemeRefreshActions();
        UpdateOocTagControls();
        UpdateTagPreview();
        UpdateStatusText();
        UpdateAvailability();
        UpdateAllXenoPreviewSelections();
        UpdateDynamicPreviews();
        _suppressAutoSave = false;
        _savedSnapshot = BuildSnapshot();
        UpdateSaveState();
    }

    private Control BuildHeroPanel()
    {
        var hero = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.36f),
                BorderColor = GetWindowAccent().WithAlpha(0.40f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 14,
                ContentMarginTopOverride = 14,
                ContentMarginRightOverride = 14,
                ContentMarginBottomOverride = 14,
            },
        };

        var stack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 12,
        };

        var heroAccentLine = new PanelContainer
        {
            MinSize = new Vector2(0, 5),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = GetWindowAccent().WithAlpha(0.90f),
            },
        };
        stack.AddChild(heroAccentLine);

        var titleRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 14,
            HorizontalExpand = true,
        };

        var titleStack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 4,
            HorizontalExpand = true,
        };
        titleStack.AddChild(_heroTitleLabel);

        titleRow.AddChild(titleStack);
        stack.AddChild(titleRow);

        var infoGrid = new GridContainer
        {
            Columns = 2,
            HSeparationOverride = 12,
            VSeparationOverride = 12,
        };
        infoGrid.AddChild(BuildHeroInfoCard(_statusLabel, () => GetWindowAccent()));
        infoGrid.AddChild(BuildHeroInfoCard(_statusHintLabel, () => GetThemeAccent(0.18f)));
        stack.AddChild(infoGrid);
        stack.AddChild(_heroWipLabel);

        hero.AddChild(stack);
        _themeRefreshActions.Add(() =>
        {
            hero.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.36f),
                BorderColor = GetWindowAccent().WithAlpha(0.40f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 14,
                ContentMarginTopOverride = 14,
                ContentMarginRightOverride = 14,
                ContentMarginBottomOverride = 14,
            };
            heroAccentLine.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = GetWindowAccent().WithAlpha(0.90f),
            };
            _heroTitleLabel.FontColorOverride = GetWindowAccent();
            _heroWipLabel.FontColorOverride = GetThemeAccent(0.22f);
        });
        return hero;
    }

    private Control BuildBottomActionBar()
    {
        var bar = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.28f),
                BorderColor = GetWindowAccent().WithAlpha(0.28f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 10,
                ContentMarginTopOverride = 8,
                ContentMarginRightOverride = 10,
                ContentMarginBottomOverride = 8,
            },
        };

        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 10,
            HorizontalExpand = true,
        };

        row.AddChild(_saveStateLabel);
        row.AddChild(_saveButton);
        bar.AddChild(row);
        _themeRefreshActions.Add(() =>
        {
            bar.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.28f),
                BorderColor = GetWindowAccent().WithAlpha(0.28f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 10,
                ContentMarginTopOverride = 8,
                ContentMarginRightOverride = 10,
                ContentMarginBottomOverride = 8,
            };
        });
        return bar;
    }

    private Control BuildHeroInfoCard(Control content, Func<Color> accentProvider)
    {
        var accent = accentProvider();
        var panel = new PanelContainer
        {
            MinSize = new Vector2(0, 64),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = accent.WithAlpha(0.16f),
                BorderColor = accent.WithAlpha(0.28f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 10,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 10,
            },
        };

        var line = new PanelContainer
        {
            MinSize = new Vector2(34, 3),
            MaxSize = new Vector2(34, 3),
        };
        var stack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
        };
        stack.AddChild(line);
        stack.AddChild(content);
        panel.AddChild(stack);
        _themeRefreshActions.Add(() =>
        {
            var liveAccent = accentProvider();
            panel.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = liveAccent.WithAlpha(0.16f),
                BorderColor = liveAccent.WithAlpha(0.28f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 10,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 10,
            };
            line.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = liveAccent.WithAlpha(0.96f),
            };
        });
        return panel;
    }

    private Control BuildPageTabs()
    {
        var row = new GridContainer
        {
            Columns = 3,
            HSeparationOverride = 10,
        };

        row.AddChild(CreatePageButton(CustomizationPage.Marines, "ccm-customization-tab-marines"));
        row.AddChild(CreatePageButton(CustomizationPage.Xeno, "ccm-customization-tab-xeno"));
        row.AddChild(CreatePageButton(CustomizationPage.Misc, "ccm-customization-tab-misc"));
        return row;
    }

    private Button CreatePageButton(CustomizationPage page, string textKey)
    {
        var button = new Button
        {
            Text = Loc.GetString(textKey),
            MinSize = new Vector2(0, 36),
            HorizontalExpand = true,
        };
        button.OnPressed += _ =>
        {
            _currentPage = page;
            UpdatePageState();
        };
        button.OnMouseEntered += _ => ApplyPageButtonStyle(page, hovered: true);
        button.OnMouseExited += _ => ApplyPageButtonStyle(page);
        button.OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyPageButtonStyle(page, pressed: true);
        };
        button.OnKeyBindUp += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyPageButtonStyle(page);
        };

        _pageButtons[page] = button;
        ApplyPageButtonStyle(page);
        return button;
    }

    private void UpdatePageState()
    {
        _xenoPage.Visible = _currentPage == CustomizationPage.Xeno;
        _marinesPage.Visible = _currentPage == CustomizationPage.Marines;
        _miscPage.Visible = _currentPage == CustomizationPage.Misc;

        foreach (var page in _pageButtons.Keys)
        {
            ApplyPageButtonStyle(page);
        }
    }

    private Control BuildXenoPage()
    {
        var stack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 12,
            HorizontalExpand = true,
        };

        stack.AddChild(BuildSectionBlock("ccm-customization-section-xeno", BuildXenoGallery()));
        return stack;
    }

    private Control BuildMarinesPage()
    {
        var stack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 12,
            HorizontalExpand = true,
        };

        stack.AddChild(BuildSectionBlock("ccm-customization-tab-marines", BuildMarineCustomization()));
        return stack;
    }

    private Control BuildMiscPage()
    {
        var stack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 12,
            HorizontalExpand = true,
        };

        var grid = new GridContainer
        {
            Columns = 2,
            HSeparationOverride = 12,
            VSeparationOverride = 12,
        };
        grid.AddChild(BuildGhostCard());
        grid.AddChild(BuildTagCard());
        grid.AddChild(BuildChatColorCard("ccm-customization-slot-ooc-color", _oocColorSelector, _oocColorPreviewLabel, "OOC"));
        grid.AddChild(BuildChatColorCard("ccm-customization-slot-looc-color", _loocColorSelector, _loocColorPreviewLabel, "LOOC"));

        stack.AddChild(BuildSectionBlock("ccm-customization-tab-misc", grid));
        return stack;
    }

    private Control BuildSectionBlock(string titleKey, Control body)
    {
        var panel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.24f),
                BorderColor = GetWindowAccent().WithAlpha(0.32f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 12,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 12,
            },
        };

        var stack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 10,
        };

        var titleRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 10,
        };
        var icon = new PanelContainer
        {
            MinSize = new Vector2(24, 24),
            MaxSize = new Vector2(24, 24),
        };
        titleRow.AddChild(icon);
        var titleLabel = new Label
        {
            Text = Loc.GetString(titleKey),
            FontColorOverride = GetWindowAccent(),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 16),
            VerticalAlignment = VAlignment.Center,
        };
        titleRow.AddChild(titleLabel);

        stack.AddChild(titleRow);
        stack.AddChild(body);
        panel.AddChild(stack);
        _themeRefreshActions.Add(() =>
        {
            panel.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.24f),
                BorderColor = GetWindowAccent().WithAlpha(0.32f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 12,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 12,
            };
            icon.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = GetWindowAccent().WithAlpha(0.14f),
                BorderColor = GetWindowAccent().WithAlpha(0.34f),
                BorderThickness = new Thickness(1),
            };
            titleLabel.FontColorOverride = GetWindowAccent();
        });
        return panel;
    }

    private Control BuildXenoGallery()
    {
        var grid = new GridContainer
        {
            Columns = 2,
            HSeparationOverride = 12,
            VSeparationOverride = 12,
        };

        grid.AddChild(BuildXenoSkinCard("xeno_defender", "ccm-customization-slot-defender"));
        grid.AddChild(BuildXenoSkinCard("xeno_drone", "ccm-customization-slot-drone"));
        grid.AddChild(BuildXenoSkinCard("xeno_queen", "ccm-customization-slot-queen"));
        grid.AddChild(BuildXenoSkinCard("xeno_runner", "ccm-customization-slot-runner"));
        grid.AddChild(BuildXenoSkinCard("xeno_sentinel", "ccm-customization-slot-sentinel"));
        return grid;
    }

    private Control BuildMarineCustomization()
    {
        var stack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 12,
        };

        var topCard = BuildCamouflageCard(
            "armor_variant",
            "ccm-customization-slot-armor-variant",
            Loc.GetString("ccm-customization-badge-marine"),
            BuildCamouflagePreview("armor_variant"));
        topCard.HorizontalExpand = true;
        stack.AddChild(topCard);

        var grid = new GridContainer
        {
            Columns = 2,
            HSeparationOverride = 12,
            VSeparationOverride = 12,
            HorizontalExpand = true,
        };

        grid.AddChild(BuildCamouflageCard(
            "armor_palette",
            "ccm-customization-slot-armor-palette",
            Loc.GetString("ccm-customization-badge-marine"),
            BuildCamouflagePreview("armor_palette")));
        grid.AddChild(BuildCamouflageCard(
            "weapon_spray",
            "ccm-customization-slot-weapon-spray",
            Loc.GetString("ccm-customization-badge-weapon"),
            BuildCamouflagePreview("weapon_spray")));

        stack.AddChild(grid);
        return stack;
    }

    private Control BuildXenoSkinCard(string slotId, string titleKey)
    {
        Func<Color> accentProvider = () => GetSlotAccent(slotId);
        var accent = accentProvider();
        var selector = MakeSelector(slotId, 0);
        selector.OnItemSelected += _ => UpdateXenoPreviewSelection(slotId);
        selector.HorizontalExpand = true;

        var card = BuildDecoratedCard(accentProvider, 196,
            BuildCardHeader(Loc.GetString(titleKey), Loc.GetString("ccm-customization-badge-xeno"), accentProvider),
            BuildXenoCurrentPreview(slotId, accent),
            selector);
        return WrapWithAvailabilityOverlay(card, () => !(_status?.CustomizationUnlocked ?? false));
    }

    private Control BuildXenoCurrentPreview(string slotId, Color accent)
    {
        var frame = new PanelContainer
        {
            MinSize = new Vector2(0, 104),
            HorizontalExpand = true,
            RectClipContent = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.22f),
                BorderColor = accent.WithAlpha(0.28f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 6,
                ContentMarginTopOverride = 6,
                ContentMarginRightOverride = 6,
                ContentMarginBottomOverride = 6,
            },
        };

        var texture = new TextureRect
        {
            Texture = GetXenoPreviewTexture(DefaultXenoPreviewPaths[slotId]),
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        _xenoPreviewTextures[slotId] = texture;
        frame.AddChild(texture);
        UpdateXenoPreviewSelection(slotId);
        _themeRefreshActions.Add(() =>
        {
            var liveAccent = GetSlotAccent(slotId);
            frame.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.22f),
                BorderColor = liveAccent.WithAlpha(0.28f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 6,
                ContentMarginTopOverride = 6,
                ContentMarginRightOverride = 6,
                ContentMarginBottomOverride = 6,
            };
        });
        return frame;
    }

    private Control BuildArmorCard()
    {
        Func<Color> accentProvider = () => GetSlotAccent("armor_paint");
        var selector = MakeSelector("armor_paint", 0);
        selector.HorizontalExpand = true;
        selector.OnItemSelected += _ => UpdateDynamicPreviews();

        return BuildDecoratedCard(accentProvider, 228,
            BuildCardHeader(Loc.GetString("ccm-customization-slot-armor"), Loc.GetString("ccm-customization-badge-marine"), accentProvider),
            BuildArmorPreview(),
            selector);
    }

    private Control BuildCamouflageCard(string slotId, string titleKey, string badgeText, Control preview)
    {
        Func<Color> accentProvider = () => GetSlotAccent(slotId);
        var accent = accentProvider();
        var selector = MakeSelector(slotId, 0);
        selector.HorizontalExpand = true;
        selector.OnItemSelected += _ => UpdateDynamicPreviews();
        var hintKey = slotId == "armor_variant"
            ? "ccm-customization-armor-variant-hint"
            : "ccm-customization-camo-hint";
        var hintMaxWidth = slotId == "armor_variant" ? 820f : 420f;

        var card = BuildDecoratedCard(accentProvider, 228,
            BuildCardHeader(Loc.GetString(titleKey), badgeText, accentProvider),
            MakeWrappedText(Loc.GetString(hintKey), Color.FromHex("#B4BFCA"), 11, hintMaxWidth),
            preview,
            selector);

        return slotId is "armor_palette" or "armor_variant" or "weapon_spray"
            ? card
            : WrapWithAvailabilityOverlay(card, () => !(_status?.CustomizationUnlocked ?? false));
    }

    private Control BuildGhostCard()
    {
        Func<Color> accentProvider = () => GetSlotAccent("ghost");
        var accent = accentProvider();
        var selector = MakeSelector("ghost", 0);
        selector.HorizontalExpand = true;
        selector.OnItemSelected += _ => UpdateDynamicPreviews();

        var card = BuildDecoratedCard(accentProvider, 200,
            BuildCardHeader(Loc.GetString("ccm-customization-slot-ghost"), Loc.GetString("ccm-customization-badge-misc"), accentProvider),
            BuildGhostPreview(),
            selector);
        return WrapWithAvailabilityOverlay(card, () => (_status?.Tier ?? CCMSponsorshipTier.None) < CCMSponsorshipTier.SponsorII);
    }

    private Control BuildTagCard()
    {
        Func<Color> accentProvider = () => GetSlotAccent("ooc");
        var accent = accentProvider();

        var customTagPanel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.26f),
                BorderColor = accent.WithAlpha(0.2f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 8,
                ContentMarginTopOverride = 8,
                ContentMarginRightOverride = 8,
                ContentMarginBottomOverride = 8,
            },
        };

        var customTagStack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 8,
        };
        customTagStack.AddChild(new Label
        {
            Text = Loc.GetString("ccm-customization-slot-custom-tag"),
            FontColorOverride = Color.White,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13),
        });
        customTagStack.AddChild(_customTagEdit);
        customTagPanel.AddChild(customTagStack);

        var customTagContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };
        customTagContainer.AddChild(customTagPanel);

        var presetTagContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };
        presetTagContainer.AddChild(_oocTagSelector);

        var customTagGuard = WrapWithAvailabilityOverlay(
            customTagContainer,
            () =>
            {
                var tier = _status?.Tier ?? CCMSponsorshipTier.None;
                return tier >= CCMSponsorshipTier.SponsorII && tier < CCMSponsorshipTier.SponsorIII;
            });

        var tagControls = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 8,
            HorizontalExpand = true,
        };
        tagControls.AddChild(presetTagContainer);
        tagControls.AddChild(customTagGuard);

        var tagControlsGuard = WrapWithAvailabilityOverlay(
            tagControls,
            () => (_status?.Tier ?? CCMSponsorshipTier.None) < CCMSponsorshipTier.SponsorII);

        var card = BuildDecoratedCard(accentProvider, 332,
            BuildCardHeader(Loc.GetString("ccm-customization-slot-ooc-tag"), "OOC", accentProvider),
            MakeWrappedText(Loc.GetString("ccm-customization-tag-hint"), Color.FromHex("#B4BFCA"), 11, 420f),
            tagControlsGuard,
            BuildPreviewBubble(_tagPreviewLabel, () => GetThemeAccent(0.18f).WithAlpha(0.30f), minHeight: 44));

        _themeRefreshActions.Add(() =>
        {
            var liveAccent = accentProvider();
            customTagPanel.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.26f),
                BorderColor = liveAccent.WithAlpha(0.2f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 8,
                ContentMarginTopOverride = 8,
                ContentMarginRightOverride = 8,
                ContentMarginBottomOverride = 8,
            };
        });

        return card;
    }

    private Control BuildChatColorCard(string titleKey, CCMOptionButton selector, Label previewLabel, string previewChannel)
    {
        var card = BuildDecoratedCard(() => GetThemeAccent(0.18f), 142,
            new Label
            {
                Text = Loc.GetString(titleKey),
                FontColorOverride = Color.White,
                FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13),
            },
            selector,
            BuildPreviewBubble(previewLabel, () => GetThemeAccent(0.18f).WithAlpha(0.26f), previewChannel, minHeight: 38, verticalExpand: false));
        return WrapWithAvailabilityOverlay(card, () => !(_status?.CustomizationUnlocked ?? false));
    }

    private Control BuildPreviewBubble(Label content, Func<Color> borderColorProvider, string? prefix = null, float minHeight = 82, bool verticalExpand = true)
    {
        var borderColor = borderColorProvider();
        var bubble = new PanelContainer
        {
            MinSize = new Vector2(0, minHeight),
            VerticalExpand = verticalExpand,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.30f),
                BorderColor = borderColor,
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 10,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 10,
            },
        };

        if (string.IsNullOrWhiteSpace(prefix))
        {
            _themeRefreshActions.Add(() =>
            {
                bubble.PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = Color.Black.WithAlpha(0.30f),
                    BorderColor = borderColorProvider(),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 12,
                    ContentMarginTopOverride = 10,
                    ContentMarginRightOverride = 12,
                    ContentMarginBottomOverride = 10,
                };
            });
            bubble.AddChild(content);
            return bubble;
        }

        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
        };
        var prefixLabel = new Label
        {
            Text = prefix,
            FontColorOverride = Color.FromHex("#9DB6C5"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 11),
            VerticalAlignment = VAlignment.Center,
        };
        row.AddChild(prefixLabel);
        row.AddChild(content);
        bubble.AddChild(row);
        _themeRefreshActions.Add(() =>
        {
            bubble.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.30f),
                BorderColor = borderColorProvider(),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 10,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 10,
            };
            prefixLabel.FontColorOverride = GetThemeAccent(0.30f);
        });
        return bubble;
    }

    private Control BuildGhostPreview()
    {
        return BuildGhostPreviewVariant("current", Color.White.WithAlpha(0.82f));
    }

    private Control BuildArmorPreview()
    {
        var preview = new PanelContainer
        {
            MinSize = new Vector2(0, 94),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.30f),
                BorderColor = GetSlotAccent("armor_paint").WithAlpha(0.32f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 8,
                ContentMarginTopOverride = 8,
                ContentMarginRightOverride = 8,
                ContentMarginBottomOverride = 8,
            },
        };

        var texture = new TextureRect
        {
            Stretch = TextureRect.StretchMode.KeepCentered,
            TextureScale = new Vector2(2.6f, 2.6f),
            HorizontalExpand = true,
            VerticalExpand = true,
            Texture = _resourceCache.GetTexture("/Textures/_RMC14/Objects/Clothing/Accessory/PVE/Marine/paint/skull.rsi/icon.png"),
        };
        _dynamicPreviewTextures["armor_paint"] = texture;
        preview.AddChild(texture);
        _themeRefreshActions.Add(() =>
        {
            preview.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.30f),
                BorderColor = GetSlotAccent("armor_paint").WithAlpha(0.32f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 8,
                ContentMarginTopOverride = 8,
                ContentMarginRightOverride = 8,
                ContentMarginBottomOverride = 8,
            };
        });
        return preview;
    }

    private Control BuildCamouflagePreview(string slotId)
    {
        var preview = new PanelContainer
        {
            MinSize = new Vector2(0, 68),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.30f),
                BorderColor = GetSlotAccent(slotId).WithAlpha(0.32f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 10,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 10,
            },
        };

        var label = new Label
        {
            FontColorOverride = GetSlotAccent(slotId),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 14),
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        _camoPreviewLabels[slotId] = label;
        preview.AddChild(label);
        _themeRefreshActions.Add(() =>
        {
            preview.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.30f),
                BorderColor = GetSlotAccent(slotId).WithAlpha(0.32f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 10,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 10,
            };
            label.FontColorOverride = GetSlotAccent(slotId);
        });
        return preview;
    }

    private Control BuildCardHeader(string title, string badgeText, Func<Color> accentProvider)
    {
        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
        };
        row.AddChild(new Label
        {
            Text = title,
            FontColorOverride = Color.White,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 14),
            HorizontalExpand = true,
        });
        var badgeLabel = new Label
        {
            Text = badgeText,
            FontColorOverride = accentProvider(),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 11),
            HorizontalAlignment = HAlignment.Right,
        };
        row.AddChild(badgeLabel);
        _themeRefreshActions.Add(() => badgeLabel.FontColorOverride = accentProvider());
        return row;
    }

    private Control BuildDecoratedCard(Func<Color> accentProvider, float minHeight, params Control[] body)
    {
        var accent = accentProvider();
        var panel = new PanelContainer
        {
            MinSize = new Vector2(0, minHeight),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.25f),
                BorderColor = accent.WithAlpha(0.24f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 11,
                ContentMarginTopOverride = 11,
                ContentMarginRightOverride = 11,
                ContentMarginBottomOverride = 11,
            },
        };

        var stack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 10,
        };
        var accentLine = new PanelContainer
        {
            MinSize = new Vector2(0, 4),
            HorizontalExpand = true,
        };
        stack.AddChild(accentLine);

        foreach (var child in body)
        {
            stack.AddChild(child);
        }

        panel.AddChild(stack);
        _themeRefreshActions.Add(() =>
        {
            var liveAccent = accentProvider();
            panel.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.25f),
                BorderColor = liveAccent.WithAlpha(0.24f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 11,
                ContentMarginTopOverride = 11,
                ContentMarginRightOverride = 11,
                ContentMarginBottomOverride = 11,
            };
            accentLine.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = liveAccent.WithAlpha(0.90f),
            };
        });
        return panel;
    }

    private Control BuildGhostPreviewVariant(string key, Color color)
    {
        var panel = new PanelContainer
        {
            MinSize = new Vector2(0, 88),
            HorizontalExpand = true,
            RectClipContent = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = color.WithAlpha(0.16f),
                BorderColor = color.WithAlpha(0.24f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 8,
                ContentMarginTopOverride = 8,
                ContentMarginRightOverride = 8,
                ContentMarginBottomOverride = 8,
            },
        };

        var layout = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        layout.AddChild(new Control
        {
            HorizontalExpand = true,
            SizeFlagsStretchRatio = 0.92f,
        });

        var texture = new TextureRect
        {
            Texture = _resourceCache.GetTexture("/Textures/Mobs/Ghosts/ghost_human.rsi/icon.png"),
            TextureScale = new Vector2(2.8f, 2.8f),
            ModulateSelfOverride = color,
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Center,
        };
        _dynamicPreviewTextures[$"ghost:{key}"] = texture;
        layout.AddChild(texture);
        layout.AddChild(new Control
        {
            HorizontalExpand = true,
            SizeFlagsStretchRatio = 1.08f,
        });
        panel.AddChild(layout);
        _themeRefreshActions.Add(() =>
        {
            panel.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = color.WithAlpha(0.16f),
                BorderColor = color.WithAlpha(0.24f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 8,
                ContentMarginTopOverride = 8,
                ContentMarginRightOverride = 8,
                ContentMarginBottomOverride = 8,
            };
        });
        return panel;
    }

    private Control WrapWithAvailabilityOverlay(Control target, Func<bool> predicate, Control? wholeCard = null)
    {
        var overlay = new PanelContainer
        {
            Visible = false,
            MouseFilter = MouseFilterMode.Ignore,
        };
        var label = new Label
        {
            Text = Loc.GetString("ccm-customization-overlay-locked"),
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            HorizontalExpand = true,
            VerticalExpand = true,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 12),
        };
        overlay.AddChild(label);

        var content = wholeCard ?? target;
        var container = new CCMAvailabilityOverlayContainer(content, overlay)
        {
            HorizontalExpand = true,
        };

        _themeRefreshActions.Add(() =>
        {
            var accent = GetWindowAccent();
            overlay.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = BlendTowards(accent, Color.Black, 0.80f).WithAlpha(0.88f),
                BorderColor = accent.WithAlpha(0.70f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 12,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 12,
            };
            label.FontColorOverride = GetThemeAccent(0.46f);
        });

        _availabilityOverlays.Add((overlay, predicate));
        return container;
    }

    private CCMOptionButton MakeSelector(string slotId, float width)
    {
        var selector = new CCMOptionButton
        {
            MinSize = new Vector2(width, 36),
            HorizontalExpand = width <= 0,
        };

        var options = SlotOptions[slotId];
        for (var i = 0; i < options.Length; i++)
        {
            selector.AddItem(Loc.GetString(options[i].NameKey), i);

            if (TryGetOptionTextColor(slotId, options[i].Id, out var color))
                selector.SetItemTextColor(i, color);
        }

        selector.SelectId(0);
        selector.OnItemSelected += args =>
        {
            args.Button.SelectId(args.Id);
            UpdateDynamicPreviews();
            UpdateSaveState();
        };
        _selectors[slotId] = selector;
        return selector;
    }

    private CCMOptionButton MakeOocTagSelector()
    {
        var selector = new CCMOptionButton
        {
            MinSize = new Vector2(0, 36),
            HorizontalExpand = true,
        };

        for (var i = 0; i < OocTagOptions.Length; i++)
        {
            selector.AddItem(Loc.GetString(OocTagOptions[i].NameKey), i);
        }

        selector.SelectId(0);
        selector.OnItemSelected += args =>
        {
            args.Button.SelectId(args.Id);
            UpdateOocTagControls();
            UpdateTagPreview();
            UpdateSaveState();
        };
        return selector;
    }

    private CCMOptionButton MakeChatColorSelector(bool looc)
    {
        var selector = new CCMOptionButton
        {
            MinSize = new Vector2(0, 36),
            HorizontalExpand = true,
        };

        for (var i = 0; i < ChatColorOptions.Length; i++)
        {
            selector.AddItem(Loc.GetString(ChatColorOptions[i].NameKey), i);
            if (TryGetChatColorOption(ChatColorOptions[i].Id, out var color))
                selector.SetItemTextColor(i, color);
        }

        selector.SelectId(0);
        selector.OnItemSelected += args =>
        {
            args.Button.SelectId(args.Id);
            UpdateChatColorPreview(looc);
            UpdateSaveState();
        };
        return selector;
    }

    private LineEdit MakeCustomTagEdit()
    {
        var edit = new LineEdit
        {
            MinSize = new Vector2(0, 36),
            HorizontalExpand = true,
            StyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.28f),
                BorderColor = GetWindowAccent().WithAlpha(0.50f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 11,
                ContentMarginTopOverride = 7,
                ContentMarginRightOverride = 11,
                ContentMarginBottomOverride = 7,
            },
        };

        edit.PlaceHolder = Loc.GetString("ccm-customization-tag-placeholder");
        edit.IsValid = text => text.Length <= CCMCustomizationConstants.CustomOocTagMaxLength;
        edit.OnTextChanged += _ =>
        {
            UpdateOocTagControls();
            UpdateTagPreview();
            UpdateSaveState();
        };
        _themeRefreshActions.Add(() =>
        {
            edit.StyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.28f),
                BorderColor = GetWindowAccent().WithAlpha(0.50f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 11,
                ContentMarginTopOverride = 7,
                ContentMarginRightOverride = 11,
                ContentMarginBottomOverride = 7,
            };
        });
        return edit;
    }

    private CCMCustomizationSnapshot BuildSnapshot()
    {
        var selections = new List<CCMCustomizationSelectionData>();
        foreach (var (slotId, selector) in _selectors)
        {
            var selected = Math.Clamp(selector.SelectedId, 0, SlotOptions[slotId].Length - 1);
            selections.Add(new CCMCustomizationSelectionData(slotId, SlotOptions[slotId][selected].Id));
        }

        var selectedTagId = OocTagOptions[Math.Clamp(_oocTagSelector.SelectedId, 0, OocTagOptions.Length - 1)].Id;
        var selectedOocColorId = ChatColorOptions[Math.Clamp(_oocColorSelector.SelectedId, 0, ChatColorOptions.Length - 1)].Id;
        var selectedLoocColorId = ChatColorOptions[Math.Clamp(_loocColorSelector.SelectedId, 0, ChatColorOptions.Length - 1)].Id;
        return new CCMCustomizationSnapshot(
            selections.ToArray(),
            selectedTagId,
            _customTagEdit.Text,
            selectedOocColorId,
            selectedLoocColorId);
    }

    private static string NormalizeValue(string slotId, string valueId)
    {
        return string.IsNullOrWhiteSpace(valueId) ? SlotOptions[slotId][0].Id : valueId;
    }

    private RichTextLabel MakeWrappedText(string text, Color color, int size, float? maxWidth = null)
    {
        var label = new RichTextLabel
        {
            HorizontalExpand = true,
            VerticalExpand = false,
            HorizontalAlignment = HAlignment.Left,
        };

        if (maxWidth.HasValue)
            label.MaxWidth = maxWidth.Value;

        label.SetMessage(FormattedMessage.FromMarkupOrThrow($"[color={color.ToHex()}]{FormattedMessage.EscapeText(text)}[/color]"));
        return label;
    }

    private void UpdateStatusText()
    {
        var tier = _status?.Tier ?? CCMSponsorshipTier.None;
        _statusLabel.Text = Loc.GetString("ccm-sponsorship-current-tier",
            ("tier", Loc.GetString(GetTierTitleKey(tier))));
        _statusHintLabel.Text = _status?.CustomizationUnlocked ?? false
            ? Loc.GetString("ccm-customization-status-enabled")
            : Loc.GetString("ccm-customization-status-locked");
    }

    private void UpdateAvailability()
    {
        foreach (var (overlay, visible) in _availabilityOverlays)
        {
            var shown = visible();
            overlay.Visible = shown;
            overlay.MouseFilter = shown ? MouseFilterMode.Stop : MouseFilterMode.Ignore;
        }

        UpdateOocTagControls();
        UpdateChatColorPreview(false);
        UpdateChatColorPreview(true);
    }

    private void UpdateOocTagControls()
    {
        var tier = _status?.Tier ?? CCMSponsorshipTier.None;
        var selectedTagId = OocTagOptions[Math.Clamp(_oocTagSelector.SelectedId, 0, OocTagOptions.Length - 1)].Id;
        var canUsePresetTag = tier >= CCMSponsorshipTier.SponsorI;
        var canUseCustomTag = tier >= CCMSponsorshipTier.SponsorIII;

        if (!canUsePresetTag && selectedTagId != CCMOocTags.None)
        {
            _oocTagSelector.SelectId(0);
            selectedTagId = CCMOocTags.None;
        }

        var customSelected = selectedTagId == CCMOocTags.Custom;

        if (customSelected && !canUseCustomTag)
        {
            _oocTagSelector.SelectId(0);
            customSelected = false;
        }

        _customTagEdit.Editable = customSelected && canUseCustomTag;
        _customTagEdit.ModulateSelfOverride = (!customSelected || canUseCustomTag)
            ? null
            : Color.FromHex("#7A838D");
    }

    private void UpdateTagPreview()
    {
        var selectedTag = OocTagOptions[Math.Clamp(_oocTagSelector.SelectedId, 0, OocTagOptions.Length - 1)].Id;
        var tagText = selectedTag switch
        {
            CCMOocTags.None => string.Empty,
            CCMOocTags.Custom => _customTagEdit.Text.Trim(),
            _ => Loc.GetString(OocTagOptions.First(option => option.Id == selectedTag).NameKey),
        };

        _tagPreviewLabel.Text = string.IsNullOrWhiteSpace(tagText)
            ? "localhost"
            : $"[{tagText}] localhost";

        UpdateChatColorPreview(false);
        UpdateChatColorPreview(true);
    }

    private void UpdateChatColorPreview(bool looc)
    {
        var selector = looc ? _loocColorSelector : _oocColorSelector;
        var label = looc ? _loocColorPreviewLabel : _oocColorPreviewLabel;
        var colorId = ChatColorOptions[Math.Clamp(selector.SelectedId, 0, ChatColorOptions.Length - 1)].Id;
        var colorHex = colorId != CCMChatColorPresets.Default
            ? CCMChatColorPresets.GetHex(colorId)
            : looc
                ? _status?.LoocColorHex ?? string.Empty
                : _status?.OocColorHex ?? string.Empty;

        var baseTagPreview = _tagPreviewLabel.Text ?? string.Empty;
        label.Text = looc
            ? "localhost: Local chatter."
            : baseTagPreview.Length > 0
                ? $"{baseTagPreview}: Lobby chatter."
                : "localhost: Lobby chatter.";
        label.FontColorOverride = colorHex.Length > 0 ? Color.FromHex(colorHex) : Color.White;

        if (!looc)
            _tagPreviewLabel.FontColorOverride = label.FontColorOverride;
    }

    private void UpdateDynamicPreviews()
    {
        UpdateGhostPreview();
        UpdateCamouflagePreview("armor_variant");
        UpdateCamouflagePreview("armor_palette");
        UpdateCamouflagePreview("weapon_spray");
    }

    private void UpdateGhostPreview()
    {
        if (!_selectors.TryGetValue("ghost", out var selector))
            return;

        var option = SlotOptions["ghost"][Math.Clamp(selector.SelectedId, 0, SlotOptions["ghost"].Length - 1)];
        var selected = option.Id;

        if (_dynamicPreviewTextures.TryGetValue("ghost:current", out var currentTexture))
        {
            currentTexture.Texture = GetGhostPreviewTexture(option);
            currentTexture.TextureScale = GetGhostPreviewScale(option);
            currentTexture.ModulateSelfOverride = TryGetGhostSkinColor(selected, out var color)
                ? color
                : Color.White.WithAlpha(0.90f);
        }
    }

    private void UpdateArmorPaintPreview()
    {
        if (!_selectors.TryGetValue("armor_paint", out var selector) ||
            !_dynamicPreviewTextures.TryGetValue("armor_paint", out var texture))
        {
            return;
        }

        var selected = SlotOptions["armor_paint"][Math.Clamp(selector.SelectedId, 0, SlotOptions["armor_paint"].Length - 1)].Id;
        var texturePath = selected switch
        {
            "heart" => "/Textures/_RMC14/Objects/Clothing/Accessory/PVE/Marine/paint/heart.rsi/icon.png",
            "medic" => "/Textures/_RMC14/Objects/Clothing/Accessory/PVE/Marine/paint/medic.rsi/icon.png",
            "un" => "/Textures/_RMC14/Objects/Clothing/Accessory/PVE/Marine/paint/un.rsi/icon.png",
            "target" => "/Textures/_RMC14/Objects/Clothing/Accessory/PVE/Marine/paint/target.rsi/icon.png",
            "smiley" => "/Textures/_RMC14/Objects/Clothing/Accessory/PVE/Marine/paint/smiley.rsi/icon.png",
            "neutral" => "/Textures/_RMC14/Objects/Clothing/Accessory/PVE/Marine/paint/neutral.rsi/icon.png",
            "cross" => "/Textures/_RMC14/Objects/Clothing/Accessory/PVE/Marine/paint/cross.rsi/icon.png",
            "inscription" => "/Textures/_RMC14/Objects/Clothing/Accessory/PVE/Marine/paint/inscription.rsi/icon.png",
            "mixtape" => "/Textures/_RMC14/Objects/Clothing/Accessory/PVE/Marine/paint/mixtape.rsi/icon.png",
            _ => "/Textures/_RMC14/Objects/Clothing/Accessory/PVE/Marine/paint/skull.rsi/icon.png",
        };

        texture.Texture = _resourceCache.GetTexture(texturePath);
    }

    private void UpdateCamouflagePreview(string slotId)
    {
        if (!_selectors.TryGetValue(slotId, out var selector) ||
            !_camoPreviewLabels.TryGetValue(slotId, out var label))
        {
            return;
        }

        var option = SlotOptions[slotId][Math.Clamp(selector.SelectedId, 0, SlotOptions[slotId].Length - 1)];
        var selected = option.Id;
        label.Text = Loc.GetString(option.NameKey);

        label.FontColorOverride = selected switch
        {
            CCMCustomizationArmorVariantIds.None => Color.FromHex("#A0AFBA"),
            CCMCustomizationArmorVariantIds.Padded => Color.FromHex("#7DBF73"),
            CCMCustomizationArmorVariantIds.Padless => Color.FromHex("#77D7FF"),
            CCMCustomizationArmorVariantIds.Ridged => Color.FromHex("#FFB36F"),
            CCMCustomizationArmorVariantIds.Carrier => Color.FromHex("#F5C46F"),
            CCMCustomizationArmorVariantIds.Skull => Color.FromHex("#FF9D6B"),
            CCMCustomizationArmorVariantIds.Smooth => Color.FromHex("#D88BFF"),
            CCMCustomizationCamouflageIds.Desert => Color.FromHex("#C79C63"),
            CCMCustomizationCamouflageIds.Snow => Color.FromHex("#D9E4EC"),
            CCMCustomizationCamouflageIds.Classic => Color.FromHex("#5F87A6"),
            CCMCustomizationCamouflageIds.Urban => Color.FromHex("#88919E"),
            _ => Color.FromHex("#7DBF73"),
        };
    }

    private void ApplyPageButtonStyle(CustomizationPage page, bool hovered = false, bool pressed = false)
    {
        if (!_pageButtons.TryGetValue(page, out var button))
            return;

        var accent = page switch
        {
            CustomizationPage.Xeno => GetThemeAccent(0.22f),
            CustomizationPage.Marines => GetThemeAccent(0.10f),
            _ => GetThemeAccent(0.16f),
        };
        var active = _currentPage == page;
        var background = active
            ? accent.WithAlpha(pressed ? 0.30f : hovered ? 0.24f : 0.20f)
            : pressed
                ? accent.WithAlpha(0.16f)
                : hovered
                    ? Color.Black.WithAlpha(0.24f)
                    : Color.Black.WithAlpha(0.18f);

        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = background,
            BorderColor = active ? accent.WithAlpha(0.85f) : accent.WithAlpha(0.42f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 10,
            ContentMarginTopOverride = 5,
            ContentMarginRightOverride = 10,
            ContentMarginBottomOverride = 5,
        };
        button.Label.FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 12);
        button.Label.FontColorOverride = active ? accent : Color.White;
    }

    private void UpdateAllXenoPreviewSelections()
    {
        foreach (var slotId in DefaultXenoPreviewPaths.Keys)
        {
            UpdateXenoPreviewSelection(slotId);
        }
    }

    private void UpdateXenoPreviewSelection(string slotId)
    {
        if (!_selectors.TryGetValue(slotId, out var selector))
            return;

        if (!_xenoPreviewTextures.TryGetValue(slotId, out var texture))
            return;

        var selectedOption = SlotOptions[slotId][Math.Clamp(selector.SelectedId, 0, SlotOptions[slotId].Length - 1)].Id;
        var option = SlotOptions[slotId].FirstOrDefault(opt => opt.Id == selectedOption);
        var texturePath = selectedOption == "default" || string.IsNullOrWhiteSpace(option.PreviewTexturePath)
            ? DefaultXenoPreviewPaths[slotId]
            : option.PreviewTexturePath;
        texture.Texture = GetXenoPreviewTexture(texturePath);
        texture.TextureScale = GetXenoPreviewScale(texture.Texture);
    }

    private Texture GetGhostPreviewTexture(CustomOption option)
    {
        var texturePath = string.IsNullOrWhiteSpace(option.PreviewTexturePath)
            ? "/Textures/Mobs/Ghosts/ghost_human.rsi/icon.png"
            : option.PreviewTexturePath;
        var texture = _resourceCache.GetTexture(texturePath);

        if (GhostPreviewFrameSizes.TryGetValue(option.Id, out var frameSize))
            return new AtlasTexture(texture, UIBox2.FromDimensions(0, 0, frameSize, frameSize));

        return texture;
    }

    private static Vector2 GetGhostPreviewScale(CustomOption option)
    {
        return GhostPreviewFrameSizes.TryGetValue(option.Id, out var frameSize)
            ? new Vector2(82f / frameSize, 82f / frameSize)
            : new Vector2(2.8f, 2.8f);
    }

    private Texture GetXenoPreviewTexture(string texturePath)
    {
        var texture = _resourceCache.GetTexture(texturePath);
        if (texturePath.EndsWith("/alive.png", StringComparison.OrdinalIgnoreCase))
        {
            var frameWidth = Math.Min(texture.Width, texture.Height);
            return new AtlasTexture(texture, UIBox2.FromDimensions(0, 0, frameWidth, texture.Height));
        }

        return texture;
    }

    private static Vector2 GetXenoPreviewScale(Texture texture)
    {
        const float queenReferenceHeight = 80f;
        const float queenReferenceScale = 1.85f;

        var height = Math.Max(1, texture.Height);
        var scale = queenReferenceScale * (queenReferenceHeight / height);
        return new Vector2(scale, scale);
    }

    private void ApplySaveButtonStyle(bool hovered = false, bool pressed = false)
    {
        var enabled = !_saveButton.Disabled;
        var normalBackground = StyleNano.ButtonColorContext;
        var hoverBackground = StyleNano.ButtonColorContextHover;
        var pressedBackground = StyleNano.ButtonColorContextPressed;
        var disabledBackground = StyleNano.ButtonColorContextDisabled;
        var borderColor = StyleNano.UiButtonBorder;
        var disabledTextColor = Color.FromHex("#E7F4FF");
        var activeTextColor = Color.FromHex("#F2FAFF");
        _saveButton.ModulateSelfOverride = Color.White;
        _saveButton.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = !enabled
                ? disabledBackground
                : pressed
                    ? pressedBackground
                    : hovered
                        ? hoverBackground
                        : normalBackground,
            BorderColor = !enabled
                ? borderColor
                : pressed
                    ? borderColor
                    : borderColor,
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 10,
            ContentMarginTopOverride = 3,
            ContentMarginRightOverride = 10,
            ContentMarginBottomOverride = 3,
        };
        _saveButton.Label.FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 12);
        _saveButton.Label.FontColorOverride = !enabled
            ? disabledTextColor
            : pressed
                ? Color.Black
                : activeTextColor;
        _saveButton.Label.FontColorShadowOverride = !enabled || !pressed
            ? Color.Black.WithAlpha(0.72f)
            : null;
    }

    private void UpdateSaveState()
    {
        if (_suppressAutoSave)
            return;

        var pendingChanges = _savedSnapshot == null || !SnapshotsEqual(BuildSnapshot(), _savedSnapshot);
        _saveButton.Disabled = !pendingChanges;
        _saveStateLabel.Text = pendingChanges
            ? "Unsaved changes"
            : "No changes";
        _saveStateLabel.FontColorOverride = pendingChanges
            ? GetWindowAccent()
            : Color.FromHex("#8FA2B5");
        ApplySaveButtonStyle();
    }

    private static bool SnapshotsEqual(CCMCustomizationSnapshot left, CCMCustomizationSnapshot right)
    {
        if (left.SelectedOocTagId != right.SelectedOocTagId ||
            left.CustomOocTagText != right.CustomOocTagText ||
            left.SelectedOocColorId != right.SelectedOocColorId ||
            left.SelectedLoocColorId != right.SelectedLoocColorId)
        {
            return false;
        }

        if (left.Selections.Length != right.Selections.Length)
            return false;

        var leftSelections = left.Selections.OrderBy(s => s.SlotId).ToArray();
        var rightSelections = right.Selections.OrderBy(s => s.SlotId).ToArray();
        for (var i = 0; i < leftSelections.Length; i++)
        {
            if (leftSelections[i].SlotId != rightSelections[i].SlotId ||
                leftSelections[i].ValueId != rightSelections[i].ValueId)
            {
                return false;
            }
        }

        return true;
    }

    private void ApplyWindowTheme()
    {
        var theme = StyleNano.GetConfiguredTheme(_config);
        var headerColor = theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#171D24").WithAlpha(0.995f),
            _ => Color.FromHex("#041105").WithAlpha(0.995f),
        };
        var bodyColor = theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#1C232C").WithAlpha(0.995f),
            _ => Color.FromHex("#061507").WithAlpha(0.995f),
        };
        var borderColor = theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#7B8898").WithAlpha(0.86f),
            _ => StyleNano.LobbyMenuButtonBase.WithAlpha(0.82f),
        };

        HeaderPanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = headerColor,
            BorderColor = borderColor,
            BorderThickness = new Thickness(1, 1, 1, 0),
        };

        BodyPanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = bodyColor,
            BorderColor = borderColor,
            BorderThickness = new Thickness(1, 0, 1, 1),
        };
    }

    private void ApplyThemeRefreshActions()
    {
        foreach (var action in _themeRefreshActions)
        {
            action();
        }
    }

    private string GetTierTitleKey(CCMSponsorshipTier tier)
    {
        return tier switch
        {
            CCMSponsorshipTier.SponsorI => "ccm-sponsorship-tier-1-title",
            CCMSponsorshipTier.SponsorII => "ccm-sponsorship-tier-2-title",
            CCMSponsorshipTier.SponsorIII => "ccm-sponsorship-tier-3-title",
            _ => "ccm-sponsorship-tier-none-title",
        };
    }

    private Color GetSlotAccent(string slotId)
    {
        return slotId switch
        {
            "xeno_defender" => GetThemeAccent(0.14f),
            "xeno_drone" => GetThemeAccent(0.18f),
            "xeno_queen" => GetThemeAccent(0.28f),
            "xeno_runner" => GetThemeAccent(0.22f),
            "xeno_sentinel" => GetThemeAccent(0.24f),
            "ghost" => GetThemeAccent(0.16f),
            "weapon_spray" => GetThemeAccent(0.14f),
            "armor_palette" => GetThemeAccent(0.24f),
            "armor_variant" => GetThemeAccent(0.12f),
            "armor_paint" => GetThemeAccent(0.20f),
            "ooc" => GetThemeAccent(0.18f),
            _ => GetWindowAccent(),
        };
    }

    private Color GetThemeAccent(float amount)
    {
        return BlendTowards(GetWindowAccent(), Color.White, amount);
    }

    private Color GetWindowAccent()
    {
        return StyleNano.LobbyMenuButtonBase;
    }

    private static Color BlendTowards(Color source, Color target, float factor)
    {
        factor = Math.Clamp(factor, 0f, 1f);
        return new Color(
            source.R + (target.R - source.R) * factor,
            source.G + (target.G - source.G) * factor,
            source.B + (target.B - source.B) * factor,
            source.A + (target.A - source.A) * factor);
    }

    private static bool TryGetOptionTextColor(string slotId, string optionId, out Color color)
    {
        if (slotId is "armor_palette" or "weapon_spray")
            return TryGetCamouflageColor(optionId, out color);

        if (slotId == "ghost")
            return TryGetGhostSkinColor(optionId, out color);

        color = default;
        return false;
    }

    private static bool TryGetGhostSkinColor(string optionId, out Color color)
    {
        color = optionId switch
        {
            "holo_green" => Color.FromHex("#7CFF9A"),
            "holo_blue" => Color.FromHex("#77E3FF"),
            "holo_violet" => Color.FromHex("#C695FF"),
            "holo_amber" => Color.FromHex("#FFC76A"),
            "holo_crimson" => Color.FromHex("#FF7C9C"),
            "holo_teal" => Color.FromHex("#6FF2E8"),
            _ => default,
        };

        return optionId is
            "holo_green" or
            "holo_blue" or
            "holo_violet" or
            "holo_amber" or
            "holo_crimson" or
            "holo_teal";
    }

    private static bool TryGetCamouflageColor(string optionId, out Color color)
    {
        color = optionId switch
        {
            CCMCustomizationCamouflageIds.Jungle => Color.FromHex("#7BE18C"),
            CCMCustomizationCamouflageIds.Desert => Color.FromHex("#E7BE76"),
            CCMCustomizationCamouflageIds.Snow => Color.FromHex("#E6F4FF"),
            CCMCustomizationCamouflageIds.Classic => Color.FromHex("#D6D0B4"),
            CCMCustomizationCamouflageIds.Urban => Color.FromHex("#B8C4D4"),
            _ => default,
        };

        return optionId != CCMCustomizationCamouflageIds.Default;
    }

    private static bool TryGetChatColorOption(string optionId, out Color color)
    {
        color = optionId switch
        {
            "mint" => Color.FromHex("#6EF2BF"),
            "azure" => Color.FromHex("#7FC9FF"),
            "amber" => Color.FromHex("#F5C46F"),
            "rose" => Color.FromHex("#FF8FB8"),
            "violet" => Color.FromHex("#C58BFF"),
            "crimson" => Color.FromHex("#FF7272"),
            _ => default,
        };

        return optionId != CCMChatColorPresets.Default;
    }
}
