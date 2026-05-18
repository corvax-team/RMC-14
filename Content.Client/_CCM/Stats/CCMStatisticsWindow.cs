// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.Client._CCM.UserInterface.Controls;
using Content.Client.Players.PlayTimeTracking;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Shared._CCM.Stats;
using Content.Shared._RMC14.CCVar;
using Content.Shared.Localizations;
using Content.Shared.Roles;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.IoC;
using Robust.Shared.Input;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._CCM.Stats;

public sealed partial class CCMStatisticsWindow : DefaultCMWindow
{
    private const float LiveRefreshIntervalSeconds = 2.5f;

    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly JobRequirementsManager _jobRequirementsManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private enum StatsTab
    {
        General,
        Marines,
        Xenos,
        Playtime,
    }

    private enum PlaytimeCategory
    {
        Overall = 0,
        Xeno = 1,
        Marines = 2,
        Survivors = 3,
    }

    private readonly CCMStatsSystem _statsSystem;
    private readonly Button _generalButton;
    private readonly Button _marineButton;
    private readonly Button _xenoButton;
    private readonly Button _playtimeButton;
    private readonly CCMOptionButton _playtimeCategorySelector;
    private readonly Button[] _tabButtons;
    private readonly BoxContainer _content;
    private readonly Label _headerLabel;
    private readonly Font _windowTitleFont;
    private readonly Font _headerFont;
    private readonly Font _sectionFont;
    private readonly Font _rowFont;

    private bool _dragging;
    private Vector2 _dragOffset;
    private float _refreshTimer;

    private CCMPlayerStatsSnapshot _stats = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    private StatsTab _tab = StatsTab.General;
    private PlaytimeCategory _playtimeCategory = PlaytimeCategory.Overall;

