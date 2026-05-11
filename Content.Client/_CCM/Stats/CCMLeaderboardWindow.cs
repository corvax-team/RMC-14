// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Numerics;
using Content.Client._CCM.UserInterface.Controls;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Shared._RMC14.CCVar;
using Content.Shared._CCM.Stats;
using Robust.Shared.Configuration;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.IoC;
using Robust.Shared.Input;

namespace Content.Client._CCM.Stats;

public sealed partial class CCMLeaderboardWindow : DefaultCMWindow
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    private readonly CCMStatsSystem _statsSystem;
    private readonly CCMOptionButton _categoryButton;
    private readonly CCMOptionButton _timeframeButton;
    private readonly Button _prevButton;
    private readonly Button _nextButton;
    private readonly Button _goButton;
    private readonly LineEdit _pageEdit;
    private readonly Label _pageLabel;
    private readonly BoxContainer _entries;
    private readonly BoxContainer _viewerRow;
    private readonly Label _viewerHeader;
    private readonly Label _headerLabel;
    private readonly Button[] _navButtons;
    private readonly CCMOptionButton[] _filterButtons;
    private readonly Font _windowTitleFont;
    private readonly Font _headerFont;
    private readonly Font _columnFont;
    private readonly Font _rowFont;
    private bool _dragging;
    private Vector2 _dragOffset;

    private CCMLeaderboardCategory _category = CCMLeaderboardCategory.OverallVictoryPoints;
    private CCMLeaderboardTimeframe _timeframe = CCMLeaderboardTimeframe.AllTime;
    private int _page = 1;
    private int _totalPages = 1;

    private static readonly CCMLeaderboardCategory[] LeaderboardCategories =
    [
        CCMLeaderboardCategory.OverallVictoryPoints,
        CCMLeaderboardCategory.OverallKills,
        CCMLeaderboardCategory.MarineVictoryPoints,
        CCMLeaderboardCategory.MarineImpact,
        CCMLeaderboardCategory.XenoVictoryPoints,
        CCMLeaderboardCategory.XenoImpact,
    ];

    public CCMLeaderboardWindow()
    {
        IoCManager.InjectDependencies(this);
        Stylesheet = IoCManager.Resolve<IStylesheetManager>().SheetNano;
        _statsSystem = _entManager.System<CCMStatsSystem>();
        _windowTitleFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 16);
        _headerFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 22);
        _columnFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
        _rowFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 13);

        Title = string.Empty;
        MinSize = new Vector2(620, 640);
        WindowTitleLabel.FontOverride = _windowTitleFont;
        WindowTitleLabel.Visible = false;
        HeaderPanel.MinSize = new Vector2(0, 26);
        HeaderPanel.Margin = new Thickness(10, 6, 10, 0);
        BodyPanel.Margin = new Thickness(10, -1, 10, 10);

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 8,
            Margin = new Thickness(12, 0, 12, 12),
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        _headerLabel = new Label
        {
            Text = Loc.GetString("ccm-leaderboard-header"),
            FontColorOverride = GetWindowAccent(),
            FontOverride = _headerFont,
            HorizontalAlignment = HAlignment.Left,
        };

        var controls = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
        };

        _categoryButton = new CCMOptionButton
        {
            HorizontalExpand = true,
            MinSize = new Vector2(0, 34),
        };

        foreach (var category in LeaderboardCategories)
        {
            _categoryButton.AddItem(Loc.GetString(GetCategoryLocKey(category)), (int) category);
        }

        _categoryButton.SelectId((int) _category);
        _categoryButton.OnItemSelected += args =>
        {
            _category = (CCMLeaderboardCategory) args.Id;
            args.Button.SelectId(args.Id);
            _page = 1;
            RequestPage();
        };

        _timeframeButton = new CCMOptionButton
        {
            MinSize = new Vector2(170, 34),
        };
        _timeframeButton.AddItem(Loc.GetString("ccm-leaderboard-timeframe-all-time"));
        _timeframeButton.AddItem(Loc.GetString("ccm-leaderboard-timeframe-current-month"));
        _timeframeButton.SelectId((int) _timeframe);
        _timeframeButton.OnItemSelected += args =>
        {
            _timeframe = (CCMLeaderboardTimeframe) args.Id;
            args.Button.SelectId(args.Id);
            _page = 1;
            RequestPage();
        };

        controls.AddChild(_categoryButton);
        controls.AddChild(_timeframeButton);
        root.AddChild(BuildHeroPanel(controls));

        root.AddChild(BuildColumnsHeader());

        var scroll = new ScrollContainer
        {
            VerticalExpand = true,
            HorizontalExpand = true,
            HScrollEnabled = false,
        };

        _entries = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
            HorizontalExpand = true,
        };

        scroll.AddChild(_entries);
        root.AddChild(scroll);

        _viewerHeader = new Label
        {
            Text = Loc.GetString("ccm-leaderboard-your-position"),
            FontColorOverride = GetWindowAccent(),
            FontOverride = _columnFont,
            Visible = false,
        };
        root.AddChild(_viewerHeader);

        _viewerRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
            Visible = false,
        };
        root.AddChild(_viewerRow);

        var pageControls = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
            Margin = new Thickness(0, 6, 0, 0),
        };

        _prevButton = new Button
        {
            Text = Loc.GetString("ccm-common-previous"),
            MinSize = new Vector2(100, 34),
        };
        _prevButton.OnPressed += _ =>
        {
            if (_page <= 1)
                return;

            _page--;
            RequestPage();
        };

        _pageLabel = new Label
        {
            HorizontalExpand = true,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            FontOverride = _columnFont,
        };

        _pageEdit = new LineEdit
        {
            MinSize = new Vector2(72, 38),
            Text = "1",
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
        };

        _goButton = new Button
        {
            Text = Loc.GetString("ccm-common-go"),
            MinSize = new Vector2(90, 34),
        };
        _goButton.OnPressed += _ => JumpToPage();

        _nextButton = new Button
        {
            Text = Loc.GetString("ccm-common-next"),
            MinSize = new Vector2(100, 34),
        };
        _nextButton.OnPressed += _ =>
        {
            if (_page >= _totalPages)
                return;

            _page++;
            RequestPage();
        };

        _navButtons = new[] { _prevButton, _goButton, _nextButton };
        _filterButtons = new[] { _categoryButton, _timeframeButton };

        pageControls.AddChild(_prevButton);
        pageControls.AddChild(_pageLabel);
        pageControls.AddChild(_pageEdit);
        pageControls.AddChild(_goButton);
        pageControls.AddChild(_nextButton);
        root.AddChild(pageControls);

        Contents.AddChild(root);
        OnKeyBindDown += StartDrag;
        OnKeyBindUp += StopDrag;

        _statsSystem.LeaderboardReceived += OnLeaderboardReceived;
        OnClose += () =>
        {
            _statsSystem.LeaderboardReceived -= OnLeaderboardReceived;
            OnKeyBindDown -= StartDrag;
            OnKeyBindUp -= StopDrag;
            _config.UnsubValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged);
            _config.UnsubValueChanged(RMCCVars.RMCLobbyUiStyle, OnThemeChanged);
        };

        AttachInteractionState();
        ApplyWindowTheme();
        _config.OnValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged, false);
        _config.OnValueChanged(RMCCVars.RMCLobbyUiStyle, OnThemeChanged, false);
    }

    private void OnThemeChanged(string _)
    {
        ApplyWindowTheme();
    }

    public void RefreshData()
    {
        RequestPage();
    }

    private Control BuildHeroPanel(Control controls)
    {
        var panel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.24f),
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
            SeparationOverride = 10,
        };

        stack.AddChild(new PanelContainer
        {
            MinSize = new Vector2(0, 4),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = GetWindowAccent().WithAlpha(0.92f),
            },
        });

        stack.AddChild(_headerLabel);
        stack.AddChild(controls);
        panel.AddChild(stack);
        return panel;
    }

    private void OnLeaderboardReceived(CCMLeaderboardPage data)
    {
        if (data.Category != _category || data.Timeframe != _timeframe)
            return;

        _page = data.Page;
        _totalPages = data.TotalPages;
        _pageEdit.Text = _page.ToString();
        _pageLabel.Text = Loc.GetString("ccm-leaderboard-page-label", ("page", _page), ("pages", _totalPages));
        _prevButton.Disabled = _page <= 1;
        _nextButton.Disabled = _page >= _totalPages;
        foreach (var button in _navButtons)
        {
            StyleNavButton(button);
        }

        _entries.DisposeAllChildren();
        if (data.Entries.Length == 0)
        {
            var emptyLabel = new RichTextLabel
            {
                HorizontalExpand = true,
                Text = Loc.GetString("ccm-leaderboard-empty"),
            };
            emptyLabel.Margin = new Thickness(6, 8, 6, 0);
            _entries.AddChild(emptyLabel);
        }
        else
        {
            foreach (var entry in data.Entries)
            {
                _entries.AddChild(BuildRow(entry.Rank.ToString(), entry.Ckey, entry.Score.ToString(), entry.IsViewer));
            }
        }

        _viewerRow.DisposeAllChildren();
        _viewerHeader.Visible = data.ViewerEntry != null;
        _viewerRow.Visible = data.ViewerEntry != null;

        if (data.ViewerEntry != null)
        {
            _viewerRow.AddChild(BuildRow(
                data.ViewerEntry.Rank.ToString(),
                data.ViewerEntry.Ckey,
                data.ViewerEntry.Score.ToString(),
                true));
        }
    }

    private void JumpToPage()
    {
        if (!int.TryParse(_pageEdit.Text, out var requested))
            return;

        _page = Math.Clamp(requested, 1, Math.Max(1, _totalPages));
        RequestPage();
    }

    private void RequestPage()
    {
        _statsSystem.RequestLeaderboard(_category, _timeframe, _page);
    }

    private PanelContainer BuildColumnsHeader()
    {
        var panel = new PanelContainer
        {
            PanelOverride = MakeRowPanel(true, false),
            MouseFilter = MouseFilterMode.Stop,
        };

        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 0,
            HorizontalExpand = true,
        };

        row.AddChild(WrapColumn(BuildColumnLabel(Loc.GetString("ccm-leaderboard-column-rank"), new Vector2(56, 0), HAlignment.Left), false));
        row.AddChild(WrapColumn(BuildColumnLabel(Loc.GetString("ccm-leaderboard-column-player"), new Vector2(260, 0), HAlignment.Left), true));
        row.AddChild(WrapColumn(BuildColumnLabel(Loc.GetString("ccm-leaderboard-column-score"), new Vector2(92, 0), HAlignment.Center), false));
        panel.AddChild(row);
        return panel;
    }

    private PanelContainer BuildRow(string rank, string player, string score, bool highlight)
    {
        var accent = highlight ? GetViewerTextColor() : Color.White;
        var panel = new PanelContainer
        {
            PanelOverride = MakeRowPanel(false, highlight),
            Margin = new Thickness(0, 0, 0, 2),
            MouseFilter = MouseFilterMode.Stop,
        };

        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 0,
            HorizontalExpand = true,
        };

        row.AddChild(WrapColumn(BuildRowLabel(rank, accent, new Vector2(56, 0), HAlignment.Left), false));
        row.AddChild(WrapColumn(BuildRowLabel(player, accent, new Vector2(260, 0), HAlignment.Left), true));
        row.AddChild(WrapColumn(BuildRowLabel(score, accent, new Vector2(92, 0), HAlignment.Center), false));
        panel.AddChild(row);
        return panel;
    }

    private StyleBoxFlat MakeRowPanel(bool header, bool highlight)
    {
        return new StyleBoxFlat
        {
            BackgroundColor = header
                ? Color.Black.WithAlpha(0.22f)
                : highlight
                    ? GetWindowAccent().WithAlpha(0.18f)
                    : Color.Black.WithAlpha(0.24f),
            BorderColor = header
                ? GetWindowAccent().WithAlpha(0.75f)
                : highlight
                    ? GetWindowAccent().WithAlpha(0.46f)
                    : GetWindowAccent().WithAlpha(0.22f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 8,
            ContentMarginTopOverride = 6,
            ContentMarginRightOverride = 8,
            ContentMarginBottomOverride = 6,
        };
    }

    private Label BuildColumnLabel(string text, Vector2 minSize, HAlignment alignment)
    {
        return new Label
        {
            Text = text,
            MinSize = minSize,
            HorizontalAlignment = alignment,
            Align = alignment switch
            {
                HAlignment.Right => Label.AlignMode.Right,
                HAlignment.Center => Label.AlignMode.Center,
                _ => Label.AlignMode.Left,
            },
            FontOverride = _columnFont,
            FontColorOverride = GetWindowAccent(),
            VerticalAlignment = VAlignment.Center,
            HorizontalExpand = true,
        };
    }

    private Label BuildRowLabel(string text, Color color, Vector2 minSize, HAlignment alignment)
    {
        return new Label
        {
            Text = text,
            MinSize = minSize,
            HorizontalAlignment = alignment,
            Align = alignment switch
            {
                HAlignment.Right => Label.AlignMode.Right,
                HAlignment.Center => Label.AlignMode.Center,
                _ => Label.AlignMode.Left,
            },
            FontOverride = _rowFont,
            FontColorOverride = color,
            HorizontalExpand = true,
            ClipText = true,
            VerticalAlignment = VAlignment.Center,
        };
    }

    private PanelContainer WrapColumn(Control child, bool expand)
    {
        var panel = new PanelContainer
        {
            HorizontalExpand = expand,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Transparent,
                BorderColor = GetWindowAccent().WithAlpha(0.18f),
                BorderThickness = new Thickness(0, 0, 1, 0),
                ContentMarginLeftOverride = 4,
                ContentMarginTopOverride = 0,
                ContentMarginRightOverride = 4,
                ContentMarginBottomOverride = 0,
            },
            MouseFilter = MouseFilterMode.Stop,
        };
        panel.AddChild(child);
        return panel;
    }

    private void StyleOptionButton(OptionButton button)
    {
        button.ModulateSelfOverride = Color.White;
        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = StyleNano.DropdownButtonColorContext,
            BorderColor = GetWindowAccent(),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 6,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 6,
            ContentMarginBottomOverride = 4,
        };
    }

    private void StyleNavButton(Button button)
    {
        button.ModulateSelfOverride = Color.White;
        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = button.Disabled
                ? Color.Black.WithAlpha(0.18f)
                : StyleNano.ButtonColorContext.WithAlpha(0.92f),
            BorderColor = button.Disabled
                ? GetWindowAccent().WithAlpha(0.24f)
                : GetWindowAccent().WithAlpha(0.55f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 6,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 6,
            ContentMarginBottomOverride = 4,
        };
        button.Label.FontOverride = _columnFont;
        button.Label.FontColorOverride = button.Disabled
            ? Color.FromHex("#76808C")
            : Color.FromHex("#C5CED8");
    }

    private void AttachInteractionState()
    {
        foreach (var button in _navButtons)
        {
            button.OnMouseEntered += _ =>
            {
                if (!button.Disabled)
                    ApplyNavState(button, false);
            };
            button.OnMouseExited += _ => StyleNavButton(button);
            button.OnKeyBindDown += args =>
            {
                if (args.Function != EngineKeyFunctions.UIClick || button.Disabled)
                    return;

                ApplyNavState(button, true);
            };
            button.OnKeyBindUp += args =>
            {
                if (args.Function != EngineKeyFunctions.UIClick)
                    return;

                StyleNavButton(button);
            };
        }

        foreach (var button in _filterButtons)
        {
            button.OnMouseEntered += _ => ApplyFilterState(button, false);
            button.OnMouseExited += _ => StyleOptionButton(button);
            button.OnKeyBindDown += args =>
            {
                if (args.Function != EngineKeyFunctions.UIClick)
                    return;

                ApplyFilterState(button, true);
            };
            button.OnKeyBindUp += args =>
            {
                if (args.Function != EngineKeyFunctions.UIClick)
                    return;

                StyleOptionButton(button);
            };
        }
    }

    private void ApplyNavState(Button button, bool pressed)
    {
        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = pressed
                ? GetWindowAccent().WithAlpha(0.92f)
                : StyleNano.ButtonColorContextHover.WithAlpha(0.95f),
            BorderColor = pressed
                ? GetWindowAccent()
                : GetWindowAccent().WithAlpha(0.75f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 6,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 6,
            ContentMarginBottomOverride = 4,
        };
        button.Label.FontOverride = _columnFont;
        button.Label.FontColorOverride = pressed ? Color.Black : GetWindowAccent();
    }

    private void ApplyFilterState(OptionButton button, bool pressed)
    {
        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = pressed
                ? StyleNano.DropdownButtonColorContextPressed
                : StyleNano.DropdownButtonColorContextHover,
            BorderColor = pressed
                ? GetWindowAccent()
                : GetWindowAccent(),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 6,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 6,
            ContentMarginBottomOverride = 4,
        };
    }

    private void ApplyWindowTheme()
    {
        var bodyColor = StyleNano.CurrentTheme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#1A2028").WithAlpha(0.94f),
            _ => Color.FromHex("#05180A").WithAlpha(0.94f),
        };
        var borderColor = GetWindowAccent().WithAlpha(0.65f);

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

        _pageEdit.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = Color.Black.WithAlpha(0.28f),
            BorderColor = GetWindowAccent().WithAlpha(0.5f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 10,
            ContentMarginTopOverride = 6,
            ContentMarginRightOverride = 10,
            ContentMarginBottomOverride = 6,
        };

        WindowTitleLabel.FontColorOverride = Color.White;
        _headerLabel.FontColorOverride = GetWindowAccent();
        _viewerHeader.FontColorOverride = GetWindowAccent();
        foreach (var button in _navButtons)
        {
            StyleNavButton(button);
        }

        foreach (var button in _filterButtons)
        {
            StyleOptionButton(button);
        }
    }

    private Color GetWindowAccent()
    {
        return StyleNano.LobbyMenuButtonBase;
    }

    private Color GetViewerTextColor()
    {
        return StyleNano.CurrentTheme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#EEF2F6"),
            _ => Color.FromHex("#D7F0DE"),
        };
    }

    private static string GetCategoryLocKey(CCMLeaderboardCategory category)
    {
        return category switch
        {
            CCMLeaderboardCategory.OverallVictoryPoints => "ccm-leaderboard-category-overall-vp",
            CCMLeaderboardCategory.OverallKills => "ccm-leaderboard-category-overall-kills",
            CCMLeaderboardCategory.MarineVictoryPoints => "ccm-leaderboard-category-marine-vp",
            CCMLeaderboardCategory.MarineImpact => "ccm-leaderboard-category-marine-impact",
            CCMLeaderboardCategory.MarineKills => "ccm-leaderboard-category-marine-kills",
            CCMLeaderboardCategory.XenoVictoryPoints => "ccm-leaderboard-category-xeno-vp",
            CCMLeaderboardCategory.XenoImpact => "ccm-leaderboard-category-xeno-impact",
            CCMLeaderboardCategory.XenoKills => "ccm-leaderboard-category-xeno-kills",
            _ => "ccm-leaderboard-category-overall-vp",
        };
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        if (!_dragging)
            return;

        var topLeftGlobal = args.GlobalPosition - _dragOffset;
        var parentGlobal = Parent?.GlobalPosition ?? Vector2.Zero;
        LayoutContainer.SetPosition(this, topLeftGlobal - parentGlobal);
    }

    private void StartDrag(GUIBoundKeyEventArgs args)
    {
        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        var global = args.PointerLocation.Position / UIScale;
        if (IsPointOverInteractiveControl(global))
            return;

        _dragging = true;
        _dragOffset = global - GlobalPosition;
        args.Handle();
    }

    private void StopDrag(GUIBoundKeyEventArgs args)
    {
        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        _dragging = false;
        args.Handle();
    }

    private bool IsPointOverInteractiveControl(Vector2 globalPosition)
    {
        return FindInteractiveControl(this, globalPosition) != null;
    }

    private Control? FindInteractiveControl(Control control, Vector2 globalPosition)
    {
        foreach (var child in control.Children)
        {
            if (!ContainsGlobalPoint(child, globalPosition))
                continue;

            var nested = FindInteractiveControl(child, globalPosition);
            if (nested != null)
                return nested;

            if (child != this && IsInteractiveControl(child))
                return child;
        }

        return null;
    }

    private static bool ContainsGlobalPoint(Control control, Vector2 globalPosition)
    {
        if (!control.Visible)
            return false;

        var min = control.GlobalPosition;
        var max = min + control.Size;
        return globalPosition.X >= min.X && globalPosition.X <= max.X &&
               globalPosition.Y >= min.Y && globalPosition.Y <= max.Y;
    }

    private static bool IsInteractiveControl(Control control)
    {
        return control is BaseButton or OptionButton or LineEdit or TextEdit or ScrollBar;
    }
}
