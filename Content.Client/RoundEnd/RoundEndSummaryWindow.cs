using System.Linq;
using System.Numerics;
using Content.Client.Administration.UI.CustomControls;
using Content.Client._CCM.Stats;
using Content.Client.Message;
using Content.Client.Resources;
using Robust.Client.ResourceManagement;
using Content.Client.Stylesheets;
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
        private readonly Font _mvpTitleFont;
        private readonly Font _mvpSubtitleFont;
        public int RoundId;

        public RoundEndSummaryWindow(string gm, string roundEnd, TimeSpan roundTimeSpan, int roundId,
            RoundEndMessageEvent.RoundEndPlayerInfo[] info, IEntityManager entityManager)
        {
            _entityManager = entityManager;
            _ccmStatsSystem = _entityManager.System<CCMStatsSystem>();
            var resourceCache = IoCManager.Resolve<IResourceCache>();
            _mvpTitleFont = resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 14);
            _mvpSubtitleFont = resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 12);

            MinSize = new Vector2(400, 580);
            SetSize = new Vector2(400, 580);

            Title = Loc.GetString("round-end-summary-window-title");

            // The round end window is split into two tabs, one about the round stats
            // and the other is a list of RoundEndPlayerInfo for each player.
            // This tab would be a good place for things like: "x many people died.",
            // "clown slipped the crew x times.", "x shots were fired this round.", etc.
            // Also good for serious info.

            RoundId = roundId;
            var roundEndTabs = new TabContainer();
            roundEndTabs.AddChild(MakeRoundEndSummaryTab(gm, roundEnd, roundTimeSpan, roundId));
            roundEndTabs.AddChild(MakePlayerManifestTab(info));

            Contents.AddChild(roundEndTabs);

            OpenCenteredRight();
            MoveToFront();
        }

        private BoxContainer MakeRoundEndSummaryTab(string gamemode, string roundEnd, TimeSpan roundDuration, int roundId)
        {
            var roundEndSummaryTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-round-end-summary-tab-title")
            };

            var roundEndSummaryContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10),
                HScrollEnabled = false,
            };
            var roundEndSummaryContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 6,
            };

            var roundIdLabel = new RichTextLabel();
            roundIdLabel.SetMarkup(Loc.GetString("round-end-summary-window-round-id-label", ("roundId", roundId)));
            roundEndSummaryContainer.AddChild(roundIdLabel);

            //Duration
            var roundTimeLabel = new RichTextLabel();
            roundTimeLabel.SetMarkup(Loc.GetString("round-end-summary-window-duration-label",
                                                   ("hours", roundDuration.Hours),
                                                   ("minutes", roundDuration.Minutes),
                                                   ("seconds", roundDuration.Seconds)));
            roundEndSummaryContainer.AddChild(roundTimeLabel);

            //Round end text
            if (!string.IsNullOrEmpty(roundEnd))
            {
                var roundEndLabel = new RichTextLabel
                {
                    HorizontalExpand = true,
                };
                roundEndLabel.SetMarkup(roundEnd);
                roundEndSummaryContainer.AddChild(roundEndLabel);
            }

            var roundStats = _ccmStatsSystem.LatestRoundEndStats;
            if (roundStats != null)
            {
                roundEndSummaryContainer.AddChild(BuildCampaignScoreBlock(
                    roundStats.MarineCampaignWins,
                    roundStats.XenoCampaignWins));
                roundEndSummaryContainer.AddChild(BuildRoundScoreLabel(roundStats.PersonalScore));

                if (roundStats.MarineMvp != null)
                    roundEndSummaryContainer.AddChild(BuildMvpBlock(roundStats.MarineMvp));

                if (roundStats.XenoMvp != null)
                    roundEndSummaryContainer.AddChild(BuildMvpBlock(roundStats.XenoMvp));

                if (roundStats.PersonalStats != null)
                    roundEndSummaryContainer.AddChild(BuildPersonalStatsBlock(roundStats.PersonalStats));
            }

            roundEndSummaryContainerScrollbox.AddChild(roundEndSummaryContainer);
            roundEndSummaryTab.AddChild(roundEndSummaryContainerScrollbox);

            return roundEndSummaryTab;
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
                };

                var playerInfoText = new RichTextLabel
                {
                    VerticalAlignment = VAlignment.Center,
                    VerticalExpand = true,
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
                            Loc.GetString("round-end-summary-window-player-info-if-observer-text",
                                          ("playerOOCName", playerInfo.PlayerOOCName),
                                          ("playerICName", playerInfo.PlayerICName)));
                    }
                    else
                    {
                        //TODO: On Hover display a popup detailing more play info.
                        //For example: their antag goals and if they completed them sucessfully.
                        var icNameColor = playerInfo.Antag ? "red" : "white";
                        playerInfoText.SetMarkup(
                            Loc.GetString("round-end-summary-window-player-info-if-not-observer-text",
                                ("playerOOCName", playerInfo.PlayerOOCName),
                                ("icNameColor", icNameColor),
                                ("playerICName", playerInfo.PlayerICName),
                                ("playerRole", Loc.GetString(playerInfo.Role))));
                    }
                }
                hBox.AddChild(playerInfoText);
                playerInfoContainer.AddChild(hBox);
            }

            playerInfoContainerScrollbox.AddChild(playerInfoContainer);
            playerManifestTab.AddChild(playerInfoContainerScrollbox);

            return playerManifestTab;
        }

        private RichTextLabel BuildRoundScoreLabel(int score)
        {
            var label = new RichTextLabel();
            label.SetMarkup(Loc.GetString("ccm-round-end-personal-score", ("score", score)));
            return label;
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
                FontColorOverride = Color.White.WithAlpha(0.75f),
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
                    BackgroundColor = Color.Black.WithAlpha(0.35f),
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
                FontColorOverride = Color.White,
                ClipText = true,
            });
            details.AddChild(new Label
            {
                Text = data.Ckey,
                FontColorOverride = Color.White.WithAlpha(0.7f),
                ClipText = true,
                Margin = new Thickness(0, 0, 0, 4),
            });

            details.AddChild(BuildMvpMetricRow("ccm-round-end-mvp-impact", data.ImpactPoints.ToString(), accent, accent));
            details.AddChild(BuildMvpMetricRow("ccm-round-end-mvp-damage", data.DamageDone.ToString(), accent, Color.White));
            details.AddChild(BuildMvpMetricRow("ccm-round-end-mvp-kills", data.Kills.ToString(), accent, Color.White));
            details.AddChild(BuildMvpMetricRow("ccm-round-end-mvp-healing", data.HealingDone.ToString(), accent, Color.White));

            if (data.Side == CCMStatsSide.Marines)
                details.AddChild(BuildMvpMetricRow("ccm-round-end-mvp-revives", data.Revives.ToString(), accent, Color.White));

            details.AddChild(BuildMvpMetricRow("ccm-round-end-mvp-structures", data.StructuresBuilt.ToString(), accent, Color.White));

            row.AddChild(details);

            block.AddChild(row);
            panel.AddChild(block);
            return panel;
        }

        private Control BuildPersonalStatsBlock(CCMRoundPersonalStatsData data)
        {
            var accent = StyleNano.LobbyMenuButtonBase;
            var background = Color.FromHex("#08150D");

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

            var time = TimeSpan.FromSeconds(data.RoundSecondsPlayed);

            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-victory-points", data.VictoryPoints.ToString(), accent, Color.White));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-impact-points", data.ImpactPoints.ToString(), accent, accent));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-damage", data.DamageDone.ToString(), accent, Color.White));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-kills", data.Kills.ToString(), accent, Color.White));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-healing", data.HealingDone.ToString(), accent, Color.White));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-revives", data.Revives.ToString(), accent, Color.White));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-structures", data.StructuresBuilt.ToString(), accent, Color.White));
            root.AddChild(BuildMvpMetricRow("ccm-round-end-personal-time", Loc.GetString("round-end-summary-window-duration-label",
                ("hours", time.Hours),
                ("minutes", time.Minutes),
                ("seconds", time.Seconds)), accent, Color.White));

            if (data.MarineVictoryPoints > 0 ||
                data.MarineImpactPoints > 0 ||
                data.MarineDamageDone > 0 ||
                data.MarineKills > 0 ||
                data.MarineHealingDone > 0 ||
                data.MarineRevives > 0 ||
                data.MarineStructuresBuilt > 0)
            {
                root.AddChild(BuildSideSummary(
                    Loc.GetString("ccm-round-end-personal-marines"),
                    accent,
                    data.MarineVictoryPoints,
                    data.MarineImpactPoints,
                    data.MarineDamageDone,
                    data.MarineKills,
                    data.MarineHealingDone,
                    data.MarineRevives,
                    data.MarineStructuresBuilt));
            }

            if (data.XenoVictoryPoints > 0 ||
                data.XenoImpactPoints > 0 ||
                data.XenoDamageDone > 0 ||
                data.XenoKills > 0 ||
                data.XenoHealingDone > 0 ||
                data.XenoStructuresBuilt > 0)
            {
                root.AddChild(BuildSideSummary(
                    Loc.GetString("ccm-round-end-personal-xenos"),
                    accent,
                    data.XenoVictoryPoints,
                    data.XenoImpactPoints,
                    data.XenoDamageDone,
                    data.XenoKills,
                    data.XenoHealingDone,
                    0,
                    data.XenoStructuresBuilt));
            }

            panel.AddChild(root);
            return panel;
        }

        private Control BuildCampaignScoreBlock(int marineWins, int xenoWins)
        {
            var marineAccent = StyleNano.LobbyMenuButtonBase;
            var xenoAccent = new Color(0.79f, 0.56f, 0.97f);
            var neutral = Color.FromHex("#D9DDE3");

            var panel = new PanelContainer
            {
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalExpand = true,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = Color.FromHex("#08150D"),
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

            root.AddChild(new Label
            {
                Text = Loc.GetString("ccm-round-wins-title"),
                HorizontalAlignment = HAlignment.Center,
                HorizontalExpand = true,
                FontColorOverride = neutral,
            });

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
                marineAccent));
            row.AddChild(new Label
            {
                Text = "-",
                FontColorOverride = neutral.WithAlpha(0.7f),
                VerticalAlignment = VAlignment.Center,
            });
            row.AddChild(BuildCampaignScoreSide(
                Loc.GetString("ccm-round-wins-xenos"),
                xenoWins.ToString(),
                xenoAccent));

            root.AddChild(row);
            panel.AddChild(root);
            return panel;
        }

        private static Control BuildCampaignScoreSide(string title, string score, Color accent)
        {
            var column = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                HorizontalExpand = true,
                SeparationOverride = 2,
            };

            column.AddChild(new Label
            {
                Text = title,
                HorizontalAlignment = HAlignment.Center,
                HorizontalExpand = true,
                FontColorOverride = accent,
            });
            column.AddChild(new Label
            {
                Text = score,
                HorizontalAlignment = HAlignment.Center,
                HorizontalExpand = true,
                FontColorOverride = Color.White,
            });

            return column;
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
                FontColorOverride = Color.White.WithAlpha(0.78f),
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
            int revives,
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
                FontColorOverride = accent.WithAlpha(0.9f),
            });

            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-victory-points", victoryPoints.ToString(), accent, Color.White));
            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-impact-points", impactPoints.ToString(), accent, accent));
            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-damage", damage.ToString(), accent, Color.White));
            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-kills", kills.ToString(), accent, Color.White));
            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-healing", healing.ToString(), accent, Color.White));

            if (revives > 0)
                container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-revives", revives.ToString(), accent, Color.White));

            container.AddChild(BuildMvpMetricRow("ccm-round-end-personal-structures", structures.ToString(), accent, Color.White));
            return container;
        }

        private static Color GetMvpAccentColor(CCMStatsSide side)
        {
            return side == CCMStatsSide.Marines
                ? StyleNano.LobbyMenuButtonBase
                : new Color(0.79f, 0.56f, 0.97f);
        }

        private static Color GetMvpBackgroundColor(CCMStatsSide side)
        {
            return side == CCMStatsSide.Marines
                ? Color.FromHex("#07150A").WithAlpha(0.92f)
                : Color.FromHex("#13081A").WithAlpha(0.92f);
        }
    }

}

