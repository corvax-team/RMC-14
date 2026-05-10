// CM14 rework: non-RMC edit marker.
using System.Linq;
using System.Numerics;
using Content.Client.GameTicking.Managers;
using Content.Client.Lobby;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Client.Players.PlayTimeTracking;
using Content.Client.UserInterface.Controls;
using Content.Shared._RMC14.Prototypes;
using Content.Shared.CCVar;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Robust.Client.Console;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.LateJoin
{
    public readonly record struct LateJoinPalette(
        Color HeaderBackground,
        Color HeaderBorder,
        Color HeaderText,
        Color HeaderSubText,
        Color CardBackground,
        Color CardBorder,
        Color InnerBackground,
        Color InnerBorder,
        Color SectionBackground,
        Color SectionBorder,
        Color SectionText,
        Color BadgeBackground,
        Color BadgeBorder,
        Color BadgeText,
        Color IconTint,
        Color EmptyText);

    public sealed class LateJoinGui : DefaultCMWindow
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IClientConsoleHost _consoleHost = default!;
        [Dependency] private readonly IConfigurationManager _configManager = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
        [Dependency] private readonly JobRequirementsManager _jobRequirements = default!;
        [Dependency] private readonly IClientPreferencesManager _preferencesManager = default!;
        [Dependency] private readonly ILogManager _logManager = default!;

        public event Action<(NetEntity, string)> SelectedId;

        private readonly ClientGameTicker _gameTicker;
        private readonly SpriteSystem _sprites;
        private readonly ISawmill _sawmill;

        private readonly Dictionary<NetEntity, Dictionary<string, List<JobButton>>> _jobButtons = new();
        private readonly Dictionary<NetEntity, Dictionary<string, BoxContainer>> _jobCategories = new();
        private readonly List<Control> _stationSections = new();

        private readonly Control _base;
        private LateJoinPalette _palette;

        public LateJoinGui()
        {
            MinSize = SetSize = new Vector2(700, 780);
            IoCManager.InjectDependencies(this);
            _sprites = _entitySystem.GetEntitySystem<SpriteSystem>();
            _gameTicker = _entitySystem.GetEntitySystem<ClientGameTicker>();
            _sawmill = _logManager.GetSawmill("latejoin.panel");
            _palette = GetPalette();

            Title = Loc.GetString("late-join-gui-title");

            _base = new BoxContainer()
            {
                Orientation = LayoutOrientation.Vertical,
                VerticalExpand = true,
                SeparationOverride = 8,
                Margin = new Thickness(8),
            };

            Contents.AddChild(_base);

            _jobRequirements.Updated += RebuildUI;
            RebuildUI();

            SelectedId += x =>
            {
                var (station, jobId) = x;
                _sawmill.Info($"Late joining as ID: {jobId}");
                _consoleHost.ExecuteCommand($"joingame {CommandParsing.Escape(jobId)} {station}");
                Close();
            };

            _gameTicker.LobbyJobsAvailableUpdated += JobsAvailableUpdated;
        }

        private void RebuildUI()
        {
            _base.RemoveAllChildren();
            _stationSections.Clear();
            _jobButtons.Clear();
            _jobCategories.Clear();
            _palette = GetPalette();

            if (!_gameTicker.DisallowedLateJoin && _gameTicker.StationNames.Count == 0)
                _sawmill.Warning("No stations exist, nothing to display in late-join GUI");

            var showStationTabs = _gameTicker.StationNames.Count > 1;
            BoxContainer? stationTabs = null;
            if (showStationTabs)
            {
                stationTabs = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                    SeparationOverride = 6,
                    HorizontalExpand = true,
                };
                _base.AddChild(stationTabs);
            }

            var firstStation = true;
            foreach (var (id, name) in _gameTicker.StationNames)
            {
                var jobList = new BoxContainer
                {
                    Orientation = LayoutOrientation.Vertical,
                    SeparationOverride = 6,
                    HorizontalExpand = true,
                };
                _jobCategories[id] = new Dictionary<string, BoxContainer>();

                var stationBody = new PanelContainer
                {
                    VerticalExpand = true,
                    Visible = firstStation,
                    PanelOverride = new StyleBoxFlat
                    {
                        BackgroundColor = _palette.CardBackground,
                        BorderColor = _palette.CardBorder,
                        BorderThickness = new Thickness(1),
                    },
                };

                var stationStack = new BoxContainer
                {
                    Orientation = LayoutOrientation.Vertical,
                    SeparationOverride = 8,
                    VerticalExpand = true,
                    Margin = new Thickness(8),
                };
                stationBody.AddChild(stationStack);
                stationStack.AddChild(BuildStationHeader(name));

                var listShell = new PanelContainer
                {
                    VerticalExpand = true,
                    PanelOverride = new StyleBoxFlat
                    {
                        BackgroundColor = _palette.InnerBackground,
                        BorderColor = _palette.InnerBorder,
                        BorderThickness = new Thickness(1),
                    },
                };
                var jobListScroll = new ScrollContainer
                {
                    VerticalExpand = true,
                    HScrollEnabled = false,
                    Margin = new Thickness(6),
                    Children = { jobList },
                };
                listShell.AddChild(jobListScroll);
                stationStack.AddChild(listShell);

                _stationSections.Add(stationBody);
                _base.AddChild(stationBody);

                if (stationTabs != null)
                {
                    var tabButton = new Button
                    {
                        Text = name,
                        ToggleMode = true,
                        Pressed = firstStation,
                        HorizontalExpand = true,
                        MinHeight = 34,
                    };
                    tabButton.OnPressed += _ => ShowStationSection(stationBody, tabButton, stationTabs);
                    stationTabs.AddChild(tabButton);
                }

                var firstCategory = true;
                var departments = _prototypeManager.EnumerateCM<DepartmentPrototype>().ToArray();
                Array.Sort(departments, DepartmentUIComparer.Instance);

                // Keep the main marine department at the top of the late-join list.
                var marineDepartmentIndex = Array.FindIndex(departments, department => department.ID == "CMSquad");
                if (marineDepartmentIndex > 0)
                {
                    (departments[0], departments[marineDepartmentIndex]) =
                        (departments[marineDepartmentIndex], departments[0]);
                }

                _jobButtons[id] = new Dictionary<string, List<JobButton>>();

                foreach (var department in departments)
                {
                    var departmentName = Loc.GetString(department.Name);
                    var stationAvailable = _gameTicker.JobsAvailable[id];
                    var jobsAvailable = new List<JobPrototype>();

                    foreach (var jobId in department.Roles)
                    {
                        if (!stationAvailable.ContainsKey(jobId))
                            continue;

                        jobsAvailable.Add(_prototypeManager.Index<JobPrototype>(jobId));
                    }

                    jobsAvailable.Sort(JobUIComparer.Instance);

                    // Do not display departments with no jobs available.
                    if (jobsAvailable.Count == 0)
                        continue;

                    var category = new BoxContainer
                    {
                        Orientation = LayoutOrientation.Vertical,
                        Name = department.ID,
                        ToolTip = Loc.GetString("late-join-gui-jobs-amount-in-department-tooltip",
                            ("departmentName", departmentName)),
                        SeparationOverride = 4,
                    };

                    if (!firstCategory)
                    {
                        category.AddChild(new Control
                        {
                            MinSize = new Vector2(0, 6),
                        });
                    }
                    firstCategory = false;

                    category.AddChild(new PanelContainer
                    {
                        PanelOverride = new StyleBoxFlat
                        {
                            BackgroundColor = _palette.SectionBackground,
                            BorderColor = _palette.SectionBorder,
                            BorderThickness = new Thickness(1),
                        },
                        Children =
                        {
                            new Label
                            {
                                StyleClasses = { "LabelBig" },
                                Text = Loc.GetString("late-join-gui-department-jobs-label", ("departmentName", departmentName)),
                                Margin = new Thickness(8, 4),
                                FontColorOverride = _palette.SectionText,
                            }
                        }
                    });

                    _jobCategories[id][department.ID] = category;
                    jobList.AddChild(category);

                    foreach (var prototype in jobsAvailable)
                    {
                        var value = stationAvailable[prototype.ID];

                        var jobIcon = _prototypeManager.Index(prototype.Icon);
                        var jobButton = new JobButton(_sprites.Frame0(jobIcon.Icon), prototype.ID, prototype.LocalizedName, value, _palette);
                        jobButton.MinSize = new Vector2(0, 32);
                        jobButton.HorizontalExpand = true;
                        jobButton.Margin = new Thickness(0, 0, 0, 2);
                        category.AddChild(jobButton);

                        jobButton.OnPressed += _ => SelectedId.Invoke((id, jobButton.JobId));

                        var selectedProfile = _preferencesManager.Preferences is { } prefs &&
                            prefs.TryGetSelectedCharacter(out var selectedCharacter)
                            ? selectedCharacter as HumanoidCharacterProfile
                            : null;

                        if (!_jobRequirements.IsAllowed(prototype, selectedProfile, out var reason))
                        {
                            jobButton.Disabled = true;

                            if (!reason.IsEmpty)
                            {
                                var tooltip = new Tooltip();
                                tooltip.SetMessage(reason);
                                jobButton.TooltipSupplier = _ => tooltip;
                            }
                            jobButton.SetLocked(_sprites.Frame0(new SpriteSpecifier.Texture(new("/Textures/Interface/Nano/lock.svg.192dpi.png"))));
                        }
                        else if (value == 0)
                        {
                            jobButton.Disabled = true;
                        }

                        if (!_jobButtons[id].ContainsKey(prototype.ID))
                        {
                            _jobButtons[id][prototype.ID] = new List<JobButton>();
                        }

                        _jobButtons[id][prototype.ID].Add(jobButton);
                    }
                }

                firstStation = false;
            }
        }

        private void ShowStationSection(Control visibleSection, Button pressedButton, BoxContainer stationTabs)
        {
            foreach (var section in _stationSections)
            {
                section.Visible = section == visibleSection;
            }

            foreach (var child in stationTabs.Children)
            {
                if (child is Button button)
                    button.Pressed = button == pressedButton;
            }
        }

        private Control BuildStationHeader(string stationName)
        {
            var panel = new PanelContainer
            {
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = _palette.SectionBackground.WithAlpha(0.82f),
                    BorderColor = _palette.SectionBorder,
                    BorderThickness = new Thickness(1),
                },
            };

            panel.AddChild(new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Margin = new Thickness(8, 6),
                Children =
                {
                    new Label
                    {
                        Text = stationName,
                        StyleClasses = { "LabelBig" },
                        FontColorOverride = _palette.HeaderText,
                    },
                    new Label
                    {
                        Text = Loc.GetString("late-join-gui-jobs-amount-in-department-tooltip", ("departmentName", stationName)),
                        FontColorOverride = _palette.HeaderSubText,
                    },
                },
            });

            return panel;
        }

        private static LateJoinPalette GetPalette()
        {
            return StyleNano.CurrentTheme switch
            {
                StyleNano.UiColorTheme.Gray => new LateJoinPalette(
                    Color.FromHex("#323B47").WithAlpha(0.96f),
                    Color.FromHex("#738396").WithAlpha(0.95f),
                    Color.FromHex("#F2F6FA"),
                    Color.FromHex("#C7D0DA"),
                    Color.FromHex("#181D24").WithAlpha(0.96f),
                    Color.FromHex("#4C5A6B").WithAlpha(0.95f),
                    Color.FromHex("#1F252D").WithAlpha(0.92f),
                    Color.FromHex("#637184").WithAlpha(0.90f),
                    Color.FromHex("#36414D").WithAlpha(0.94f),
                    Color.FromHex("#8191A3").WithAlpha(0.95f),
                    Color.FromHex("#F2F5F8"),
                    Color.FromHex("#566374").WithAlpha(0.98f),
                    Color.FromHex("#AEB9C7").WithAlpha(0.98f),
                    Color.White,
                    Color.FromHex("#E4EAF1"),
                    Color.FromHex("#B6C0CB")),
                _ => new LateJoinPalette(
                    Color.FromHex("#13371C").WithAlpha(0.96f),
                    Color.FromHex("#3E8050").WithAlpha(0.95f),
                    Color.FromHex("#EFFEF2"),
                    Color.FromHex("#C5E1CB"),
                    Color.FromHex("#08140B").WithAlpha(0.96f),
                    Color.FromHex("#2A5D36").WithAlpha(0.95f),
                    Color.FromHex("#0E1F12").WithAlpha(0.92f),
                    Color.FromHex("#356B42").WithAlpha(0.90f),
                    Color.FromHex("#184726").WithAlpha(0.94f),
                    Color.FromHex("#4D9962").WithAlpha(0.95f),
                    Color.FromHex("#F0FFF3"),
                    Color.FromHex("#2E7B42").WithAlpha(0.98f),
                    Color.FromHex("#8FE0A5").WithAlpha(0.98f),
                    Color.White,
                    Color.FromHex("#DDFBE4"),
                    Color.FromHex("#A3CFAC")),
            };
        }

        private void JobsAvailableUpdated(IReadOnlyDictionary<NetEntity, Dictionary<ProtoId<JobPrototype>, int?>> updatedJobs)
        {
            foreach (var stationEntries in updatedJobs)
            {
                if (_jobButtons.ContainsKey(stationEntries.Key))
                {
                    var jobsAvailable = stationEntries.Value;

                    var existingJobEntries = _jobButtons[stationEntries.Key];
                    foreach (var existingJobEntry in existingJobEntries)
                    {
                        if (jobsAvailable.ContainsKey(existingJobEntry.Key))
                        {
                            var updatedJobValue = jobsAvailable[existingJobEntry.Key];
                            foreach (var matchingJobButton in existingJobEntry.Value)
                            {
                                if (matchingJobButton.Amount != updatedJobValue)
                                {
                                    matchingJobButton.RefreshLabel(updatedJobValue);
                                    matchingJobButton.Disabled |= matchingJobButton.Amount == 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _jobRequirements.Updated -= RebuildUI;
                _gameTicker.LobbyJobsAvailableUpdated -= JobsAvailableUpdated;
                _jobButtons.Clear();
                _jobCategories.Clear();
            }
        }
    }

    sealed class JobButton : ContainerButton
    {
        public Label JobLabel { get; }
        public string JobId { get; }
        public string JobLocalisedName { get; }
        public int? Amount { get; private set; }
        private readonly Label _amountLabel;
        private readonly PanelContainer _amountBadge;
        private readonly TextureRect _statusTexture;
        private readonly LateJoinPalette _palette;
        private bool _initialised = false;

        public JobButton(Texture? iconTexture, ProtoId<JobPrototype> jobId, string jobLocalisedName, int? amount, LateJoinPalette palette)
        {
            _palette = palette;
            JobLabel = new Label
            {
                HorizontalExpand = true,
                ClipText = true,
                FontColorOverride = Color.White,
            };
            JobId = jobId;
            JobLocalisedName = jobLocalisedName;
            _amountLabel = new Label
            {
                HorizontalAlignment = HAlignment.Center,
                VerticalAlignment = VAlignment.Center,
                FontColorOverride = palette.BadgeText,
            };
            _amountBadge = new PanelContainer
            {
                MinWidth = 34,
                HorizontalAlignment = HAlignment.Right,
                VerticalAlignment = VAlignment.Center,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = palette.BadgeBackground,
                    BorderColor = palette.BadgeBorder,
                    BorderThickness = new Thickness(1),
                },
                Children =
                {
                    new BoxContainer
                    {
                        Orientation = LayoutOrientation.Vertical,
                        Margin = new Thickness(8, 2),
                        Children = { _amountLabel },
                    },
                },
            };
            _statusTexture = new TextureRect
            {
                Visible = false,
                HorizontalAlignment = HAlignment.Right,
                VerticalAlignment = VAlignment.Center,
                TextureScale = new Vector2(0.45f, 0.45f),
                Stretch = TextureRect.StretchMode.KeepCentered,
            };

            var icon = new TextureRect
            {
                Texture = iconTexture,
                TextureScale = new Vector2(2f, 2f),
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                ModulateSelfOverride = palette.IconTint,
            };

            var iconPanel = new PanelContainer
            {
                MinSize = new Vector2(28, 28),
                VerticalAlignment = VAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = palette.SectionBackground.WithAlpha(0.72f),
                    BorderColor = palette.SectionBorder.WithAlpha(0.92f),
                    BorderThickness = new Thickness(1),
                },
                Children = { icon },
            };

            AddChild(new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalExpand = true,
                Margin = new Thickness(4, 4, 6, 4),
                Children =
                {
                    iconPanel,
                    JobLabel,
                    _amountBadge,
                    _statusTexture,
                },
            });

            RefreshLabel(amount);
            AddStyleClass(StyleClassButton);
            _initialised = true;
        }

        public void SetLocked(Texture? texture)
        {
            _statusTexture.Texture = texture;
            _statusTexture.Visible = texture != null;
            _amountBadge.Visible = texture == null;
        }

        public void RefreshLabel(int? amount)
        {
            if (Amount == amount && _initialised)
            {
                return;
            }

            Amount = amount;
            JobLabel.Text = JobLocalisedName;
            JobLabel.FontColorOverride = amount == 0 ? _palette.EmptyText : Color.White;
            _amountLabel.Text = amount == null ? "∞" : amount.Value.ToString();
            _amountLabel.FontColorOverride = amount == 0 ? _palette.EmptyText : _palette.BadgeText;
            _amountBadge.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = amount == 0
                    ? _palette.BadgeBackground.WithAlpha(0.55f)
                    : _palette.BadgeBackground,
                BorderColor = amount == 0
                    ? _palette.BadgeBorder.WithAlpha(0.65f)
                    : _palette.BadgeBorder,
                BorderThickness = new Thickness(1),
            };
        }
    }
}

