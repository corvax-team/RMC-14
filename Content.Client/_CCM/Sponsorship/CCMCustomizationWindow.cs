using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.Client._CCM.UserInterface.Controls;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Shared._CCM.Sponsorship;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.IoC;
using Robust.Shared.Input;
using Robust.Shared.Maths;
using Robust.Shared.Utility;

namespace Content.Client._CCM.Sponsorship;

public sealed partial class CCMCustomizationWindow : DefaultCMWindow
{
    private enum CustomizationPage : byte
    {
        Xeno,
        Marines,
        Misc,
    }

    private readonly record struct CustomOption(string Id, string NameKey, string? PreviewTexturePath = null);

    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private readonly Dictionary<string, CCMOptionButton> _selectors = new();
    private readonly Dictionary<string, TextureRect> _xenoPreviewTextures = new();
    private readonly Dictionary<string, TextureRect> _dynamicPreviewTextures = new();
    private readonly Dictionary<string, Label> _camoPreviewLabels = new();
    private readonly List<(Control Overlay, Func<bool> Visible)> _availabilityOverlays = new();
    private readonly Dictionary<CustomizationPage, Button> _pageButtons = new();
    private readonly Label _statusLabel;
    private readonly Label _statusHintLabel;
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
        ["xeno_defender"] = "/Textures/_RMC14/Mobs/Xenonids/Defender/defender.rsi/alive.png",
        ["xeno_drone"] = "/Textures/_RMC14/Mobs/Xenonids/Drone/drone.rsi/alive.png",
        ["xeno_queen"] = "/Textures/_RMC14/Mobs/Xenonids/Queen/queen.rsi/alive.png",
        ["xeno_runner"] = "/Textures/_RMC14/Mobs/Xenonids/Runner/runner.rsi/alive.png",
        ["xeno_sentinel"] = "/Textures/_RMC14/Mobs/Xenonids/Sentinel/sentinel.rsi/alive.png",
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

    public CCMCustomizationWindow()
    {
        IoCManager.InjectDependencies(this);

        Title = string.Empty;
        MinSize = new Vector2(872, 780);
        WindowTitleLabel.Visible = false;
        HeaderPanel.MinSize = new Vector2(0, 26);
        HeaderPanel.Margin = new Thickness(10, 6, 10, 0);
        BodyPanel.Margin = new Thickness(10, -1, 10, 10);

        ApplyWindowTheme();

        _statusLabel = new Label
        {
            FontColorOverride = StyleNano.LobbyMenuButtonBase,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 12),
        };
        _statusHintLabel = new Label
        {
            FontColorOverride = Color.FromHex("#B7C3CE"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 11),
            HorizontalExpand = true,
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
        UpdateStatusText();
        UpdateOocTagControls();
        UpdateTagPreview();
        UpdatePageState();
        UpdateAllXenoPreviewSelections();
        UpdateDynamicPreviews();
        _savedSnapshot = BuildSnapshot();
        UpdateSaveState();
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
        UpdateOocTagControls();
        UpdateTagPreview();
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
                BackgroundColor = Color.Black.WithAlpha(0.24f),
                BorderColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.40f),
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

