// CM14 rework: non-RMC edit marker.
using System;
using System.Numerics;
using Content.Client.Gameplay;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Shared._RMC14.CCVar;
using Content.Shared._CCM.Achievements;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Localization;
using Robust.Shared.Timing;

namespace Content.Client._CCM.Achievements;

[UsedImplicitly]
public sealed class CCMAchievementsUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    private const float LiveRefreshInterval = 2.5f;
    private const float ToastSafeMarginHorizontal = 68f;
    private const float ToastSafeMarginVertical = 44f;
    private const float ToastWidthMax = 390f;
    private const float ToastHeightMax = 214f;

    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    private CCMAchievementsWindow? _window;
    private BoxContainer? _toastRoot;
    private CCMAchievementsSystem? _system;
    private bool _systemSubscribed;
    private float _refreshTimer;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void OnStateEntered(GameplayState state)
    {
        EnsureSystem();
        _refreshTimer = 0f;

        _toastRoot = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
        };

        LayoutContainer.SetAnchorPreset(_toastRoot, LayoutContainer.LayoutPreset.BottomRight);
        LayoutContainer.SetGrowHorizontal(_toastRoot, LayoutContainer.GrowDirection.Begin);
        LayoutContainer.SetGrowVertical(_toastRoot, LayoutContainer.GrowDirection.Begin);
        // BottomRight anchors expect negative right/bottom margins to keep the control inside the screen.
        LayoutContainer.SetMarginRight(_toastRoot, -ToastSafeMarginHorizontal);
        LayoutContainer.SetMarginBottom(_toastRoot, -ToastSafeMarginVertical);
        UIManager.PopupRoot.AddChild(_toastRoot);
    }

    public void OnStateExited(GameplayState state)
    {
        if (_systemSubscribed && _system != null)
        {
            _system.AchievementsReceived -= OnAchievementsReceived;
            _system.AchievementUnlocked -= OnAchievementUnlocked;
            _systemSubscribed = false;
        }

        if (_toastRoot?.Parent != null)
            _toastRoot.Parent.RemoveChild(_toastRoot);

        _toastRoot = null;
    }

    public void ToggleWindow()
    {
        EnsureWindow();
        if (_window == null)
            return;

        if (_window.IsOpen)
            _window.CloseAnimated();
        else
            OpenWindow();
    }

    public void OpenWindow()
    {
        EnsureSystem();
        EnsureWindow();
        if (_window == null || _system == null)
            return;

        _window.OpenCenteredAnimated();
        if (_system.LatestSnapshot != null)
            _window.SetSnapshot(_system.LatestSnapshot);

        _system.RequestAchievements();
        _refreshTimer = LiveRefreshInterval;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_window is not { Disposed: false, IsOpen: true } || _system == null)
            return;

        _refreshTimer -= args.DeltaSeconds;
        if (_refreshTimer > 0f)
            return;

        _refreshTimer = LiveRefreshInterval;
        _system.RequestAchievements();
    }

    private void EnsureWindow()
    {
        if (_window != null && !_window.Disposed)
            return;

        _window = UIManager.CreateWindow<CCMAchievementsWindow>();
        _window.OnClose += () => { };
    }

    private void OnAchievementsReceived(CCMAchievementsSnapshot snapshot)
    {
        _window?.SetSnapshot(snapshot);
    }

    private void OnAchievementUnlocked(CCMAchievementUnlockedEvent ev)
    {
        _system?.RequestAchievements();

        if (_window != null && !_window.Disposed && _window.IsOpen && _system?.LatestSnapshot != null)
            _window.SetSnapshot(_system.LatestSnapshot);

        if (_toastRoot == null)
            return;

        var toast = BuildToast(ev);
        _toastRoot.AddChild(toast);
        Timer.Spawn(TimeSpan.FromSeconds(6), () =>
        {
            if (!toast.Disposed && toast.Parent != null)
                toast.Parent.RemoveChild(toast);

            toast.Dispose();
        });
    }

    private Control BuildToast(CCMAchievementUnlockedEvent ev)
    {
        var headerFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
        var titleFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 17);
        var bodyFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 11);
        var toastWidth = Math.Clamp(UIManager.PopupRoot.Size.X - ToastSafeMarginHorizontal * 2f, 220f, ToastWidthMax);
        var toastMaxHeight = Math.Clamp(UIManager.PopupRoot.Size.Y - ToastSafeMarginVertical * 2f, 160f, ToastHeightMax);
        var theme = GetTheme();
        var accent = GetToastAccent(theme);
        var accentSoft = accent.WithAlpha(0.28f);
        var baseBackground = GetToastBackground(theme);
        var insetBackground = GetToastInsetBackground(theme);
        var accentText = GetToastAccentText(theme);

        var panel = new PanelContainer
        {
            MinSize = new Vector2(toastWidth, 0),
            MaxSize = new Vector2(toastWidth, toastMaxHeight),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = baseBackground,
                BorderColor = accent.WithAlpha(0.86f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 0,
                ContentMarginTopOverride = 0,
                ContentMarginRightOverride = 0,
                ContentMarginBottomOverride = 0,
            },
        };

        var content = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 0,
            HorizontalExpand = true,
        };

        content.AddChild(new PanelContainer
        {
            MinSize = new Vector2(6, 0),
            VerticalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = accent.WithAlpha(0.95f),
            },
        });

        var inner = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 10,
            Margin = new Thickness(12, 12, 12, 12),
            HorizontalExpand = true,
        };

        var topRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 10,
            HorizontalExpand = true,
        };

        var iconShell = new PanelContainer
        {
            MinSize = new Vector2(56, 56),
            MaxSize = new Vector2(56, 56),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = insetBackground,
                BorderColor = accentSoft,
                BorderThickness = new Thickness(1),
            },
        };
        iconShell.AddChild(new TextureRect
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            Stretch = TextureRect.StretchMode.KeepCentered,
            TextureScale = new Vector2(0.72f, 0.72f),
            Texture = _resourceCache.GetTexture("/Textures/_CCM14/Logo/icon/achievement.png"),
            Modulate = accentText,
        });
        topRow.AddChild(iconShell);

        var headerBlock = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 2,
            HorizontalExpand = true,
        };
        headerBlock.AddChild(new Label
        {
            Text = Loc.GetString("ccm-achievements-toast-header"),
            FontOverride = headerFont,
            FontColorOverride = accent,
        });
        headerBlock.AddChild(new Label
        {
            Text = Loc.GetString(ev.Achievement.TitleKey),
            FontOverride = titleFont,
            FontColorOverride = Color.White,
            ClipText = true,
            HorizontalExpand = true,
        });
        topRow.AddChild(headerBlock);

        var summaryChip = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = insetBackground,
                BorderColor = accentSoft,
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 8,
                ContentMarginTopOverride = 5,
                ContentMarginRightOverride = 8,
                ContentMarginBottomOverride = 5,
            },
        };
        summaryChip.AddChild(new Label
        {
            Text = $"{ev.CompletedCount}/{ev.TotalCount}",
            FontOverride = bodyFont,
            FontColorOverride = accentText,
            HorizontalAlignment = Control.HAlignment.Center,
        });
        topRow.AddChild(summaryChip);
        inner.AddChild(topRow);

        var descriptionPanel = new PanelContainer
        {
            MaxSize = new Vector2(float.MaxValue, 52),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = insetBackground.WithAlpha(0.88f),
                BorderColor = accentSoft,
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 10,
                ContentMarginTopOverride = 8,
                ContentMarginRightOverride = 10,
                ContentMarginBottomOverride = 8,
            },
        };
        descriptionPanel.AddChild(new Label
        {
            Text = Loc.GetString(ev.Achievement.DescriptionKey),
            FontOverride = bodyFont,
            FontColorOverride = GetToastBodyText(theme),
            ClipText = true,
            HorizontalExpand = true,
        });
        inner.AddChild(descriptionPanel);

        var progressBar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = Math.Max(1, ev.Achievement.Goal),
            Value = Math.Clamp(ev.Achievement.Progress, 0, Math.Max(1, ev.Achievement.Goal)),
            MinSize = new Vector2(0, 20),
            HorizontalExpand = true,
            ForegroundStyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = accent,
            },
            BackgroundStyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = insetBackground.WithAlpha(0.88f),
                BorderColor = accentSoft,
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
            Text = Loc.GetString("ccm-achievements-progress-label",
                ("current", ev.Achievement.Progress),
                ("goal", ev.Achievement.Goal)),
            FontOverride = bodyFont,
            FontColorOverride = Color.Black,
            HorizontalAlignment = Control.HAlignment.Center,
            VerticalAlignment = Control.VAlignment.Center,
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        LayoutContainer.SetAnchorPreset(progressText, LayoutContainer.LayoutPreset.Wide);
        progressPanel.AddChild(progressText);
        inner.AddChild(progressPanel);

        var footerRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
        };

        footerRow.AddChild(new Label
        {
            Text = Loc.GetString("ccm-achievements-completed"),
            FontOverride = bodyFont,
            FontColorOverride = accent,
        });

        footerRow.AddChild(new Control
        {
            HorizontalExpand = true,
        });

        footerRow.AddChild(new Label
        {
            Text = Loc.GetString("ccm-achievements-progress-summary",
                ("completed", ev.CompletedCount),
                ("total", ev.TotalCount)),
            FontOverride = bodyFont,
            FontColorOverride = accentText,
            HorizontalAlignment = Control.HAlignment.Right,
        });
        inner.AddChild(footerRow);

        content.AddChild(inner);
        panel.AddChild(content);
        return panel;
    }

    private StyleNano.UiColorTheme GetTheme()
    {
        return StyleNano.GetConfiguredTheme(_config);
    }

    private static Color GetToastAccent(StyleNano.UiColorTheme theme)
    {
        return theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#A7B3C0"),
            _ => Color.FromHex("#6CFF6C"),
        };
    }

    private static Color GetToastBackground(StyleNano.UiColorTheme theme)
    {
        return theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#1A2028").WithAlpha(0.985f),
            _ => Color.FromHex("#082110").WithAlpha(0.985f),
        };
    }

    private static Color GetToastInsetBackground(StyleNano.UiColorTheme theme)
    {
        return theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#252E39").WithAlpha(0.95f),
            _ => Color.FromHex("#12371C").WithAlpha(0.95f),
        };
    }

    private static Color GetToastAccentText(StyleNano.UiColorTheme theme)
    {
        return theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#F0F3F6"),
            _ => Color.FromHex("#E7FFE8"),
        };
    }

    private static Color GetToastBodyText(StyleNano.UiColorTheme theme)
    {
        return theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#D5DCE4"),
            _ => Color.FromHex("#D3EAD7"),
        };
    }

    private void EnsureSystem()
    {
        if (_systemSubscribed)
            return;

        _system = _entManager.System<CCMAchievementsSystem>();
        _system.AchievementsReceived += OnAchievementsReceived;
        _system.AchievementUnlocked += OnAchievementUnlocked;
        _systemSubscribed = true;
    }
}
