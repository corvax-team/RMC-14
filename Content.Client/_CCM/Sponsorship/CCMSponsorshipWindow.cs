using System;
using System.Collections.Generic;
using System.Globalization;
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
using Robust.Shared.Utility;

namespace Content.Client._CCM.Sponsorship;

public sealed class CCMSponsorshipWindow : DefaultCMWindow
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private readonly Label _statusLabel;
    private readonly Label _expirationLabel;
    private readonly Button _websiteButton;
    private readonly BoxContainer _tiersContainer;
    private string _donateUrl = string.Empty;
    private CCMSponsorshipTier _currentTier = CCMSponsorshipTier.None;

    public event Action<string>? OpenDonateRequested;

    public CCMSponsorshipWindow()
    {
        IoCManager.InjectDependencies(this);

        Title = string.Empty;
        MinSize = new Vector2(1140, 730);
        WindowTitleLabel.Visible = false;
        HeaderPanel.MinSize = new Vector2(0, 26);
        HeaderPanel.Margin = new Thickness(10, 6, 10, 0);
        BodyPanel.Margin = new Thickness(10, -1, 10, 10);

        ApplyWindowTheme();

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 10,
            Margin = new Thickness(14, 2, 14, 12),
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        root.AddChild(new Label
        {
            Text = Loc.GetString("ccm-sponsorship-header"),
            HorizontalAlignment = HAlignment.Center,
            FontColorOverride = StyleNano.LobbyMenuButtonBase,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 24),
        });

        var topSection = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 4,
            HorizontalExpand = true,
        };

        var topRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 10,
            HorizontalExpand = true,
        };

        _statusLabel = new Label
        {
            HorizontalExpand = true,
            FontColorOverride = Color.FromHex("#E6EDF5"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 16),
            VerticalAlignment = VAlignment.Center,
        };

        _expirationLabel = new Label
        {
            HorizontalExpand = true,
            FontColorOverride = Color.FromHex("#AFC1D4"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 12),
        };

        _websiteButton = new Button
        {
            Text = Loc.GetString("ccm-sponsorship-open-site"),
            MinSize = new Vector2(242, 40),
        };
        _websiteButton.OnPressed += _ => OpenDonateRequested?.Invoke(_donateUrl);
        _websiteButton.OnMouseEntered += _ =>
        {
            if (!_websiteButton.Disabled)
                ApplyWebsiteButtonState(pressed: false);
        };
        _websiteButton.OnMouseExited += _ => StyleWebsiteButton();
        _websiteButton.OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick || _websiteButton.Disabled)
                return;

            ApplyWebsiteButtonState(pressed: true);
        };
        _websiteButton.OnKeyBindUp += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            StyleWebsiteButton();
        };

        topRow.AddChild(_statusLabel);
        topRow.AddChild(_websiteButton);
        topSection.AddChild(topRow);
        topSection.AddChild(_expirationLabel);
        root.AddChild(topSection);

        root.AddChild(new Label
        {
            Text = Loc.GetString("ccm-sponsorship-intro"),
            FontColorOverride = Color.FromHex("#BAC7D4"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 13),
        });

        var scroll = new ScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            HScrollEnabled = false,
        };

        _tiersContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 16,
            HorizontalAlignment = HAlignment.Center,
            HorizontalExpand = true,
            VerticalExpand = false,
        };

        scroll.AddChild(_tiersContainer);
        root.AddChild(scroll);
        root.AddChild(BuildSponsorInfoBlock());
        Contents.AddChild(root);

        BuildTierCards(CCMSponsorshipTier.None);
        StyleWebsiteButton();
    }

    public void SetStatus(CCMSponsorshipStatusSnapshot snapshot)
    {
        _donateUrl = snapshot.DonateUrl;
        _currentTier = snapshot.Tier;
        _websiteButton.Disabled = string.IsNullOrWhiteSpace(snapshot.DonateUrl);
        _statusLabel.Text = Loc.GetString("ccm-sponsorship-current-tier",
            ("tier", Loc.GetString(GetTierTitleKey(snapshot.Tier))));
        _expirationLabel.Text = snapshot.ExpirationUnixSeconds > 0
            ? Loc.GetString("ccm-sponsorship-expires",
                ("date", DateTimeOffset.FromUnixTimeSeconds(snapshot.ExpirationUnixSeconds)
                    .ToLocalTime()
                    .ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)))
            : Loc.GetString("ccm-sponsorship-expires-none");
        BuildTierCards(snapshot.Tier);
        StyleWebsiteButton();
    }

    private void BuildTierCards(CCMSponsorshipTier currentTier)
    {
        _tiersContainer.DisposeAllChildren();

        foreach (var tier in new[] { CCMSponsorshipTier.SponsorI, CCMSponsorshipTier.SponsorIII, CCMSponsorshipTier.SponsorII })
        {
            _tiersContainer.AddChild(BuildTierCard(tier, currentTier == tier, tier == CCMSponsorshipTier.SponsorIII));
        }
    }

    private Control BuildTierCard(CCMSponsorshipTier tier, bool current, bool featured)
    {
        var accent = GetTierAccent(tier);
        var baseBackground = GetTierCardBackground(tier);
        var imageBackground = GetTierImageBackground(tier);
        var cardWidth = featured ? 376 : 316;
        var cardHeight = featured ? 496 : 454;
        var imageHeight = featured ? 184 : 162;
        var titleSize = featured ? 28 : 24;

        var panel = new PanelContainer
        {
            MinSize = new Vector2(cardWidth, cardHeight),
            MaxSize = new Vector2(cardWidth, cardHeight),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = current
                    ? baseBackground.WithAlpha(0.96f)
                    : baseBackground.WithAlpha(0.86f),
                BorderColor = current
                    ? accent.WithAlpha(0.95f)
                    : StyleNano.LobbyMenuButtonBase.WithAlpha(0.32f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 12,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 12,
            },
        };

        var content = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = featured ? 11 : 9,
        };

        content.AddChild(new PanelContainer
        {
            MinSize = new Vector2(0, 4),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = accent.WithAlpha(current ? 0.95f : 0.78f),
            },
        });

        content.AddChild(new Label
        {
            Text = Loc.GetString(GetTierTitleKey(tier)),
            HorizontalAlignment = HAlignment.Center,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", titleSize),
            FontColorOverride = accent,
        });

        if (current)
        {
            var badge = new PanelContainer
            {
                HorizontalAlignment = HAlignment.Center,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = accent.WithAlpha(0.16f),
                    BorderColor = accent.WithAlpha(0.45f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 7,
                    ContentMarginTopOverride = 2,
                    ContentMarginRightOverride = 7,
                    ContentMarginBottomOverride = 2,
                },
            };

            badge.AddChild(new Label
            {
                Text = Loc.GetString("ccm-sponsorship-current-tier-badge"),
                FontColorOverride = accent,
                FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 10),
            });
            content.AddChild(badge);
        }

        var imagePanel = new PanelContainer
        {
            MinSize = new Vector2(0, imageHeight),
            MaxSize = new Vector2(float.MaxValue, imageHeight),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = imageBackground,
                BorderColor = accent.WithAlpha(0.42f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 4,
                ContentMarginTopOverride = 4,
                ContentMarginRightOverride = 4,
                ContentMarginBottomOverride = 4,
            },
        };
        imagePanel.AddChild(new TextureRect
        {
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
            Texture = _resourceCache.GetTexture("/Textures/Logo/logo.png"),
            HorizontalExpand = true,
            VerticalExpand = true,
        });
        content.AddChild(imagePanel);

        var perks = new RichTextLabel
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            MaxWidth = cardWidth - 34,
        };
        perks.SetMessage(BuildPerksMessage(tier));

        content.AddChild(perks);
        panel.AddChild(content);
        return panel;
    }

    private static Color GetTierAccent(CCMSponsorshipTier tier)
    {
        return tier switch
        {
            CCMSponsorshipTier.SponsorIII => Color.FromHex("#F6C453"),
            CCMSponsorshipTier.SponsorII => Color.FromHex("#D96CFF"),
            CCMSponsorshipTier.SponsorI => Color.FromHex("#61C9FF"),
            _ => Color.FromHex("#8FA0AE"),
        };
    }

    private static Color GetTierCardBackground(CCMSponsorshipTier tier)
    {
        return tier switch
        {
            CCMSponsorshipTier.SponsorIII => Color.FromHex("#241E0E"),
            CCMSponsorshipTier.SponsorII => Color.FromHex("#241129"),
            _ => Color.FromHex("#0C1D27"),
        };
    }

    private static Color GetTierImageBackground(CCMSponsorshipTier tier)
    {
        return tier switch
        {
            CCMSponsorshipTier.SponsorIII => Color.FromHex("#8B6F24").WithAlpha(0.88f),
            CCMSponsorshipTier.SponsorII => Color.FromHex("#6C3B7D").WithAlpha(0.88f),
            _ => Color.FromHex("#356F8C").WithAlpha(0.88f),
        };
    }

    private FormattedMessage BuildPerksMessage(CCMSponsorshipTier tier)
    {
        var fontSize = tier == CCMSponsorshipTier.SponsorIII ? 12 : 11;
        var message = new FormattedMessage();

        foreach (var perkKey in GetTierPerkKeys(tier))
        {
            message.AddMarkupOrThrow($"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size={fontSize}][color=#DCE5EE]- {Loc.GetString(perkKey)}[/color][/font]\n");
        }

        return message;
    }

    private static string GetTierTitleKey(CCMSponsorshipTier tier)
    {
        return tier switch
        {
            CCMSponsorshipTier.SponsorIII => "ccm-sponsorship-tier-3-title",
            CCMSponsorshipTier.SponsorII => "ccm-sponsorship-tier-2-title",
            CCMSponsorshipTier.SponsorI => "ccm-sponsorship-tier-1-title",
            _ => "ccm-sponsorship-tier-none-title",
        };
    }

    private static IReadOnlyList<string> GetTierPerkKeys(CCMSponsorshipTier tier)
    {
        return tier switch
        {
            CCMSponsorshipTier.SponsorIII =>
            [
                "ccm-sponsorship-perk-chat-color",
                "ccm-sponsorship-perk-role-weight-3",
                "ccm-sponsorship-perk-endgame-credits",
                "ccm-sponsorship-perk-customization",
                "ccm-sponsorship-perk-queue"
            ],
            CCMSponsorshipTier.SponsorII =>
            [
                "ccm-sponsorship-perk-chat-color",
                "ccm-sponsorship-perk-role-weight-2",
                "ccm-sponsorship-perk-endgame-credits",
                "ccm-sponsorship-perk-customization",
                "ccm-sponsorship-perk-queue"
            ],
            _ =>
            [
                "ccm-sponsorship-perk-chat-color",
                "ccm-sponsorship-perk-endgame-credits",
                "ccm-sponsorship-perk-customization",
                "ccm-sponsorship-perk-queue"
            ],
        };
    }

    private Control BuildSponsorInfoBlock()
    {
        var panel = new PanelContainer
        {
            MinSize = new Vector2(0, 86),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.20f),
                BorderColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.34f),
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

        stack.AddChild(new Label
        {
            Text = Loc.GetString("ccm-sponsorship-info-title"),
            FontColorOverride = StyleNano.LobbyMenuButtonBase,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13),
        });

        var notes = new RichTextLabel
        {
            HorizontalExpand = true,
        };
        notes.SetMessage(FormattedMessage.FromMarkupOrThrow(
            $"[color=#D7E1EB]- {FormattedMessage.EscapeText(Loc.GetString("ccm-sponsorship-info-line-1"))}[/color]\n" +
            $"[color=#D7E1EB]- {FormattedMessage.EscapeText(Loc.GetString("ccm-sponsorship-info-line-2"))}[/color]\n" +
            $"[color=#D7E1EB]- {FormattedMessage.EscapeText(Loc.GetString("ccm-sponsorship-info-line-3"))}[/color]"));

        stack.AddChild(notes);
        panel.AddChild(stack);
        return panel;
    }

    private void StyleWebsiteButton()
    {
        var accent = GetWebsiteAccent();

        _websiteButton.ModulateSelfOverride = Color.White;
        _websiteButton.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = _websiteButton.Disabled
                ? Color.Black.WithAlpha(0.18f)
                : MakeButtonBackground(accent, 0.20f, 0.96f),
            BorderColor = _websiteButton.Disabled
                ? StyleNano.LobbyMenuButtonBase.WithAlpha(0.24f)
                : accent.WithAlpha(0.86f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 12,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 12,
            ContentMarginBottomOverride = 4,
        };
        _websiteButton.Label.FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
        _websiteButton.Label.FontColorOverride = _websiteButton.Disabled
            ? Color.FromHex("#76808C")
            : accent;
    }

    private void ApplyWebsiteButtonState(bool pressed)
    {
        var accent = GetWebsiteAccent();

        _websiteButton.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = pressed
                ? accent.WithAlpha(0.92f)
                : MakeButtonBackground(accent, 0.28f, 0.98f),
            BorderColor = pressed
                ? accent
                : accent.WithAlpha(0.92f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 12,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 12,
            ContentMarginBottomOverride = 4,
        };
        _websiteButton.Label.FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
        _websiteButton.Label.FontColorOverride = pressed ? Color.Black : accent;
    }

    private Color GetWebsiteAccent()
    {
        return _currentTier == CCMSponsorshipTier.None
            ? StyleNano.LobbyMenuButtonBase
            : GetTierAccent(_currentTier);
    }

    private static Color MakeButtonBackground(Color accent, float scale, float alpha)
    {
        return new Color(accent.R * scale, accent.G * scale, accent.B * scale, alpha);
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
}
