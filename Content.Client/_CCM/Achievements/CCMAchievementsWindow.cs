// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.Client._CCM.UserInterface.Controls;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Shared._RMC14.CCVar;
using Content.Shared._CCM.Achievements;
using Robust.Shared.Configuration;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.IoC;
using Robust.Shared.Input;
using Robust.Shared.Localization;
using Robust.Shared.Utility;

namespace Content.Client._CCM.Achievements;

public sealed class CCMAchievementsWindow : DefaultCMWindow
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    private readonly Font _windowTitleFont;
    private readonly Font _headerFont;
    private readonly Font _sectionFont;
    private readonly Font _titleFont;
    private readonly Font _bodyFont;
    private readonly Font _smallFont;

    private readonly Label _headerLabel;
    private readonly Label _summaryLabel;
    private readonly Button _hideCompletedButton;
    private readonly Button _generalButton;
    private readonly Button _miscButton;
    private readonly Button _marinesButton;
    private readonly Button _xenosButton;
    private readonly Button[] _tabButtons;
    private readonly BoxContainer _content;
    private readonly PanelContainer _heroPanel;
    private readonly PanelContainer _heroAccentLine;

    private CCMAchievementsSnapshot _snapshot = new(0, 0, Array.Empty<CCMAchievementProgressData>());
    private CCMAchievementCategory _category = CCMAchievementCategory.General;
    private bool _hideCompleted;
    private bool _dragging;
    private Vector2 _dragOffset;

    public CCMAchievementsWindow()
    {
        IoCManager.InjectDependencies(this);
        Stylesheet = IoCManager.Resolve<IStylesheetManager>().SheetNano;

        var cache = IoCManager.Resolve<IResourceCache>();
        _windowTitleFont = cache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 16);
        _headerFont = cache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 22);
        _sectionFont = cache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
        _titleFont = cache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 15);
        _bodyFont = cache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 12);
        _smallFont = cache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 11);

        Title = string.Empty;
        MinSize = new Vector2(700, 760);
        WindowTitleLabel.Visible = false;
        WindowTitleLabel.FontOverride = _windowTitleFont;
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
            Text = Loc.GetString("ccm-achievements-header"),
            FontColorOverride = StyleNano.LobbyMenuButtonBase,
            FontOverride = _headerFont,
            HorizontalAlignment = HAlignment.Left,
        };

        var summaryRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
        };

        _summaryLabel = new Label
        {
            HorizontalExpand = true,
            FontOverride = _sectionFont,
            FontColorOverride = GetWindowAccent(),
            VerticalAlignment = VAlignment.Center,
        };

        _hideCompletedButton = new Button
        {
            ToggleMode = true,
            MinSize = new Vector2(220, 34),
        };
        _hideCompletedButton.OnToggled += args =>
        {
            _hideCompleted = args.Pressed;
            Rebuild();
        };
        AttachInteractiveStyle(_hideCompletedButton, selected: () => _hideCompletedButton.Pressed);

        summaryRow.AddChild(_summaryLabel);
        summaryRow.AddChild(_hideCompletedButton);
        (_heroPanel, _heroAccentLine) = BuildHeroPanel(summaryRow);
        root.AddChild(_heroPanel);

        var tabBar = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
        };

        _generalButton = MakeTabButton("ccm-achievements-tab-general", CCMAchievementCategory.General);
        _miscButton = MakeTabButton("ccm-achievements-tab-misc", CCMAchievementCategory.Misc);
        _marinesButton = MakeTabButton("ccm-achievements-tab-marines", CCMAchievementCategory.Marines);
        _xenosButton = MakeTabButton("ccm-achievements-tab-xenos", CCMAchievementCategory.Xenos);
        _tabButtons = new[] { _generalButton, _miscButton, _marinesButton, _xenosButton };

        tabBar.AddChild(_generalButton);
        tabBar.AddChild(_miscButton);
        tabBar.AddChild(_marinesButton);
        tabBar.AddChild(_xenosButton);
        root.AddChild(tabBar);

        var scroll = new ScrollContainer
        {
            VerticalExpand = true,
            HorizontalExpand = true,
            HScrollEnabled = false,
        };

        _content = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 8,
            HorizontalExpand = true,
            Margin = new Thickness(0, 0, 18, 0),
        };
        scroll.AddChild(_content);
        root.AddChild(scroll);

        Contents.AddChild(root);
        OnKeyBindDown += StartDrag;
        OnKeyBindUp += StopDrag;

        OnClose += () =>
        {
            OnKeyBindDown -= StartDrag;
            OnKeyBindUp -= StopDrag;
            _config.UnsubValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged);
            _config.UnsubValueChanged(RMCCVars.RMCLobbyUiStyle, OnThemeChanged);
        };

        ApplyWindowTheme();
        _headerLabel.FontColorOverride = StyleNano.LobbyMenuButtonBase;
        _summaryLabel.FontColorOverride = GetWindowAccent();
        Rebuild();
        _config.OnValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged, false);
        _config.OnValueChanged(RMCCVars.RMCLobbyUiStyle, OnThemeChanged, false);
    }

    private void OnThemeChanged(string _)
    {
        ApplyWindowTheme();
        _headerLabel.FontColorOverride = StyleNano.LobbyMenuButtonBase;
        _summaryLabel.FontColorOverride = GetWindowAccent();
        Rebuild();
    }

    public void SetSnapshot(CCMAchievementsSnapshot snapshot)
    {
        _snapshot = snapshot;
        Rebuild();
    }

    private Button MakeTabButton(string textKey, CCMAchievementCategory category)
    {
        var button = new Button
        {
            Text = Loc.GetString(textKey),
            HorizontalExpand = true,
            MinSize = new Vector2(0, 34),
        };

        button.OnPressed += _ =>
        {
            _category = category;
            Rebuild();
        };

        AttachInteractiveStyle(button, () => _category == category);
        return button;
    }

    private void Rebuild()
    {
        _summaryLabel.Text = Loc.GetString("ccm-achievements-progress-summary",
            ("completed", _snapshot.CompletedCount),
            ("total", _snapshot.TotalCount));
        _hideCompletedButton.Text = Loc.GetString(_hideCompleted
            ? "ccm-achievements-hide-completed-on"
            : "ccm-achievements-hide-completed-off");

        foreach (var button in _tabButtons)
        {
            ApplyTabButtonStyle(button, button == GetSelectedTabButton());
        }

        _content.DisposeAllChildren();
        var achievements = _snapshot.Achievements
            .Where(a => a.Category == _category)
            .Where(a => !_hideCompleted || !a.Completed)
            .ToArray();

        if (achievements.Length == 0)
        {
            _content.AddChild(BuildEmptyLabel());
            return;
        }

        foreach (var achievement in achievements)
        {
            _content.AddChild(BuildAchievementCard(achievement));
        }
    }

    private (PanelContainer Panel, PanelContainer AccentLine) BuildHeroPanel(Control summaryRow)
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

        var accentLine = new PanelContainer
        {
            MinSize = new Vector2(0, 4),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = GetWindowAccent().WithAlpha(0.92f),
            },
        };
        stack.AddChild(accentLine);

        stack.AddChild(_headerLabel);
        stack.AddChild(summaryRow);
        panel.AddChild(stack);
        return (panel, accentLine);
    }

    private Control BuildEmptyLabel()
    {
        var panel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.16f),
                BorderColor = GetWindowAccent().WithAlpha(0.2f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 10,
                ContentMarginTopOverride = 10,
                ContentMarginRightOverride = 10,
                ContentMarginBottomOverride = 10,
            },
        };

        panel.AddChild(new Label
        {
            Text = Loc.GetString("ccm-achievements-empty"),
            FontOverride = _bodyFont,
            HorizontalAlignment = HAlignment.Center,
        });

        return panel;
    }

    private Control BuildAchievementCard(CCMAchievementProgressData achievement)
    {
        var activeAccent = GetWindowAccent();
        var brightAccent = BlendTowards(activeAccent, Color.White, 0.35f);
        var accent = achievement.Completed
            ? brightAccent
            : BlendTowards(activeAccent, Color.White, 0.62f);
        var completion = achievement.Goal <= 0
            ? 1f
            : Math.Clamp(achievement.Progress / (float) achievement.Goal, 0f, 1f);

        var panel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = achievement.Completed
                    ? GetCardBackgroundColor().WithAlpha(0.88f)
                    : GetCardBackgroundColor().WithAlpha(0.72f),
                BorderColor = achievement.Completed
                    ? brightAccent.WithAlpha(0.86f)
                    : activeAccent.WithAlpha(0.42f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 10,
                ContentMarginTopOverride = 10,
                ContentMarginRightOverride = 10,
                ContentMarginBottomOverride = 10,
            },
        };

        var content = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
            HorizontalExpand = true,
        };

        var top = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
        };

        var title = new Label
        {
            Text = Loc.GetString(achievement.TitleKey),
            FontOverride = _titleFont,
            FontColorOverride = accent,
            HorizontalExpand = true,
        };

        var status = new Label
        {
            Text = achievement.Completed
                ? Loc.GetString("ccm-achievements-completed")
                : Loc.GetString("ccm-achievements-in-progress"),
            FontOverride = _smallFont,
            FontColorOverride = achievement.Completed
                ? brightAccent
                : BlendTowards(activeAccent, Color.White, 0.38f),
            HorizontalAlignment = HAlignment.Right,
            VerticalAlignment = VAlignment.Top,
            MinSize = new Vector2(96, 0),
            Align = Label.AlignMode.Right,
        };

        top.AddChild(title);
        top.AddChild(status);
        content.AddChild(top);

        var description = new RichTextLabel
        {
            HorizontalExpand = true,
            HorizontalAlignment = HAlignment.Left,
            MaxWidth = 610,
        };
        description.SetMessage(FormattedMessage.FromMarkupOrThrow($"[color=#D7E1EB]{Loc.GetString(achievement.DescriptionKey)}[/color]"));
        content.AddChild(description);

        var progressLabel = new Label
        {
            Text = Loc.GetString("ccm-achievements-progress-label",
                ("current", achievement.Progress),
                ("goal", achievement.Goal)),
            FontOverride = _smallFont,
            FontColorOverride = achievement.Completed
                ? brightAccent
                : BlendTowards(activeAccent, Color.White, 0.46f),
            HorizontalAlignment = HAlignment.Right,
        };
        content.AddChild(progressLabel);

        var progressBar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = Math.Max(1, achievement.Goal),
            Value = Math.Clamp(achievement.Progress, 0, Math.Max(1, achievement.Goal)),
            MinSize = new Vector2(0, 20),
            HorizontalExpand = true,
            ForegroundStyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = achievement.Completed
                    ? brightAccent
                    : activeAccent.WithAlpha(0.88f),
            },
            BackgroundStyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.34f),
                BorderColor = activeAccent.WithAlpha(0.30f),
                BorderThickness = new Thickness(1),
            },
        };

        var progressPanel = new LayoutContainer
        {
            MinSize = new Vector2(0, 20),
            HorizontalExpand = true,
        };
        LayoutContainer.SetAnchorPreset(progressBar, LayoutContainer.LayoutPreset.Wide);
        progressPanel.AddChild(progressBar);
        var progressText = new Label
        {
            Text = $"{MathF.Round(completion * 100f)}%",
            FontOverride = _smallFont,
            FontColorOverride = achievement.Completed ? Color.Black : Color.White,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        LayoutContainer.SetAnchorPreset(progressText, LayoutContainer.LayoutPreset.Wide);
        progressPanel.AddChild(progressText);
        content.AddChild(progressPanel);

        panel.AddChild(content);
        return panel;
    }

    private Button GetSelectedTabButton()
    {
        return _category switch
        {
            CCMAchievementCategory.Misc => _miscButton,
            CCMAchievementCategory.Marines => _marinesButton,
            CCMAchievementCategory.Xenos => _xenosButton,
            _ => _generalButton,
        };
    }

    private void ApplyWindowTheme()
    {
        var bodyColor = GetTheme() switch
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

        _heroPanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = Color.Black.WithAlpha(0.24f),
            BorderColor = GetWindowAccent().WithAlpha(0.40f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 14,
            ContentMarginTopOverride = 14,
            ContentMarginRightOverride = 14,
            ContentMarginBottomOverride = 14,
        };

        _heroAccentLine.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = GetWindowAccent().WithAlpha(0.92f),
        };
    }

    private StyleNano.UiColorTheme GetTheme()
    {
        return StyleNano.GetConfiguredTheme(_config);
    }

    private Color GetWindowAccent()
    {
        return StyleNano.LobbyMenuButtonBase;
    }

    private Color GetCardBackgroundColor()
    {
        return GetTheme() switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#202730"),
            _ => Color.FromHex("#0A1C0D"),
        };
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

    private void AttachInteractiveStyle(Button button, Func<bool> selected)
    {
        button.OnMouseEntered += _ => ApplyTabButtonState(button, selected(), false);
        button.OnMouseExited += _ => ApplyTabButtonStyle(button, selected());
        button.OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyTabButtonState(button, selected(), true);
        };
        button.OnKeyBindUp += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyTabButtonStyle(button, selected());
        };
    }

    private void ApplyTabButtonStyle(Button button, bool selected)
    {
        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = selected
                ? StyleNano.ButtonColorContextHover.WithAlpha(0.96f)
                : StyleNano.ButtonColorContext.WithAlpha(0.92f),
            BorderColor = selected
                ? GetWindowAccent().WithAlpha(0.82f)
                : GetWindowAccent().WithAlpha(0.55f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 8,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 8,
            ContentMarginBottomOverride = 4,
        };
        button.Label.FontOverride = _sectionFont;
        button.Label.FontColorOverride = selected
            ? Color.FromHex("#F0FFF4")
            : Color.White;
    }

    private void ApplyTabButtonState(Button button, bool selected, bool pressed)
    {
        if (button.Disabled)
        {
            button.StyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.18f),
                BorderColor = GetWindowAccent().WithAlpha(0.24f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 8,
                ContentMarginTopOverride = 4,
                ContentMarginRightOverride = 8,
                ContentMarginBottomOverride = 4,
            };
            button.Label.FontOverride = _sectionFont;
            button.Label.FontColorOverride = Color.FromHex("#76808C");
            return;
        }

        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = selected
                ? (pressed
                    ? GetWindowAccent().WithAlpha(0.30f)
                    : StyleNano.ButtonColorContextHover.WithAlpha(0.96f))
                : pressed
                    ? GetWindowAccent().WithAlpha(0.20f)
                    : StyleNano.ButtonColorContextHover.WithAlpha(0.95f),
            BorderColor = selected || pressed
                ? GetWindowAccent().WithAlpha(0.86f)
                : GetWindowAccent().WithAlpha(0.75f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 8,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 8,
            ContentMarginBottomOverride = 4,
        };
        button.Label.FontOverride = _sectionFont;
        button.Label.FontColorOverride = Color.White;
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
