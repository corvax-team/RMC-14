// CM14 rework: non-RMC edit marker.
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;
using Content.Client.Administration.UI.CustomControls;
using Content.Client._CCM.Stats;
using Content.Client.Message;
using Content.Client.Resources;
using Robust.Client.ResourceManagement;
using Content.Client.Stylesheets;
using Content.Shared._CCM.Sponsorship;
using Content.Shared._CCM.Stats;
using Content.Shared.GameTicking;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.RoundEnd
{
    public sealed class RoundEndSummaryWindow : DefaultCMWindow
    {
        private readonly IEntityManager _entityManager;
        private readonly CCMStatsSystem _ccmStatsSystem;
        private readonly string _gamemode;
        private readonly TimeSpan _roundDuration;
        private readonly string _roundEndWithoutSponsors;
        private readonly List<SponsorCreditEntry> _sponsorCredits;
        private readonly BoxContainer _roundEndSummaryTab;
        private readonly Font _mvpTitleFont;
        private readonly Font _mvpSubtitleFont;
        private readonly Font _sponsorTierThreeFont;
        private readonly Font _sponsorTierTwoFont;
        private readonly Font _sponsorTierOneFont;
        private readonly Font _sponsorTierThreeNameFont;
        private readonly Font _sponsorTierTwoNameFont;
        private readonly Font _sponsorTierOneNameFont;
        public int RoundId;

        public RoundEndSummaryWindow(string gm, string roundEnd, TimeSpan roundTimeSpan, int roundId,
            RoundEndMessageEvent.RoundEndPlayerInfo[] info, IEntityManager entityManager)
        {
            _entityManager = entityManager;
            _ccmStatsSystem = _entityManager.System<CCMStatsSystem>();
            _gamemode = gm;
            _roundDuration = roundTimeSpan;
            Stylesheet = IoCManager.Resolve<IStylesheetManager>().SheetNano;
            var resourceCache = IoCManager.Resolve<IResourceCache>();
            _mvpTitleFont = resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 14);
            _mvpSubtitleFont = resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 12);
            _sponsorTierThreeFont = resourceCache.GetFont("/Fonts/Bedstead/bedstead.otf", 22);
            _sponsorTierTwoFont = resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 18);
            _sponsorTierOneFont = resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 16);
            _sponsorTierThreeNameFont = resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 14);
            _sponsorTierTwoNameFont = resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
            _sponsorTierOneNameFont = resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 12);

            MinSize = new Vector2(660, 720);
            SetSize = new Vector2(660, 720);

            Title = Loc.GetString("round-end-summary-window-title");

            // The round end window is split into two tabs, one about the round stats
            // and the other is a list of RoundEndPlayerInfo for each player.
            // This tab would be a good place for things like: "x many people died.",
            // "clown slipped the crew x times.", "x shots were fired this round.", etc.
            // Also good for serious info.

            RoundId = roundId;
            _sponsorCredits = ExtractSponsorCredits(roundEnd, out _roundEndWithoutSponsors);
            var roundEndTabs = new TabContainer();
            _roundEndSummaryTab = MakeRoundEndSummaryTab();
            roundEndTabs.AddChild(_roundEndSummaryTab);
            roundEndTabs.AddChild(MakePlayerManifestTab(info));

            Contents.AddChild(roundEndTabs);
            _ccmStatsSystem.RoundEndStatsReceived += OnRoundEndStatsReceived;
            OnClose += () => _ccmStatsSystem.RoundEndStatsReceived -= OnRoundEndStatsReceived;

            OpenCenteredRight();
            MoveToFront();
        }

        private void OnRoundEndStatsReceived(CCMRoundEndStatsEvent ev)
        {
            if (ev.RoundId != RoundId)
                return;

            RebuildRoundEndSummaryTab();
        }

        private CCMRoundEndStatsEvent? GetRoundStats()
        {
            var roundStats = _ccmStatsSystem.LatestRoundEndStats;
            return roundStats?.RoundId == RoundId ? roundStats : null;
        }

        private BoxContainer MakeRoundEndSummaryTab()
        {
            var roundEndSummaryTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-round-end-summary-tab-title")
            };

            RebuildRoundEndSummaryTab(roundEndSummaryTab);
            return roundEndSummaryTab;
        }

        private void RebuildRoundEndSummaryTab()
        {
            RebuildRoundEndSummaryTab(_roundEndSummaryTab);
        }

        private void RebuildRoundEndSummaryTab(BoxContainer roundEndSummaryTab)
        {
            roundEndSummaryTab.DisposeAllChildren();

            var roundEndSummaryContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10),
                HScrollEnabled = false,
            };
            var roundEndSummaryContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 10,
            };

            var roundStats = GetRoundStats();
            var winningSide = roundStats?.WinningSide ?? CCMStatsSide.None;

            roundEndSummaryContainer.AddChild(BuildRoundInfoBlock(
                _gamemode,
                _roundEndWithoutSponsors,
                _roundDuration,
                RoundId,
                winningSide));

            if (roundStats != null)
            {
                roundEndSummaryContainer.AddChild(BuildCampaignScoreBlock(
                    roundStats.MarineCampaignWins,
                    roundStats.XenoCampaignWins,
                    winningSide));

                roundEndSummaryContainer.AddChild(BuildRoundScoreLabel(roundStats.PersonalScore));

                if (roundStats.PersonalStats != null)
                    roundEndSummaryContainer.AddChild(BuildPersonalStatsBlock(roundStats.PersonalStats));

                if (roundStats.MarineMvp != null)
                    roundEndSummaryContainer.AddChild(BuildMvpBlock(roundStats.MarineMvp));

                if (roundStats.XenoMvp != null)
                    roundEndSummaryContainer.AddChild(BuildMvpBlock(roundStats.XenoMvp));
            }

            if (_sponsorCredits.Count > 0)
                roundEndSummaryContainer.AddChild(BuildSponsorCreditsBlock(_sponsorCredits));

            roundEndSummaryContainerScrollbox.AddChild(roundEndSummaryContainer);
            roundEndSummaryTab.AddChild(roundEndSummaryContainerScrollbox);
        }

        private BoxContainer MakePlayerManifestTab(RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var playerManifestTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-player-manifest-tab-title")
            };

            var playerInfoContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10)
            };
            var playerInfoContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };

            //Put observers at the bottom of the list. Put antags on top.
            var sortedPlayersInfo = playersInfo.OrderBy(p => p.Observer).ThenBy(p => !p.Antag);

            //Create labels for each player info.
            foreach (var playerInfo in sortedPlayersInfo)
            {
                var hBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                    SeparationOverride = 8,
                    HorizontalExpand = true,
                };

                var playerInfoText = new RichTextLabel
                {
                    VerticalAlignment = VAlignment.Center,
                    VerticalExpand = true,
                    HorizontalExpand = true,
                };

                if (playerInfo.PlayerNetEntity != null)
                {
                    hBox.AddChild(new SpriteView(playerInfo.PlayerNetEntity.Value, _entityManager)
                        {
                            OverrideDirection = Direction.South,
                            VerticalAlignment = VAlignment.Center,
                            SetSize = new Vector2(32, 32),
                            VerticalExpand = true,
                        });
                }

                if (playerInfo.PlayerICName != null)
                {
                    if (playerInfo.Observer)
                    {
                        playerInfoText.SetMarkup(
                            NormalizeEndRoundMarkup(Loc.GetString("round-end-summary-window-player-info-if-observer-text",
                                ("playerOOCName", playerInfo.PlayerOOCName),
                                ("playerICName", playerInfo.PlayerICName))));
                    }
                    else
                    {
                        //TODO: On Hover display a popup detailing more play info.
                        //For example: their antag goals and if they completed them sucessfully.
                        var icNameColor = playerInfo.Antag
                            ? GetMarkupColorHex(GetAntagTextColor())
                            : GetMarkupColorHex(GetPrimaryBodyTextColor());
                        playerInfoText.SetMarkup(
                            NormalizeEndRoundMarkup(Loc.GetString("round-end-summary-window-player-info-if-not-observer-text",
                                ("playerOOCName", playerInfo.PlayerOOCName),
                                ("icNameColor", icNameColor),
                                ("playerICName", playerInfo.PlayerICName),
                                ("playerRole", Loc.GetString(playerInfo.Role)))));
                    }
                }
                hBox.AddChild(playerInfoText);

                playerInfoContainer.AddChild(new PanelContainer
                {
                    Margin = new Thickness(0, 0, 0, 6),
                    PanelOverride = new StyleBoxFlat
                    {
                        BackgroundColor = GetSummaryInsetBackground(IsOldLobbyPalette() ? 0.72f : 0.18f),
                        BorderColor = GetPrimaryAccentColor().WithAlpha(IsOldLobbyPalette() ? 0.62f : 0.26f),
                        BorderThickness = new Thickness(1),
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 6,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 6,
                    },
                    Children = { hBox },
                });
            }

            playerInfoContainerScrollbox.AddChild(playerInfoContainer);
            playerManifestTab.AddChild(playerInfoContainerScrollbox);

            return playerManifestTab;
        }

        private Control BuildRoundInfoBlock(
            string gamemode,
            string roundEnd,
            TimeSpan roundDuration,
            int roundId,
            CCMStatsSide winningSide)
        {
            var accent = GetPrimaryAccentColor();
            var winnerAccent = winningSide == CCMStatsSide.None
                ? accent
                : GetMvpAccentColor(winningSide);

            var panel = new PanelContainer
            {
                HorizontalExpand = true,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = GetSummaryPanelBackground(),
                    BorderColor = winnerAccent.WithAlpha(0.82f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 12,
                    ContentMarginTopOverride = 12,
                    ContentMarginRightOverride = 12,
                    ContentMarginBottomOverride = 12,
                },
            };

            var root = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 8,
                HorizontalExpand = true,
            };

            var header = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 8,
                HorizontalExpand = true,
            };

            header.AddChild(new Label
            {
                Text = Loc.GetString("ccm-round-end-info-title"),
                FontColorOverride = accent,
                FontOverride = _mvpTitleFont,
                HorizontalExpand = true,
            });

            if (winningSide != CCMStatsSide.None)
                header.AddChild(BuildWinnerBadge(winningSide));

            root.AddChild(header);

            root.AddChild(new PanelContainer
            {
                MinSize = new Vector2(0, 1),
                MaxSize = new Vector2(float.MaxValue, 1),
                HorizontalExpand = true,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = winnerAccent.WithAlpha(0.55f),
                },
            });

            root.AddChild(BuildInfoRow(
                Loc.GetString("ccm-round-end-info-mode"),
                gamemode,
                accent));

            if (!string.IsNullOrWhiteSpace(roundEnd))
            {
                var (summaryMarkup, detailsMarkup) = SplitRoundEndSummary(roundEnd);
                root.AddChild(BuildRoundSummaryBlock(summaryMarkup, winnerAccent));

                if (!string.IsNullOrWhiteSpace(detailsMarkup))
                {
                    root.AddChild(BuildSectionHeader("ccm-round-end-content-title"));
                    root.AddChild(BuildRoundDetailsBlock(detailsMarkup));
                }
            }

            var metaRow = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 8,
                HorizontalExpand = true,
            };

            var roundIdLabel = new RichTextLabel();
            roundIdLabel.SetMarkup(NormalizeEndRoundMarkup(Loc.GetString("round-end-summary-window-round-id-label", ("roundId", roundId))));
            metaRow.AddChild(BuildRoundMetaChip(roundIdLabel, accent));

            var roundTimeLabel = new RichTextLabel();
            roundTimeLabel.SetMarkup(NormalizeEndRoundMarkup(Loc.GetString(
                "round-end-summary-window-duration-label",
                ("hours", roundDuration.Hours),
                ("minutes", roundDuration.Minutes),
                ("seconds", roundDuration.Seconds))));
            metaRow.AddChild(BuildRoundMetaChip(roundTimeLabel, winnerAccent));

            root.AddChild(metaRow);

            panel.AddChild(root);
            return panel;
        }

        private Control BuildWinnerBadge(CCMStatsSide winningSide)
        {
            var accent = GetMvpAccentColor(winningSide);
            var key = winningSide == CCMStatsSide.Marines
                ? "ccm-round-end-round-winner-marines"
                : "ccm-round-end-round-winner-xenos";

            var panel = new PanelContainer
            {
                VerticalAlignment = VAlignment.Center,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = accent.WithAlpha(0.18f),
                    BorderColor = accent.WithAlpha(0.9f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 8,
                    ContentMarginTopOverride = 4,
                    ContentMarginRightOverride = 8,
                    ContentMarginBottomOverride = 4,
                },
            };

            panel.AddChild(new Label
            {
                Text = Loc.GetString(key),
                FontColorOverride = IsOldLobbyPalette() && winningSide == CCMStatsSide.Marines
                    ? GetPrimaryBodyTextColor()
                    : accent,
            });

            return panel;
        }

        private static Control BuildRoundMetaChip(Control child, Color accent)
        {
            var panel = new PanelContainer
            {
                HorizontalExpand = true,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = GetSummaryInsetBackground(),
                    BorderColor = accent.WithAlpha(0.65f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 8,
                    ContentMarginTopOverride = 6,
                    ContentMarginRightOverride = 8,
                    ContentMarginBottomOverride = 6,
                },
            };

            panel.AddChild(child);
            return panel;
        }

        private Control BuildRoundSummaryBlock(string summaryMarkup, Color accent)
        {
            var panel = new PanelContainer
            {
                HorizontalExpand = true,
                Margin = new Thickness(0, 2, 0, 0),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = GetSummaryInsetBackground(0.32f),
                    BorderColor = accent.WithAlpha(0.78f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 10,
                    ContentMarginTopOverride = 10,
                    ContentMarginRightOverride = 10,
                    ContentMarginBottomOverride = 10,
                },
            };

            var row = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 10,
                HorizontalExpand = true,
            };

            row.AddChild(new Label
            {
                Text = Loc.GetString("ccm-round-end-info-summary"),
                FontColorOverride = accent,
                MinSize = new Vector2(60, 0),
            });

            var summary = new RichTextLabel
            {
                HorizontalExpand = true,
            };
            summary.SetMarkup(NormalizeEndRoundMarkup(summaryMarkup));
            row.AddChild(summary);

            panel.AddChild(row);
            return panel;
        }

        private static Control BuildRoundDetailsBlock(string detailsMarkup)
        {
            var details = new RichTextLabel
            {
                HorizontalExpand = true,
            };
            details.SetMarkup(NormalizeEndRoundMarkup(detailsMarkup));
            return details;
        }

        private static (string SummaryMarkup, string DetailsMarkup) SplitRoundEndSummary(string roundEnd)
        {
            var normalized = roundEnd
                .Replace("\r\n", "\n")
                .Trim();

            if (string.IsNullOrWhiteSpace(normalized))
                return (string.Empty, string.Empty);

            var lines = normalized
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            if (lines.Count == 0)
                return (string.Empty, string.Empty);

            var summary = lines[0];
            var details = lines.Count > 1
                ? string.Join('\n', lines.Skip(1))
                : string.Empty;

            return (summary, details);
        }

        private static Control BuildInfoRow(string label, string value, Color accent)
        {
            var row = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 8,
                HorizontalExpand = true,
            };

            row.AddChild(new Label
            {
                Text = label,
                FontColorOverride = accent,
                MinSize = new Vector2(60, 0),
            });

            row.AddChild(new Label
            {
                Text = value,
                FontColorOverride = GetPrimaryBodyTextColor(),
                HorizontalExpand = true,
                ClipText = true,
            });

            return row;
        }

        private Control BuildSectionHeader(string locKey)
        {
            var panel = new PanelContainer
            {
                HorizontalExpand = true,
                Margin = new Thickness(0, 4, 0, 0),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = GetSectionHeaderBackground(),
                    BorderColor = GetPrimaryAccentColor().WithAlpha(IsOldLobbyPalette() ? 0.78f : 0.6f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 10,
                    ContentMarginTopOverride = 7,
                    ContentMarginRightOverride = 10,
                    ContentMarginBottomOverride = 7,
                },
            };

            panel.AddChild(new Label
            {
                Text = Loc.GetString(locKey),
                FontColorOverride = GetPrimaryAccentColor(),
                HorizontalAlignment = HAlignment.Center,
                HorizontalExpand = true,
            });

            return panel;
        }

        private Control BuildRoundScoreLabel(int score)
        {
            var panel = new PanelContainer
            {
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = GetInfoChipBackground(),
                    BorderColor = GetPrimaryAccentColor().WithAlpha(IsOldLobbyPalette() ? 0.82f : 0.7f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 10,
                    ContentMarginTopOverride = 8,
                    ContentMarginRightOverride = 10,
                    ContentMarginBottomOverride = 8,
                },
            };

            var label = new RichTextLabel
            {
                HorizontalExpand = true,
            };
            label.SetMarkup(NormalizeEndRoundMarkup(Loc.GetString("ccm-round-end-personal-score", ("score", score))));
            panel.AddChild(label);
            return panel;
        }

        private Control BuildMvpBlock(CCMRoundMvpData data)
        {
            var accent = GetMvpAccentColor(data.Side);
            var background = GetMvpBackgroundColor(data.Side);

            var panel = new PanelContainer
            {
                Margin = new Thickness(0, 10, 0, 0),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = background,
                    BorderColor = accent.WithAlpha(0.9f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 10,
                    ContentMarginTopOverride = 10,
                    ContentMarginRightOverride = 10,
                    ContentMarginBottomOverride = 10,
                },
            };

            var block = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 8,
            };

            var titleRow = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 8,
                HorizontalExpand = true,
            };

            var title = new Label
            {
                Text = Loc.GetString(
                    data.Side == CCMStatsSide.Marines
                        ? "ccm-round-end-mvp-marines"
                        : "ccm-round-end-mvp-xenos"),
                FontColorOverride = accent,
                FontOverride = _mvpTitleFont,
            };

            var subtitle = new Label
            {
                Text = Loc.GetString("ccm-round-end-mvp-subtitle"),
                FontColorOverride = GetMutedTextColor().WithAlpha(0.82f),
                HorizontalExpand = true,
                HorizontalAlignment = HAlignment.Right,
                FontOverride = _mvpSubtitleFont,
            };

            titleRow.AddChild(title);
            titleRow.AddChild(subtitle);
            block.AddChild(titleRow);
            block.AddChild(new PanelContainer
            {
                MinSize = new Vector2(0, 1),
                MaxSize = new Vector2(float.MaxValue, 1),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = accent.WithAlpha(0.55f),
                },
            });

            var row = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 12,
                HorizontalExpand = true,
            };

            var portraitHolder = new PanelContainer
            {
                MinSize = new Vector2(116, 116),
                MaxSize = new Vector2(116, 116),
                VerticalAlignment = VAlignment.Top,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = GetSummaryInsetBackground(0.35f),
                    BorderColor = accent.WithAlpha(0.55f),
                    BorderThickness = new Thickness(1),
                },
            };

            if (data.NetEntity != null)
            {
                portraitHolder.AddChild(new SpriteView(data.NetEntity.Value, _entityManager)
                {
                    OverrideDirection = Direction.South,
                    SetSize = new Vector2(108, 108),
                    VerticalAlignment = VAlignment.Center,
                    HorizontalAlignment = HAlignment.Center,
                });
            }

            row.AddChild(portraitHolder);

            var details = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 4,
                HorizontalExpand = true,
            };

            details.AddChild(new Label
            {
                Text = data.Name,
                FontColorOverride = GetPrimaryBodyTextColor(),
                ClipText = true,
            });
            details.AddChild(new Label
            {
                Text = data.Ckey,
                FontColorOverride = GetMutedTextColor().WithAlpha(0.8f),
                ClipText = true,
                Margin = new Thickness(0, 0, 0, 4),
            });

            details.AddChild(BuildMvpMetricRow("ccm-round-end-mvp-impact", data.ImpactPoints.ToString(), accent, accent));
            details.AddChild(BuildMvpMetricRow("ccm-round-end-mvp-damage", data.DamageDone.ToString(), accent, Color.White));
            details.AddChild(BuildMvpMetricRow("ccm-round-end-mvp-kills", data.Kills.ToString(), accent, Color.White));
            details.AddChild(BuildMvpMetricRow("ccm-round-end-mvp-healing", data.HealingDone.ToString(), accent, Color.White));

            details.AddChild(BuildMvpMetricRow("ccm-round-end-mvp-structures", data.StructuresBuilt.ToString(), accent, Color.White));

            row.AddChild(details);

            block.AddChild(row);
            panel.AddChild(block);
            return panel;
        }

        private Control BuildPersonalStatsBlock(CCMRoundPersonalStatsData data)
        {
            var accent = GetPrimaryAccentColor();
            var background = GetSummaryPanelBackground();

            var panel = new PanelContainer
            {
                Margin = new Thickness(0, 10, 0, 0),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = background,
                    BorderColor = accent.WithAlpha(0.85f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 10,
                    ContentMarginTopOverride = 10,
                    ContentMarginRightOverride = 10,
                    ContentMarginBottomOverride = 10,
                },
            };

            var root = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 8,
            };

            root.AddChild(new Label
            {
                Text = Loc.GetString("ccm-round-end-personal-title"),
                FontColorOverride = accent,
            });

            root.AddChild(new PanelContainer
            {
                MinSize = new Vector2(0, 1),
                MaxSize = new Vector2(float.MaxValue, 1),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = accent.WithAlpha(0.55f),
                },
            });

            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-victory-points", data.VictoryPoints.ToString(), accent, Color.White));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-impact-points", data.ImpactPoints.ToString(), accent, accent));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-damage", data.DamageDone.ToString(), accent, Color.White));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-kills", data.Kills.ToString(), accent, Color.White));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-healing", data.HealingDone.ToString(), accent, Color.White));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-structures", data.StructuresBuilt.ToString(), accent, Color.White));

            var showSideBreakdown = data.MarineParticipated && data.XenoParticipated;

            if (showSideBreakdown)
            {
                root.AddChild(BuildSideSummary(
                    Loc.GetString("ccm-round-end-personal-marines"),
                    accent,
                    data.MarineVictoryPoints,
                    data.MarineImpactPoints,
                    data.MarineDamageDone,
                    data.MarineKills,
                    data.MarineHealingDone,
                    data.MarineStructuresBuilt));
            }

            if (showSideBreakdown)
            {
                root.AddChild(BuildSideSummary(
                    Loc.GetString("ccm-round-end-personal-xenos"),
                    accent,
                    data.XenoVictoryPoints,
                    data.XenoImpactPoints,
                    data.XenoDamageDone,
                    data.XenoKills,
                    data.XenoHealingDone,
                    data.XenoStructuresBuilt));
            }

            panel.AddChild(root);
            return panel;
        }

        private Control BuildCampaignScoreBlock(int marineWins, int xenoWins, CCMStatsSide winningSide)
        {
            var marineAccent = GetPrimaryAccentColor();
            var xenoAccent = GetXenoAccentColor();
            var neutral = GetPrimaryBodyTextColor();

            var panel = new PanelContainer
            {
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalExpand = true,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = GetSummaryPanelBackground(),
                    BorderColor = marineAccent.WithAlpha(0.85f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 10,
                    ContentMarginTopOverride = 10,
                    ContentMarginRightOverride = 10,
                    ContentMarginBottomOverride = 10,
                },
            };

            var root = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 8,
                HorizontalExpand = true,
            };

            var titleRow = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 8,
                HorizontalExpand = true,
            };

            titleRow.AddChild(new Label
            {
                Text = Loc.GetString("ccm-round-wins-title"),
                HorizontalExpand = true,
                FontColorOverride = neutral,
                FontOverride = _mvpSubtitleFont,
            });

            root.AddChild(titleRow);

            root.AddChild(new PanelContainer
            {
                MinSize = new Vector2(0, 1),
                MaxSize = new Vector2(float.MaxValue, 1),
                HorizontalExpand = true,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = marineAccent.WithAlpha(0.55f),
                },
            });

            var row = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 12,
                HorizontalExpand = true,
            };

            row.AddChild(BuildCampaignScoreSide(
                Loc.GetString("ccm-round-wins-marines"),
                marineWins.ToString(),
                marineAccent,
                winningSide == CCMStatsSide.Marines));
            row.AddChild(BuildCampaignScoreSide(
                Loc.GetString("ccm-round-wins-xenos"),
                xenoWins.ToString(),
                xenoAccent,
                winningSide == CCMStatsSide.Xenos));

            root.AddChild(row);
            panel.AddChild(root);
            return panel;
        }

        private Control BuildSponsorCreditsBlock(List<SponsorCreditEntry> sponsors)
        {
            var panel = new PanelContainer
            {
                Margin = new Thickness(10, 0, 10, 10),
                HorizontalExpand = true,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = GetSponsorPanelBackground(),
                    BorderColor = GetPrimaryAccentColor().WithAlpha(0.75f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 10,
                    ContentMarginTopOverride = 10,
                    ContentMarginRightOverride = 10,
                    ContentMarginBottomOverride = 10,
                },
            };

            var root = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 8,
                HorizontalExpand = true,
            };

            var headerAccent = GetPrimaryAccentColor();
            root.AddChild(new Label
            {
                Text = Loc.GetString("ccm-sponsorship-endgame-header"),
                FontColorOverride = headerAccent,
                FontOverride = _mvpTitleFont,
                HorizontalExpand = true,
            });

            root.AddChild(new PanelContainer
            {
                MinSize = new Vector2(0, 1),
                MaxSize = new Vector2(float.MaxValue, 1),
                HorizontalExpand = true,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = headerAccent.WithAlpha(0.45f),
                },
            });

            AddSponsorTierSection(root, sponsors, CCMSponsorshipTier.SponsorIII);
            AddSponsorTierSection(root, sponsors, CCMSponsorshipTier.SponsorII);
            AddSponsorTierSection(root, sponsors, CCMSponsorshipTier.SponsorI);

            panel.AddChild(root);
            return panel;
        }

        private void AddSponsorTierSection(BoxContainer root, List<SponsorCreditEntry> sponsors, CCMSponsorshipTier tier)
        {
            var tierEntries = sponsors.Where(entry => entry.Tier == tier).Select(entry => entry.Ckey).ToList();
            if (tierEntries.Count == 0)
                return;

            var accent = GetSponsorTierColor(tier);
            var tierTitle = new Label
            {
                Text = Loc.GetString(GetSponsorTierLocKey(tier)),
                FontColorOverride = accent,
                HorizontalAlignment = HAlignment.Center,
                HorizontalExpand = true,
                FontOverride = GetSponsorTierFont(tier),
                Margin = new Thickness(0, tier == CCMSponsorshipTier.SponsorIII ? 6 : 4, 0, 0),
            };

            root.AddChild(tierTitle);

            foreach (var ckey in tierEntries)
            {
                root.AddChild(new Label
                {
                    Text = ckey,
                    FontColorOverride = accent.WithAlpha(0.96f),
                    HorizontalAlignment = HAlignment.Center,
                    HorizontalExpand = true,
                    FontOverride = GetSponsorTierNameFont(tier),
                });
            }
        }

        private List<SponsorCreditEntry> ExtractSponsorCredits(string roundEnd, out string cleanedRoundEnd)
        {
            var result = new List<SponsorCreditEntry>();
            cleanedRoundEnd = roundEnd;

            if (string.IsNullOrWhiteSpace(roundEnd))
                return result;

            var sponsorHeader = Loc.GetString("ccm-sponsorship-endgame-header");
            var tierOneTitle = Loc.GetString("ccm-sponsorship-tier-1-title");
            var tierTwoTitle = Loc.GetString("ccm-sponsorship-tier-2-title");
            var tierThreeTitle = Loc.GetString("ccm-sponsorship-tier-3-title");
            var sponsorLineRegex = new Regex(@"^(?<ckey>.+?)\s+\((?<tier>[^)]+)\)$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);

            var lines = roundEnd.Split('\n').ToList();
            var cleanedLines = new List<string>();
            var inSponsorBlock = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd();
                var plainLine = StripMarkup(line).Trim();

                if (plainLine.Equals(sponsorHeader, StringComparison.CurrentCultureIgnoreCase))
                {
                    inSponsorBlock = true;
                    continue;
                }

                if (inSponsorBlock)
                {
                    var match = sponsorLineRegex.Match(plainLine);
                    if (match.Success)
                    {
                        var ckey = match.Groups["ckey"].Value.Trim();
                        var tierText = match.Groups["tier"].Value.Trim();
                        var tier = tierText switch
                        {
                            var t when t == tierThreeTitle => CCMSponsorshipTier.SponsorIII,
                            var t when t == tierTwoTitle => CCMSponsorshipTier.SponsorII,
                            _ => CCMSponsorshipTier.SponsorI,
                        };

                        result.Add(new SponsorCreditEntry(ckey, tier));
                        continue;
                    }

                    inSponsorBlock = false;
                }

                cleanedLines.Add(line);
            }

            cleanedRoundEnd = string.Join('\n', cleanedLines.Where(line => !string.IsNullOrWhiteSpace(line)));
            return result;
        }

        private static string StripMarkup(string markup)
        {
            return Regex.Replace(markup, @"\[[^\]]+\]", string.Empty);
        }

        private Font GetSponsorTierFont(CCMSponsorshipTier tier)
        {
            return tier switch
            {
                CCMSponsorshipTier.SponsorIII => _sponsorTierThreeFont,
                CCMSponsorshipTier.SponsorII => _sponsorTierTwoFont,
                _ => _sponsorTierOneFont,
            };
        }

        private Font GetSponsorTierNameFont(CCMSponsorshipTier tier)
        {
            return tier switch
            {
                CCMSponsorshipTier.SponsorIII => _sponsorTierThreeNameFont,
                CCMSponsorshipTier.SponsorII => _sponsorTierTwoNameFont,
                _ => _sponsorTierOneNameFont,
            };
        }

        private static string GetSponsorTierLocKey(CCMSponsorshipTier tier)
        {
            return tier switch
            {
                CCMSponsorshipTier.SponsorIII => "ccm-sponsorship-tier-3-title",
                CCMSponsorshipTier.SponsorII => "ccm-sponsorship-tier-2-title",
                _ => "ccm-sponsorship-tier-1-title",
            };
        }

        private static Color GetSponsorTierColor(CCMSponsorshipTier tier)
        {
            return tier switch
            {
                CCMSponsorshipTier.SponsorIII => Color.FromHex("#F6C453"),
                CCMSponsorshipTier.SponsorII => Color.FromHex("#D96CFF"),
                _ => Color.FromHex("#61C9FF"),
            };
        }

        private sealed record SponsorCreditEntry(string Ckey, CCMSponsorshipTier Tier);

        private Control BuildCampaignScoreSide(string title, string score, Color accent, bool highlighted)
        {
            var panel = new PanelContainer
            {
                HorizontalExpand = true,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = highlighted
                        ? accent.WithAlpha(0.16f)
                        : GetSummaryInsetBackground(),
                    BorderColor = accent.WithAlpha(highlighted ? 0.95f : 0.55f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 10,
                    ContentMarginTopOverride = 10,
                    ContentMarginRightOverride = 10,
                    ContentMarginBottomOverride = 10,
                },
            };

            var column = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                HorizontalExpand = true,
                SeparationOverride = 4,
            };

            column.AddChild(new Label
            {
                Text = title,
                HorizontalAlignment = HAlignment.Center,
                HorizontalExpand = true,
                FontColorOverride = accent,
                FontOverride = _mvpSubtitleFont,
            });
            column.AddChild(new Label
            {
                Text = score,
                HorizontalAlignment = HAlignment.Center,
                HorizontalExpand = true,
                FontColorOverride = GetPrimaryBodyTextColor(),
                FontOverride = _mvpTitleFont,
            });

            panel.AddChild(column);
            return panel;
        }

        private static BoxContainer BuildMvpMetricRow(string locKey, string value, Color accent, Color valueColor)
        {
            var row = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 8,
                HorizontalExpand = true,
            };

            row.AddChild(new Label
            {
                Text = Loc.GetString(locKey),
                HorizontalExpand = true,
                FontColorOverride = GetMutedTextColor().WithAlpha(0.88f),
            });

            row.AddChild(new Label
            {
                Text = value,
                FontColorOverride = valueColor,
                HorizontalAlignment = HAlignment.Right,
            });

            return row;
        }

        private static Control BuildSideSummary(
            string title,
            Color accent,
            int victoryPoints,
            int impactPoints,
            int damage,
            int kills,
            int healing,
            int structures)
        {
            var container = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 4,
                Margin = new Thickness(0, 6, 0, 0),
            };

            container.AddChild(new Label
            {
                Text = title,
                FontColorOverride = accent.WithAlpha(IsOldLobbyPalette() ? 0.98f : 0.9f),
            });

            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-victory-points", victoryPoints.ToString(), accent, Color.White));
            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-impact-points", impactPoints.ToString(), accent, accent));
            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-damage", damage.ToString(), accent, Color.White));
            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-kills", kills.ToString(), accent, Color.White));
            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-healing", healing.ToString(), accent, Color.White));
            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-structures", structures.ToString(), accent, Color.White));
            return container;
        }

        private static Color GetMvpAccentColor(CCMStatsSide side)
        {
            return side == CCMStatsSide.Marines
                ? GetPrimaryAccentColor()
                : GetXenoAccentColor();
        }

        private static Color GetXenoAccentColor()
        {
            return IsOldLobbyPalette()
                ? Color.FromHex("#C994E8")
                : Color.FromHex("#D96CFF");
        }

        private static Color GetMvpBackgroundColor(CCMStatsSide side)
        {
            return side == CCMStatsSide.Marines
                ? GetMarineMvpBackground()
                : Color.FromHex("#13081A").WithAlpha(0.92f);
        }

        private static Color GetSummaryPanelBackground()
        {
            if (IsOldLobbyPalette())
                return StyleNano.OldLobbyPanel.WithAlpha(0.98f);

            return StyleNano.CurrentTheme switch
            {
                StyleNano.UiColorTheme.Gray => Color.FromHex("#1A2028").WithAlpha(0.97f),
                _ => Color.FromHex("#08150D").WithAlpha(0.97f),
            };
        }

        private static Color GetSponsorPanelBackground()
        {
            if (IsOldLobbyPalette())
                return StyleNano.OldLobbyPanelSoft.WithAlpha(0.98f);

            return StyleNano.CurrentTheme switch
            {
                StyleNano.UiColorTheme.Gray => Color.FromHex("#202730").WithAlpha(0.96f),
                _ => Color.FromHex("#091A11").WithAlpha(0.96f),
            };
        }

        private static Color GetSummaryInsetBackground(float alpha = 0.18f)
        {
            if (IsOldLobbyPalette())
                return StyleNano.OldLobbyButton.WithAlpha(MathF.Min(0.98f, alpha + 0.52f));

            return StyleNano.CurrentTheme switch
            {
                StyleNano.UiColorTheme.Gray => Color.FromHex("#0F1318").WithAlpha(MathF.Min(0.98f, alpha + 0.16f)),
                _ => Color.Black.WithAlpha(alpha),
            };
        }

        private static Color GetMarineMvpBackground()
        {
            if (IsOldLobbyPalette())
                return StyleNano.OldLobbyPanelSoft.WithAlpha(0.95f);

            return StyleNano.CurrentTheme switch
            {
                StyleNano.UiColorTheme.Gray => Color.FromHex("#171D24").WithAlpha(0.92f),
                _ => Color.FromHex("#07150A").WithAlpha(0.92f),
            };
        }

        private static bool IsOldLobbyPalette()
        {
            return StyleNano.LobbyMenuButtonBase == StyleNano.OldLobbyButton;
        }

        private static Color GetPrimaryAccentColor()
        {
            return IsOldLobbyPalette()
                ? StyleNano.OldLobbyGold
                : StyleNano.LobbyMenuButtonBase;
        }

        private static Color GetPrimaryBodyTextColor()
        {
            return IsOldLobbyPalette()
                ? StyleNano.OldLobbyText
                : Color.White;
        }

        private static Color GetMutedTextColor()
        {
            return IsOldLobbyPalette()
                ? StyleNano.OldLobbyMuted
                : Color.White.WithAlpha(0.78f);
        }

        private static Color GetAntagTextColor()
        {
            return IsOldLobbyPalette()
                ? Color.FromHex("#D88A8A")
                : Color.Red;
        }

        private static Color GetSectionHeaderBackground()
        {
            return IsOldLobbyPalette()
                ? StyleNano.OldLobbyButtonHover.WithAlpha(0.82f)
                : StyleNano.ButtonColorContext.WithAlpha(0.12f);
        }

        private static Color GetInfoChipBackground()
        {
            return IsOldLobbyPalette()
                ? StyleNano.OldLobbyButton.WithAlpha(0.88f)
                : StyleNano.ButtonColorContext.WithAlpha(0.15f);
        }

        private static string NormalizeEndRoundMarkup(string markup)
        {
            if (!IsOldLobbyPalette() || string.IsNullOrWhiteSpace(markup))
                return markup;

            return markup
                .Replace("[color=white]", $"[color={GetMarkupColorHex(GetPrimaryBodyTextColor())}]")
                .Replace("[color=gray]", $"[color={GetMarkupColorHex(GetMutedTextColor())}]")
                .Replace("[color=yellow]", $"[color={GetMarkupColorHex(Color.FromHex("#E7D178"))}]")
                .Replace("[color=orange]", $"[color={GetMarkupColorHex(Color.FromHex("#D9B172"))}]")
                .Replace("[color=lightblue]", $"[color={GetMarkupColorHex(Color.FromHex("#AFC8E4"))}]")
                .Replace("[color=red]", $"[color={GetMarkupColorHex(GetAntagTextColor())}]");
        }

        private static string GetMarkupColorHex(Color color)
        {
            var r = (int) Math.Clamp(color.RByte, (byte) 0, (byte) 255);
            var g = (int) Math.Clamp(color.GByte, (byte) 0, (byte) 255);
            var b = (int) Math.Clamp(color.BByte, (byte) 0, (byte) 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }

}

