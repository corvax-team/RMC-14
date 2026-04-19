// CM14 rework: non-RMC edit marker.
using System;
using System.Numerics;
using Content.Client.Gameplay;
using Content.Client.Resources;
using Content.Shared._CCM.Achievements;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Localization;
using Robust.Shared.Timing;

namespace Content.Client._CCM.Achievements;

[UsedImplicitly]
public sealed class CCMAchievementsUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private CCMAchievementsWindow? _window;
    private BoxContainer? _toastRoot;
    private CCMAchievementsSystem? _system;
    private bool _systemSubscribed;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void OnStateEntered(GameplayState state)
    {
        EnsureSystem();

        _toastRoot = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
        };

        LayoutContainer.SetAnchorPreset(_toastRoot, LayoutContainer.LayoutPreset.BottomRight);
        LayoutContainer.SetGrowHorizontal(_toastRoot, LayoutContainer.GrowDirection.Begin);
        LayoutContainer.SetGrowVertical(_toastRoot, LayoutContainer.GrowDirection.Begin);
        LayoutContainer.SetMarginRight(_toastRoot, 14);
        LayoutContainer.SetMarginBottom(_toastRoot, 14);
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
        var titleFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 15);
        var bodyFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 11);

        var panel = new PanelContainer
        {
            MinSize = new Vector2(320, 0),
            MaxSize = new Vector2(360, 240),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = (Content.Client.Stylesheets.StyleNano.CurrentTheme == Content.Client.Stylesheets.StyleNano.UiColorTheme.Blue
                    ? Color.FromHex("#0D2242")
                    : Color.FromHex("#06170B")).WithAlpha(0.96f),
                BorderColor = Content.Client.Stylesheets.StyleNano.LobbyMenuButtonBase.WithAlpha(0.82f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 10,
                ContentMarginTopOverride = 9,
                ContentMarginRightOverride = 10,
                ContentMarginBottomOverride = 9,
            },
        };

        var content = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 5,
        };

        content.AddChild(new Label
        {
            Text = Loc.GetString("ccm-achievements-toast-header"),
            FontOverride = headerFont,
            FontColorOverride = Content.Client.Stylesheets.StyleNano.LobbyMenuButtonBase,
        });

        content.AddChild(new Label
        {
            Text = Loc.GetString(ev.Achievement.TitleKey),
            FontOverride = titleFont,
            FontColorOverride = Color.White,
        });

        content.AddChild(new Label
        {
            Text = Loc.GetString(ev.Achievement.DescriptionKey),
            FontOverride = bodyFont,
            FontColorOverride = Color.FromHex("#D7E1EB"),
        });

        var progressBar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = Math.Max(1, ev.Achievement.Goal),
            Value = Math.Clamp(ev.Achievement.Progress, 0, Math.Max(1, ev.Achievement.Goal)),
            MinSize = new Vector2(0, 18),
            HorizontalExpand = true,
            ForegroundStyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = Content.Client.Stylesheets.StyleNano.LobbyMenuButtonBase,
            },
            BackgroundStyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.35f),
                BorderColor = Content.Client.Stylesheets.StyleNano.LobbyMenuButtonBase.WithAlpha(0.22f),
                BorderThickness = new Thickness(1),
            },
        };

        var progressPanel = new LayoutContainer
        {
            MinSize = new Vector2(0, 18),
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
        content.AddChild(progressPanel);

        content.AddChild(new Label
        {
            Text = Loc.GetString("ccm-achievements-progress-summary",
                ("completed", ev.CompletedCount),
                ("total", ev.TotalCount)),
            FontOverride = bodyFont,
            FontColorOverride = Color.FromHex("#C5D2DE"),
            HorizontalAlignment = Control.HAlignment.Right,
        });

        panel.AddChild(content);
        return panel;
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