        stack.AddChild(new PanelContainer
        {
            MinSize = new Vector2(0, 5),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.90f),
            },
        });

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
        titleStack.AddChild(new Label
        {
            Text = Loc.GetString("ccm-customization-header"),
            FontColorOverride = StyleNano.LobbyMenuButtonBase,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 24),
        });
        titleStack.AddChild(new Label
        {
            Text = Loc.GetString("ccm-customization-status-locked"),
            FontColorOverride = Color.FromHex("#A8B5C1"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 11),
        });
        titleStack.AddChild(new Label
        {
            Text = Loc.GetString("ccm-customization-wip"),
            FontColorOverride = Color.FromHex("#8FD4FF"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 10),
        });

        titleRow.AddChild(titleStack);
        stack.AddChild(titleRow);

        var infoGrid = new GridContainer
        {
            Columns = 2,
            HSeparationOverride = 12,
            VSeparationOverride = 12,
        };
        infoGrid.AddChild(BuildHeroInfoCard(_statusLabel, StyleNano.LobbyMenuButtonBase));
        infoGrid.AddChild(BuildHeroInfoCard(_statusHintLabel, Color.FromHex("#77E3FF")));
        stack.AddChild(infoGrid);

        hero.AddChild(stack);
        return hero;
    }

    private Control BuildBottomActionBar()
    {
        var bar = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.18f),
                BorderColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.28f),
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
        return bar;
    }

    private Control BuildHeroInfoCard(Control content, Color accent)
    {
        var panel = new PanelContainer
        {
            MinSize = new Vector2(0, 64),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = accent.WithAlpha(0.09f),
                BorderColor = accent.WithAlpha(0.28f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 10,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 10,
            },
        };

        var stack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
        };
        stack.AddChild(new PanelContainer
        {
            MinSize = new Vector2(34, 3),
            MaxSize = new Vector2(34, 3),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = accent.WithAlpha(0.96f),
            },
        });
        stack.AddChild(content);
        panel.AddChild(stack);
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
                BackgroundColor = Color.Black.WithAlpha(0.16f),
                BorderColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.32f),
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
        titleRow.AddChild(new PanelContainer
        {
            MinSize = new Vector2(24, 24),
            MaxSize = new Vector2(24, 24),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.14f),
                BorderColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.34f),
                BorderThickness = new Thickness(1),
            },
        });
        titleRow.AddChild(new Label
        {
            Text = Loc.GetString(titleKey),
            FontColorOverride = StyleNano.LobbyMenuButtonBase,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 16),
            VerticalAlignment = VAlignment.Center,
        });

        stack.AddChild(titleRow);
        stack.AddChild(body);
        panel.AddChild(stack);
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
            "Marine",
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
            "Marine",
            BuildCamouflagePreview("armor_palette")));
        grid.AddChild(BuildCamouflageCard(
            "weapon_spray",
            "ccm-customization-slot-weapon-spray",
            "Weapon",
            BuildCamouflagePreview("weapon_spray")));

        stack.AddChild(grid);
        return stack;
    }

    private Control BuildXenoSkinCard(string slotId, string titleKey)
    {
        var accent = GetSlotAccent(slotId);
        var selector = MakeSelector(slotId, 0);
        selector.OnItemSelected += _ => UpdateXenoPreviewSelection(slotId);
        selector.HorizontalExpand = true;

        var card = BuildDecoratedCard(accent, 196,
            BuildCardHeader(Loc.GetString(titleKey), "Xeno", accent),
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
                BackgroundColor = Color.Black.WithAlpha(0.12f),
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
        return frame;
    }

    private Control BuildArmorCard()
    {
        var accent = GetSlotAccent("armor_paint");
        var selector = MakeSelector("armor_paint", 0);
        selector.HorizontalExpand = true;
        selector.OnItemSelected += _ => UpdateDynamicPreviews();

        return BuildDecoratedCard(accent, 228,
            BuildCardHeader(Loc.GetString("ccm-customization-slot-armor"), "Marine", accent),
            BuildArmorPreview(),
            selector);
    }

    private Control BuildCamouflageCard(string slotId, string titleKey, string badgeText, Control preview)
    {
        var accent = GetSlotAccent(slotId);
        var selector = MakeSelector(slotId, 0);
        selector.HorizontalExpand = true;
        selector.OnItemSelected += _ => UpdateDynamicPreviews();
        var hintKey = slotId == "armor_variant"
            ? "ccm-customization-armor-variant-hint"
            : "ccm-customization-camo-hint";

        var card = BuildDecoratedCard(accent, 228,
            BuildCardHeader(Loc.GetString(titleKey), badgeText, accent),
            MakeWrappedText(Loc.GetString(hintKey), Color.FromHex("#B4BFCA"), 11, 392),
            preview,
            selector);

        return slotId is "armor_palette" or "armor_variant"
            ? card
            : WrapWithAvailabilityOverlay(card, () => !(_status?.CustomizationUnlocked ?? false));
    }

    private Control BuildGhostCard()
    {
        var accent = GetSlotAccent("ghost");
        var selector = MakeSelector("ghost", 0);
        selector.HorizontalExpand = true;
        selector.OnItemSelected += _ => UpdateDynamicPreviews();

        var card = BuildDecoratedCard(accent, 200,
            BuildCardHeader(Loc.GetString("ccm-customization-slot-ghost"), "Misc", accent),
            BuildGhostPreview(),
            selector);
        return WrapWithAvailabilityOverlay(card, () => !(_status?.CustomizationUnlocked ?? false));
    }

    private Control BuildTagCard()
    {
        var accent = GetSlotAccent("ooc");

        var customTagPanel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.18f),
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

        var customTagGuard = WrapWithAvailabilityOverlay(customTagPanel, () => _status?.Tier != CCMSponsorshipTier.SponsorIII);

        var card = BuildDecoratedCard(accent, 332,
            BuildCardHeader(Loc.GetString("ccm-customization-slot-ooc-tag"), "OOC", accent),
            MakeWrappedText(Loc.GetString("ccm-customization-tag-hint"), Color.FromHex("#B4BFCA"), 11),
            _oocTagSelector,
            customTagGuard,
            BuildPreviewBubble(_tagPreviewLabel, Color.FromHex("#77E3FF").WithAlpha(0.30f), minHeight: 60));

        return card;
    }

    private Control BuildChatColorCard(string titleKey, CCMOptionButton selector, Label previewLabel, string previewChannel)
    {
        var card = BuildDecoratedCard(Color.FromHex("#77E3FF"), 170,
            new Label
            {
                Text = Loc.GetString(titleKey),
                FontColorOverride = Color.White,
                FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13),
            },
            selector,
            BuildPreviewBubble(previewLabel, Color.FromHex("#77E3FF").WithAlpha(0.26f), previewChannel));
        return WrapWithAvailabilityOverlay(card, () => !(_status?.CustomizationUnlocked ?? false));
    }

    private Control BuildPreviewBubble(Label content, Color borderColor, string? prefix = null, float minHeight = 82)
    {
        var bubble = new PanelContainer
        {
            MinSize = new Vector2(0, minHeight),
            VerticalExpand = true,
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
            bubble.AddChild(content);
            return bubble;
        }

        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
        };
        row.AddChild(new Label
        {
            Text = prefix,
            FontColorOverride = Color.FromHex("#9DB6C5"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 11),
            VerticalAlignment = VAlignment.Center,
        });
        row.AddChild(content);
        bubble.AddChild(row);
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
                BackgroundColor = Color.Black.WithAlpha(0.22f),
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
                BackgroundColor = Color.Black.WithAlpha(0.22f),
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
        return preview;
    }

    private Control BuildCardHeader(string title, string badgeText, Color accent)
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
        row.AddChild(new Label
        {
            Text = badgeText,
            FontColorOverride = accent,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 11),
            HorizontalAlignment = HAlignment.Right,
        });
        return row;
    }

    private Control BuildDecoratedCard(Color accent, float minHeight, params Control[] body)
    {
        var panel = new PanelContainer
        {
            MinSize = new Vector2(0, minHeight),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.17f),
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
        stack.AddChild(new PanelContainer
        {
            MinSize = new Vector2(0, 4),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = accent.WithAlpha(0.90f),
            },
        });

        foreach (var child in body)
        {
            stack.AddChild(child);
        }

        panel.AddChild(stack);
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
                BackgroundColor = color.WithAlpha(0.08f),
                BorderColor = color.WithAlpha(0.24f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 8,
                ContentMarginTopOverride = 8,
                ContentMarginRightOverride = 8,
                ContentMarginBottomOverride = 8,
            },
        };

        var texture = new TextureRect
        {
            Texture = _resourceCache.GetTexture("/Textures/Mobs/Ghosts/ghost_human.rsi/icon.png"),
            Stretch = TextureRect.StretchMode.KeepCentered,
            TextureScale = new Vector2(2.8f, 2.8f),
            HorizontalExpand = true,
            VerticalExpand = true,
            ModulateSelfOverride = color,
        };
        _dynamicPreviewTextures[$"ghost:{key}"] = texture;
        panel.AddChild(texture);
        return panel;
    }

    private Control WrapWithAvailabilityOverlay(Control target, Func<bool> predicate, Control? wholeCard = null)
    {
        var overlay = new PanelContainer
        {
            Visible = false,
            MouseFilter = MouseFilterMode.Ignore,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.58f),
                BorderColor = Color.White.WithAlpha(0.05f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 12,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 12,
            },
        };
        overlay.AddChild(new Label
        {
            Text = Loc.GetString("ccm-customization-overlay-locked"),
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            HorizontalExpand = true,
            VerticalExpand = true,
            FontColorOverride = Color.FromHex("#DCE5EE"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 12),
        });

        var content = wholeCard ?? target;
        var container = new CCMAvailabilityOverlayContainer(content, overlay)
        {
            HorizontalExpand = true,
        };

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
                BorderColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.50f),
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
        var customSelected = OocTagOptions[Math.Clamp(_oocTagSelector.SelectedId, 0, OocTagOptions.Length - 1)].Id == CCMOocTags.Custom;
        var canUseCustomTag = _status?.Tier == CCMSponsorshipTier.SponsorIII;
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

        var selected = SlotOptions["ghost"][Math.Clamp(selector.SelectedId, 0, SlotOptions["ghost"].Length - 1)].Id;

        if (_dynamicPreviewTextures.TryGetValue("ghost:current", out var currentTexture))
        {
            currentTexture.ModulateSelfOverride = selected switch
            {
                "holo_green" => Color.FromHex("#7CFF9A"),
                "holo_blue" => Color.FromHex("#77E3FF"),
                _ => Color.White.WithAlpha(0.90f),
            };
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

        var selected = SlotOptions[slotId][Math.Clamp(selector.SelectedId, 0, SlotOptions[slotId].Length - 1)].Id;
        label.Text = selected switch
        {
            CCMCustomizationArmorVariantIds.None => "NONE",
            CCMCustomizationArmorVariantIds.Padded => "PADDED",
            CCMCustomizationArmorVariantIds.Padless => "PADLESS",
            CCMCustomizationArmorVariantIds.Ridged => "RIDGED",
            CCMCustomizationArmorVariantIds.Carrier => "CARRIER",
            CCMCustomizationArmorVariantIds.Skull => "SKULL",
            CCMCustomizationArmorVariantIds.Smooth => "SMOOTH",
            CCMCustomizationCamouflageIds.Desert => "DESERT",
            CCMCustomizationCamouflageIds.Snow => "SNOW",
            CCMCustomizationCamouflageIds.Classic => "CLASSIC",
            CCMCustomizationCamouflageIds.Urban => "URBAN",
            _ => "JUNGLE",
        };

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
            CustomizationPage.Xeno => Color.FromHex("#8BE39E"),
            CustomizationPage.Marines => Color.FromHex("#FFB36F"),
            _ => Color.FromHex("#77E3FF"),
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
        _saveButton.ModulateSelfOverride = Color.White;
        _saveButton.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = !enabled
                ? Color.Black.WithAlpha(0.18f)
                : pressed
                    ? StyleNano.LobbyMenuButtonBase.WithAlpha(0.92f)
                    : hovered
                        ? StyleNano.ButtonColorContextHover.WithAlpha(0.96f)
                        : StyleNano.ButtonColorContext.WithAlpha(0.92f),
            BorderColor = !enabled
                ? Color.FromHex("#546372").WithAlpha(0.42f)
                : pressed
                    ? StyleNano.LobbyMenuButtonBase
                    : StyleNano.LobbyMenuButtonBase.WithAlpha(0.76f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 10,
            ContentMarginTopOverride = 3,
            ContentMarginRightOverride = 10,
            ContentMarginBottomOverride = 3,
        };
        _saveButton.Label.FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 12);
        _saveButton.Label.FontColorOverride = !enabled
            ? Color.FromHex("#7A8794")
            : pressed
                ? Color.Black
                : StyleNano.LobbyMenuButtonBase;
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
            ? StyleNano.LobbyMenuButtonBase
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
        var bodyColor = StyleNano.CurrentTheme == StyleNano.UiColorTheme.Blue
            ? Color.FromHex("#102A56").WithAlpha(0.94f)
            : Color.FromHex("#05180A").WithAlpha(0.94f);
        var borderColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.65f);

        HeaderPanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = bodyColor,
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

    private static Color GetSlotAccent(string slotId)
    {
        return slotId switch
        {
            "xeno_defender" => Color.FromHex("#7BE18C"),
            "xeno_drone" => Color.FromHex("#66D9FF"),
            "xeno_queen" => Color.FromHex("#D88BFF"),
            "xeno_runner" => Color.FromHex("#FFB36F"),
            "xeno_sentinel" => Color.FromHex("#FFD45F"),
            "ghost" => Color.FromHex("#9BE9FF"),
            "weapon_spray" => Color.FromHex("#7FC9FF"),
            "armor_palette" => Color.FromHex("#F5C46F"),
            "armor_variant" => Color.FromHex("#B4C6FF"),
            "armor_paint" => Color.FromHex("#FF9D6B"),
            "ooc" => Color.FromHex("#8FF0C4"),
            _ => StyleNano.LobbyMenuButtonBase,
        };
    }

    private static bool TryGetOptionTextColor(string slotId, string optionId, out Color color)
    {
        if (slotId is "armor_palette" or "weapon_spray")
            return TryGetCamouflageColor(optionId, out color);

        color = default;
        return false;
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