    public CCMStatisticsWindow()
    {
        IoCManager.InjectDependencies(this);
        Stylesheet = IoCManager.Resolve<IStylesheetManager>().SheetNano;
        _statsSystem = _entManager.System<CCMStatsSystem>();
        _windowTitleFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 16);
        _headerFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 20);
        _sectionFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 14);
        _rowFont = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 12);

        Title = string.Empty;
        MinSize = new Vector2(640, 700);
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
            Text = Loc.GetString("ccm-stats-header"),
            FontColorOverride = StyleNano.LobbyMenuButtonBase,
            FontOverride = _headerFont,
            HorizontalAlignment = HAlignment.Center,
        };
        root.AddChild(_headerLabel);

        var tabBar = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
        };

        _generalButton = MakeTabButton("ccm-stats-tab-general", StatsTab.General);
        _marineButton = MakeTabButton("ccm-stats-tab-marines", StatsTab.Marines);
        _xenoButton = MakeTabButton("ccm-stats-tab-xenos", StatsTab.Xenos);
        _playtimeButton = MakeTabButton("ccm-stats-tab-playtime", StatsTab.Playtime);
        _tabButtons = new[] { _generalButton, _marineButton, _xenoButton, _playtimeButton };

        tabBar.AddChild(_generalButton);
        tabBar.AddChild(_marineButton);
        tabBar.AddChild(_xenoButton);
        tabBar.AddChild(_playtimeButton);
        root.AddChild(tabBar);

        _playtimeCategorySelector = new CCMOptionButton
        {
            HorizontalExpand = true,
            Visible = false,
        };
        _playtimeCategorySelector.AddItem(Loc.GetString("ui-playtime-category-overall"), (int) PlaytimeCategory.Overall);
        _playtimeCategorySelector.AddItem(Loc.GetString("ui-playtime-category-xeno"), (int) PlaytimeCategory.Xeno);
        _playtimeCategorySelector.AddItem(Loc.GetString("ui-playtime-category-marines"), (int) PlaytimeCategory.Marines);
        _playtimeCategorySelector.AddItem(Loc.GetString("ui-playtime-category-survivors"), (int) PlaytimeCategory.Survivors);
        _playtimeCategorySelector.SelectId((int) _playtimeCategory);
        _playtimeCategorySelector.OnItemSelected += args =>
        {
            _playtimeCategory = (PlaytimeCategory) args.Id;
            args.Button.SelectId(args.Id);
            if (_tab == StatsTab.Playtime)
                Rebuild();
        };
        AttachInteractiveStyle(_playtimeCategorySelector);
        root.AddChild(_playtimeCategorySelector);

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
            Margin = new Thickness(0, 0, 10, 0),
        };
        scroll.AddChild(_content);
        root.AddChild(scroll);

        Contents.AddChild(root);
        OnKeyBindDown += StartDrag;
        OnKeyBindUp += StopDrag;

        _statsSystem.PlayerStatsReceived += OnPlayerStatsReceived;
        _jobRequirementsManager.Updated += OnJobRequirementsUpdated;
        OnClose += () =>
        {
            _statsSystem.PlayerStatsReceived -= OnPlayerStatsReceived;
            _jobRequirementsManager.Updated -= OnJobRequirementsUpdated;
            _config.UnsubValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged);
            _config.UnsubValueChanged(RMCCVars.RMCLobbyUiStyle, OnThemeChanged);
            OnKeyBindDown -= StartDrag;
            OnKeyBindUp -= StopDrag;
        };

        _config.OnValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged, false);
        _config.OnValueChanged(RMCCVars.RMCLobbyUiStyle, OnThemeChanged, false);
        Rebuild();
    }

    private void OnThemeChanged(string _)
    {
        ApplyWindowTheme();
        Rebuild();
    }

    public void RefreshData()
    {
        _jobRequirementsManager.RequestSponsorshipStatus();
        _statsSystem.RequestPlayerStats();
        _refreshTimer = LiveRefreshIntervalSeconds;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!IsOpen)
            return;

        _refreshTimer -= args.DeltaSeconds;
        if (_refreshTimer > 0f)
            return;

        RefreshData();
    }

    private void OnPlayerStatsReceived(CCMPlayerStatsSnapshot stats)
    {
        _stats = stats;
        Rebuild();
    }

    private void OnJobRequirementsUpdated()
    {
        Rebuild();
    }

    private Button MakeTabButton(string textKey, StatsTab tab)
    {
        var button = new Button
        {
            Text = Loc.GetString(textKey),
            HorizontalExpand = true,
            MinSize = new Vector2(0, 34),
        };

        button.OnPressed += _ =>
        {
            _tab = tab;
            Rebuild();
        };

        AttachInteractiveStyle(button);
        return button;
    }

    private void Rebuild()
    {
        _content.DisposeAllChildren();
        ApplyTabState();

        foreach (var section in BuildSections())
        {
            _content.AddChild(BuildSectionHeader(section.Title));

            var rowIndex = 0;
            foreach (var (name, value) in section.Rows)
            {
                _content.AddChild(BuildStatRow(name, value, rowIndex % 2 == 0));
                rowIndex++;
            }
        }
    }

    private void ApplyTabState()
    {
        ApplyTabButtonStyle(_generalButton, _tab == StatsTab.General);
        ApplyTabButtonStyle(_marineButton, _tab == StatsTab.Marines);
        ApplyTabButtonStyle(_xenoButton, _tab == StatsTab.Xenos);
        ApplyTabButtonStyle(_playtimeButton, _tab == StatsTab.Playtime);
        _playtimeCategorySelector.Visible = _tab == StatsTab.Playtime;
        ApplyOptionButtonStyle(_playtimeCategorySelector);
    }

    private static void ApplyTabButtonStyle(Button button, bool selected)
    {
        button.ModulateSelfOverride = Color.White;
        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = selected
                ? StyleNano.LobbyMenuButtonBase
                : StyleNano.ButtonColorContext.WithAlpha(0.92f),
            BorderColor = selected
                ? StyleNano.LobbyMenuButtonBase
                : StyleNano.LobbyMenuButtonBase.WithAlpha(0.55f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 6,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 6,
            ContentMarginBottomOverride = 4,
        };
        button.Label.FontColorOverride = selected ? Color.Black : Color.White;
        button.Label.FontOverride = IoCManager.Resolve<IResourceCache>().GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
    }

    private static void ApplyTabButtonState(Button button, bool selected, bool pressed)
    {
        if (button.Disabled)
        {
            button.StyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.18f),
                BorderColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.24f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 6,
                ContentMarginTopOverride = 4,
                ContentMarginRightOverride = 6,
                ContentMarginBottomOverride = 4,
            };
            button.Label.FontColorOverride = Color.FromHex("#76808C");
            button.Label.FontOverride = IoCManager.Resolve<IResourceCache>().GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
            return;
        }

        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = selected
                ? (pressed ? StyleNano.ButtonColorContextHover.WithAlpha(0.98f) : StyleNano.LobbyMenuButtonBase)
                : pressed
                    ? StyleNano.LobbyMenuButtonBase.WithAlpha(0.92f)
                    : StyleNano.ButtonColorContextHover.WithAlpha(0.95f),
            BorderColor = selected || pressed
                ? StyleNano.LobbyMenuButtonBase
                : StyleNano.LobbyMenuButtonBase.WithAlpha(0.75f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 6,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 6,
            ContentMarginBottomOverride = 4,
        };
        button.Label.FontColorOverride = Color.White;
        button.Label.FontOverride = IoCManager.Resolve<IResourceCache>().GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
    }

    private static void ApplyOptionButtonStyle(OptionButton button, bool pressed = false)
    {
        button.ModulateSelfOverride = Color.White;
        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = pressed
                ? StyleNano.LobbyMenuButtonBase.WithAlpha(0.92f)
                : StyleNano.ButtonColorContext.WithAlpha(0.92f),
            BorderColor = pressed
                ? StyleNano.LobbyMenuButtonBase
                : StyleNano.LobbyMenuButtonBase.WithAlpha(0.55f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 6,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 6,
            ContentMarginBottomOverride = 4,
        };
    }

    private IEnumerable<StatsSection> BuildSections()
    {
        return _tab switch
        {
            StatsTab.Marines => BuildMarineSections(),
            StatsTab.Xenos => BuildXenoSections(),
            StatsTab.Playtime => BuildPlaytimeSections(),
            _ => BuildGeneralSections(),
        };
    }

    private IEnumerable<StatsSection> BuildGeneralSections()
    {
        yield return new StatsSection(
            Loc.GetString("ccm-stats-section-overview"),
            new List<(string, string)>
            {
                (Loc.GetString("ccm-stats-rounds-played"), _stats.RoundsPlayed.ToString()),
                (Loc.GetString("ccm-stats-rounds-won"), _stats.RoundsWon.ToString()),
                (Loc.GetString("ccm-stats-rounds-lost"), _stats.RoundsLost.ToString()),
                (Loc.GetString("ccm-stats-winrate"), FormatPercent(_stats.RoundsWon, _stats.RoundsPlayed)),
                (Loc.GetString("ccm-stats-round-time"), FormatDuration(_stats.RoundSecondsPlayed)),
                (Loc.GetString("ccm-stats-avg-round-time"), FormatDurationPerRound(_stats.RoundSecondsPlayed, _stats.RoundsPlayed)),
            });

        yield return new StatsSection(
            Loc.GetString("ccm-stats-section-combat"),
            new List<(string, string)>
            {
                (Loc.GetString("ccm-stats-total-damage"), _stats.TotalDamageDealt.ToString()),
                (Loc.GetString("ccm-stats-total-kills"), _stats.TotalKills.ToString()),
                (Loc.GetString("ccm-stats-total-deaths"), _stats.Deaths.ToString()),
                (Loc.GetString("ccm-stats-shots-fired"), _stats.ShotsFired.ToString()),
                (Loc.GetString("ccm-stats-kd-ratio"), FormatRatio(_stats.TotalKills, _stats.Deaths)),
                (Loc.GetString("ccm-stats-damage-per-round"), FormatAverage(_stats.TotalDamageDealt, _stats.RoundsPlayed)),
                (Loc.GetString("ccm-stats-kills-per-round"), FormatAverage(_stats.TotalKills, _stats.RoundsPlayed)),
                (Loc.GetString("ccm-stats-shots-per-round"), FormatAverage(_stats.ShotsFired, _stats.RoundsPlayed)),
            });

        yield return new StatsSection(
            Loc.GetString("ccm-stats-section-contribution"),
            new List<(string, string)>
            {
                (Loc.GetString("ccm-stats-victory-points"), _stats.VictoryPoints.ToString()),
                (Loc.GetString("ccm-stats-impact-points"), _stats.ImpactPoints.ToString()),
                (Loc.GetString("ccm-stats-healing-done"), _stats.HealingDone.ToString()),
                (Loc.GetString("ccm-stats-structures-built"), _stats.StructuresBuilt.ToString()),
                (Loc.GetString("ccm-stats-score-per-round"), FormatAverage(_stats.VictoryPoints + _stats.TotalKills, _stats.RoundsPlayed)),
                (Loc.GetString("ccm-stats-impact-per-round"), FormatAverage(_stats.ImpactPoints, _stats.RoundsPlayed)),
            });
    }

    private IEnumerable<StatsSection> BuildMarineSections()
    {
        yield return new StatsSection(
            Loc.GetString("ccm-stats-section-overview"),
            new List<(string, string)>
            {
                (Loc.GetString("ccm-stats-rounds-played"), _stats.MarineRoundsPlayed.ToString()),
                (Loc.GetString("ccm-stats-rounds-won"), _stats.MarineRoundsWon.ToString()),
                (Loc.GetString("ccm-stats-rounds-lost"), _stats.MarineRoundsLost.ToString()),
                (Loc.GetString("ccm-stats-winrate"), FormatPercent(_stats.MarineRoundsWon, _stats.MarineRoundsPlayed)),
                (Loc.GetString("ccm-stats-side-share"), FormatPercent(_stats.MarineRoundsPlayed, _stats.RoundsPlayed)),
            });

        yield return new StatsSection(
            Loc.GetString("ccm-stats-section-combat"),
            new List<(string, string)>
            {
                (Loc.GetString("ccm-stats-marine-damage"), _stats.MarineDamageDealt.ToString()),
                (Loc.GetString("ccm-stats-marine-kills"), _stats.MarineKills.ToString()),
                (Loc.GetString("ccm-stats-total-deaths"), _stats.MarineDeaths.ToString()),
                (Loc.GetString("ccm-stats-shots-fired"), _stats.MarineShotsFired.ToString()),
                (Loc.GetString("ccm-stats-kd-ratio"), FormatRatio(_stats.MarineKills, _stats.MarineDeaths)),
                (Loc.GetString("ccm-stats-damage-per-round"), FormatAverage(_stats.MarineDamageDealt, _stats.MarineRoundsPlayed)),
                (Loc.GetString("ccm-stats-kills-per-round"), FormatAverage(_stats.MarineKills, _stats.MarineRoundsPlayed)),
                (Loc.GetString("ccm-stats-shots-per-round"), FormatAverage(_stats.MarineShotsFired, _stats.MarineRoundsPlayed)),
            });

        yield return new StatsSection(
            Loc.GetString("ccm-stats-section-support"),
            new List<(string, string)>
            {
                (Loc.GetString("ccm-stats-victory-points"), _stats.MarineVictoryPoints.ToString()),
                (Loc.GetString("ccm-stats-impact-points"), _stats.MarineImpactPoints.ToString()),
                (Loc.GetString("ccm-stats-marine-healing"), _stats.MarineHealingDone.ToString()),
                (Loc.GetString("ccm-stats-marine-structures"), _stats.MarineStructuresBuilt.ToString()),
                (Loc.GetString("ccm-stats-score-per-round"), FormatAverage(_stats.MarineVictoryPoints + _stats.MarineKills, _stats.MarineRoundsPlayed)),
                (Loc.GetString("ccm-stats-impact-per-round"), FormatAverage(_stats.MarineImpactPoints, _stats.MarineRoundsPlayed)),
            });
    }

    private IEnumerable<StatsSection> BuildXenoSections()
    {
        yield return new StatsSection(
            Loc.GetString("ccm-stats-section-overview"),
            new List<(string, string)>
            {
                (Loc.GetString("ccm-stats-rounds-played"), _stats.XenoRoundsPlayed.ToString()),
                (Loc.GetString("ccm-stats-rounds-won"), _stats.XenoRoundsWon.ToString()),
                (Loc.GetString("ccm-stats-rounds-lost"), _stats.XenoRoundsLost.ToString()),
                (Loc.GetString("ccm-stats-winrate"), FormatPercent(_stats.XenoRoundsWon, _stats.XenoRoundsPlayed)),
                (Loc.GetString("ccm-stats-side-share"), FormatPercent(_stats.XenoRoundsPlayed, _stats.RoundsPlayed)),
            });

        yield return new StatsSection(
            Loc.GetString("ccm-stats-section-combat"),
            new List<(string, string)>
            {
                (Loc.GetString("ccm-stats-xeno-damage"), _stats.XenoDamageDealt.ToString()),
                (Loc.GetString("ccm-stats-xeno-kills"), _stats.XenoKills.ToString()),
                (Loc.GetString("ccm-stats-total-deaths"), _stats.XenoDeaths.ToString()),
                (Loc.GetString("ccm-stats-shots-fired"), _stats.XenoShotsFired.ToString()),
                (Loc.GetString("ccm-stats-kd-ratio"), FormatRatio(_stats.XenoKills, _stats.XenoDeaths)),
                (Loc.GetString("ccm-stats-damage-per-round"), FormatAverage(_stats.XenoDamageDealt, _stats.XenoRoundsPlayed)),
                (Loc.GetString("ccm-stats-kills-per-round"), FormatAverage(_stats.XenoKills, _stats.XenoRoundsPlayed)),
                (Loc.GetString("ccm-stats-shots-per-round"), FormatAverage(_stats.XenoShotsFired, _stats.XenoRoundsPlayed)),
            });

        yield return new StatsSection(
            Loc.GetString("ccm-stats-section-support"),
            new List<(string, string)>
            {
                (Loc.GetString("ccm-stats-victory-points"), _stats.XenoVictoryPoints.ToString()),
                (Loc.GetString("ccm-stats-impact-points"), _stats.XenoImpactPoints.ToString()),
                (Loc.GetString("ccm-stats-xeno-healing"), _stats.XenoHealingDone.ToString()),
                (Loc.GetString("ccm-stats-xeno-structures"), _stats.XenoStructuresBuilt.ToString()),
                (Loc.GetString("ccm-stats-score-per-round"), FormatAverage(_stats.XenoVictoryPoints + _stats.XenoKills, _stats.XenoRoundsPlayed)),
                (Loc.GetString("ccm-stats-impact-per-round"), FormatAverage(_stats.XenoImpactPoints, _stats.XenoRoundsPlayed)),
            });
    }

    private IEnumerable<StatsSection> BuildPlaytimeSections()
    {
        var showAllRoleTimers = _jobRequirementsManager.HasUnlockedAllRoleTimers();
        var rolePlaytimes = FilterRolePlaytimes(_jobRequirementsManager.FetchPlaytimeJobIdByRoles())
            .Where(kvp => showAllRoleTimers || kvp.Value > TimeSpan.Zero)
            .OrderByDescending(kvp => kvp.Value)
            .ToList();

        var totalPlaytime = rolePlaytimes.Aggregate(TimeSpan.Zero, (acc, next) => acc + next.Value);
        var topRole = rolePlaytimes.FirstOrDefault(kvp => kvp.Value > TimeSpan.Zero);

        yield return new StatsSection(
            Loc.GetString("ccm-stats-section-playtime-overview"),
            new List<(string, string)>
            {
                (Loc.GetString("ccm-stats-playtime-total"), ContentLocalizationManager.FormatPlaytime(totalPlaytime)),
                (Loc.GetString("ccm-stats-playtime-tracked-roles"), rolePlaytimes.Count.ToString()),
                (Loc.GetString("ccm-stats-playtime-top-role"), string.IsNullOrEmpty(topRole.Key) ? "-" : GetLocalizedJobName(topRole.Key)),
                (Loc.GetString("ccm-stats-playtime-top-role-time"), string.IsNullOrEmpty(topRole.Key) ? "0m 0s" : ContentLocalizationManager.FormatPlaytime(topRole.Value)),
            });

        var rows = new List<(string, string)>();
        if (rolePlaytimes.Count == 0)
        {
            rows.Add((Loc.GetString("ccm-stats-playtime-empty"), "-"));
        }
        else
        {
            foreach (var (jobId, playtime) in rolePlaytimes)
            {
                rows.Add((GetLocalizedJobName(jobId), ContentLocalizationManager.FormatPlaytime(playtime)));
            }
        }

        yield return new StatsSection(Loc.GetString("ccm-stats-section-playtime-roles"), rows);
    }

    private IEnumerable<KeyValuePair<string, TimeSpan>> FilterRolePlaytimes(IEnumerable<KeyValuePair<string, TimeSpan>> rolePlaytimes)
    {
        if (_playtimeCategory == PlaytimeCategory.Overall)
            return rolePlaytimes;

        var departmentId = _playtimeCategory switch
        {
            PlaytimeCategory.Xeno => "CMXeno",
            PlaytimeCategory.Marines => "CMSquad",
            PlaytimeCategory.Survivors => "CMSurvivor",
            _ => string.Empty,
        };

        if (string.IsNullOrEmpty(departmentId))
            return rolePlaytimes;

        return rolePlaytimes.Where(kvp => JobInDepartment(kvp.Key, departmentId));
    }

    private bool JobInDepartment(string jobId, string departmentId)
    {
        if (!_prototypeManager.TryIndex<JobPrototype>(jobId, out var job))
            return false;

        foreach (var department in _prototypeManager.EnumeratePrototypes<DepartmentPrototype>())
        {
            if (department.ID == departmentId && department.Roles.Contains(job.ID))
                return true;
        }

        return false;
    }

    private string GetLocalizedJobName(string jobId)
    {
        if (_prototypeManager.TryIndex<JobPrototype>(jobId, out var job))
            return job.LocalizedName;

        return jobId;
    }

    private static Control BuildSectionHeader(string title)
    {
        var panel = new PanelContainer
        {
            Margin = new Thickness(0, 4, 0, 2),
            MouseFilter = MouseFilterMode.Stop,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.35f),
                BorderColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.75f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 8,
                ContentMarginTopOverride = 4,
                ContentMarginRightOverride = 8,
                ContentMarginBottomOverride = 4,
            },
        };

        panel.AddChild(new Label
        {
            Text = title,
            FontColorOverride = StyleNano.LobbyMenuButtonBase,
            FontOverride = IoCManager.Resolve<IResourceCache>().GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 14),
        });

        return panel;
    }

    private static Control BuildStatRow(string name, string value, bool evenRow)
    {
        var backgroundColor = evenRow
            ? Color.Black.WithAlpha(0.18f)
            : StyleNano.ButtonColorContext.WithAlpha(0.22f);
        var borderColor = evenRow
            ? StyleNano.LobbyMenuButtonBase.WithAlpha(0.22f)
            : StyleNano.LobbyMenuButtonBase.WithAlpha(0.30f);

        var panel = new PanelContainer
        {
            MouseFilter = MouseFilterMode.Stop,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = backgroundColor,
                BorderColor = borderColor,
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 8,
                ContentMarginTopOverride = 5,
                ContentMarginRightOverride = 8,
                ContentMarginBottomOverride = 5,
            },
        };

        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
        };

        row.AddChild(new Label
        {
            Text = name,
            HorizontalExpand = true,
            FontOverride = IoCManager.Resolve<IResourceCache>().GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 12),
        });

        row.AddChild(new Label
        {
            Text = value,
            HorizontalAlignment = HAlignment.Right,
            FontColorOverride = Color.White,
            FontOverride = IoCManager.Resolve<IResourceCache>().GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 12),
        });

        panel.AddChild(row);
        return panel;
    }

    private static string FormatPercent(int numerator, int denominator)
    {
        if (denominator <= 0)
            return "0%";

        return $"{MathF.Round(numerator * 100f / denominator)}%";
    }

    private static string FormatRatio(int numerator, int denominator)
    {
        if (denominator <= 0)
            return numerator > 0 ? numerator.ToString() : "0";

        return $"{MathF.Round(numerator / (float) denominator, 2):0.##}";
    }

    private static string FormatAverage(int total, int count)
    {
        if (count <= 0)
            return "0";

        return $"{MathF.Round(total / (float) count, 1):0.#}";
    }

    private static string FormatDuration(int totalSeconds)
    {
        var time = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
        if (time.TotalHours >= 1)
            return $"{(int) time.TotalHours}h {time.Minutes}m";

        return $"{time.Minutes}m {time.Seconds}s";
    }

    private static string FormatDurationPerRound(int totalSeconds, int rounds)
    {
        if (rounds <= 0)
            return "0m 0s";

        return FormatDuration((int) MathF.Round(totalSeconds / (float) rounds));
    }

    private sealed record StatsSection(string Title, List<(string Name, string Value)> Rows);

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

    protected override void EnteredTree()
    {
        base.EnteredTree();
        ApplyWindowTheme();
    }

    private void ApplyWindowTheme()
    {
        var theme = StyleNano.GetConfiguredTheme(_config);
        var bodyColor = theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#1A2028").WithAlpha(0.94f),
            _ => Color.FromHex("#05180A").WithAlpha(0.94f),
        };
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
        _headerLabel.FontColorOverride = StyleNano.LobbyMenuButtonBase;
        WindowTitleLabel.FontColorOverride = Color.White;
        foreach (var button in _tabButtons)
        {
            ApplyTabButtonStyle(button, button == GetSelectedTabButton());
        }

        ApplyOptionButtonStyle(_playtimeCategorySelector);
    }

    private Button GetSelectedTabButton()
    {
        return _tab switch
        {
            StatsTab.Marines => _marineButton,
            StatsTab.Xenos => _xenoButton,
            StatsTab.Playtime => _playtimeButton,
            _ => _generalButton,
        };
    }

    private void AttachInteractiveStyle(Button button)
    {
        button.OnMouseEntered += _ => ApplyTabButtonState(button, button == GetSelectedTabButton(), false);
        button.OnMouseExited += _ => ApplyTabButtonStyle(button, button == GetSelectedTabButton());
        button.OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyTabButtonState(button, button == GetSelectedTabButton(), true);
        };
        button.OnKeyBindUp += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyTabButtonStyle(button, button == GetSelectedTabButton());
        };
    }

    private void AttachInteractiveStyle(OptionButton button)
    {
        button.OnMouseEntered += _ => ApplyOptionButtonStyle(button);
        button.OnMouseExited += _ => ApplyOptionButtonStyle(button);
        button.OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyOptionButtonStyle(button, true);
        };
        button.OnKeyBindUp += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyOptionButtonStyle(button);
        };
    }
}
