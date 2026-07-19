using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Content.Client._CCM.UserInterface.Controls;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Shared._RMC14.CCVar;
using Content.Shared._CCM.Sponsorship;
using Content.Shared._Forge.Sponsor;
using Robust.Shared.Configuration;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.IoC;
using Robust.Shared.Input;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._CCM.Sponsorship;

public sealed class CCMSponsorshipWindow : DefaultCMWindow
{
    private const string DefaultDonateUrl = "https://boosty.to/corvaxforge";
    private const float DefaultWindowWidth = 1380f;
    private const float DefaultWindowHeight = 930f;
    private const float CompactMinWidth = 760f;
    private const float CompactMinHeight = 620f;
    private const float CompactViewportWidthThreshold = 1320f;

    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    private static readonly Color SponsorshipBlueAccent = Color.FromHex("#62C7FF");
    private static readonly Color SponsorshipGoldAccent = Color.FromHex("#FFC54D");
    private static readonly Color SponsorshipPurpleAccent = Color.FromHex("#E16BFF");

    private readonly Label _statusLabel;
    private readonly Label _expirationLabel;
    private readonly Button _websiteButton;
    private readonly BoxContainer _tiersContainer;
    private PanelContainer _heroPanel = default!;
    private PanelContainer _heroAccentLine = default!;
    private Label _heroTitleLabel = default!;
    private PanelContainer _infoPanel = default!;
    private Label _infoTitleLabel = default!;
    private string _donateUrl = DefaultDonateUrl;
    private SponsorLevel _currentLevel = SponsorLevel.None;

    public event Action<string>? OpenDonateRequested;

    public CCMSponsorshipWindow()
    {
        IoCManager.InjectDependencies(this);
        Stylesheet = IoCManager.Resolve<IStylesheetManager>().SheetNano;

        Title = string.Empty;
        MinSize = SetSize = new Vector2(DefaultWindowWidth, DefaultWindowHeight);
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
        _heroPanel = BuildHeroPanel(topSection);
        root.AddChild(_heroPanel);

        var scroll = new ScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            HScrollEnabled = false,
            MinSize = new Vector2(0, 560),
        };

        _tiersContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 22,
            HorizontalAlignment = HAlignment.Center,
            HorizontalExpand = true,
            VerticalExpand = false,
        };

        scroll.AddChild(_tiersContainer);
        root.AddChild(scroll);
        _infoPanel = BuildSponsorInfoBlock();
        root.AddChild(_infoPanel);
        Contents.AddChild(root);

        ApplyWindowTheme();
        BuildTierCards(SponsorLevel.None);
        UpdateStatusHeader(SponsorLevel.None, 0);
        StyleWebsiteButton();
        _config.OnValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged, false);
        _config.OnValueChanged(RMCCVars.RMCLobbyUiStyle, OnThemeChanged, false);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        UpdateResponsiveLayout();
        base.FrameUpdate(args);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _config.UnsubValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged);
            _config.UnsubValueChanged(RMCCVars.RMCLobbyUiStyle, OnThemeChanged);
    }

    private void OnThemeChanged(string _)
    {
        ApplyWindowTheme();
    }

    private void UpdateResponsiveLayout()
    {
        var viewport = Parent?.Size ?? Vector2.Zero;
        if (viewport.X <= 1f || viewport.Y <= 1f)
            return;

        var maxWidth = MathF.Min(DefaultWindowWidth, viewport.X * 0.92f);
        var maxHeight = MathF.Min(DefaultWindowHeight, viewport.Y * 0.88f);
        var minWidth = MathF.Min(CompactMinWidth, maxWidth);
        var minHeight = MathF.Min(CompactMinHeight, maxHeight);
        var responsiveMin = new Vector2(minWidth, minHeight);

        if (Vector2.DistanceSquared(MinSize, responsiveMin) > 1f)
            MinSize = responsiveMin;

        var clampedSize = new Vector2(
            Math.Clamp(SetSize.X, minWidth, maxWidth),
            Math.Clamp(SetSize.Y, minHeight, maxHeight));

        if (Vector2.DistanceSquared(SetSize, clampedSize) > 1f)
            SetSize = clampedSize;

        var compactWidth = maxWidth <= CompactViewportWidthThreshold;
        _tiersContainer.Orientation = compactWidth
            ? BoxContainer.LayoutOrientation.Vertical
            : BoxContainer.LayoutOrientation.Horizontal;
        _tiersContainer.SeparationOverride = compactWidth ? 14 : 22;
        _tiersContainer.HorizontalAlignment = HAlignment.Center;
    }

    public void SetStatus(CCMSponsorshipStatusSnapshot snapshot)
    {
        _donateUrl = string.IsNullOrWhiteSpace(snapshot.DonateUrl)
            ? DefaultDonateUrl
            : snapshot.DonateUrl;
        _currentLevel = snapshot.Level;
        _websiteButton.Disabled = false;
        UpdateStatusHeader(snapshot.Level, snapshot.ExpirationUnixSeconds);
        BuildTierCards(snapshot.Level);
        StyleWebsiteButton();
    }

    private void UpdateStatusHeader(SponsorLevel level, long expirationUnixSeconds)
    {
        _statusLabel.Text = Loc.GetString("ccm-sponsorship-current-tier",
            ("tier", GetLevelDisplayName(level)));

        // У нас нет источника даты окончания подписки, поэтому строку "Подписка не активна"
        // не показываем вовсе — метка скрывается, когда даты нет.
        if (expirationUnixSeconds > 0)
        {
            _expirationLabel.Visible = true;
            _expirationLabel.Text = Loc.GetString("ccm-sponsorship-expires",
                ("date", DateTimeOffset.FromUnixTimeSeconds(expirationUnixSeconds)
                    .ToLocalTime()
                    .ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)));
        }
        else
        {
            _expirationLabel.Visible = false;
            _expirationLabel.Text = string.Empty;
        }
    }

    private void BuildTierCards(SponsorLevel currentLevel)
    {
        _tiersContainer.DisposeAllChildren();

        // Карточки соответствуют трём порогам перков (L1/L2/L3); уровни L4-L6 наследуют порог L3.
        var currentThreshold = ThresholdLevel(currentLevel);
        foreach (var cardLevel in new[] { SponsorLevel.Level1, SponsorLevel.Level3, SponsorLevel.Level2 })
        {
            _tiersContainer.AddChild(BuildTierCard(cardLevel, cardLevel == currentThreshold, cardLevel == SponsorLevel.Level3));
        }
    }

    private static SponsorLevel ThresholdLevel(SponsorLevel level)
    {
        return level switch
        {
            >= SponsorLevel.Level3 => SponsorLevel.Level3,
            SponsorLevel.Level2 => SponsorLevel.Level2,
            SponsorLevel.Level1 => SponsorLevel.Level1,
            _ => SponsorLevel.None,
        };
    }

    private static string GetLevelDisplayName(SponsorLevel level)
    {
        return level == SponsorLevel.None
            ? Loc.GetString("ccm-sponsorship-tier-none-title")
            : SponsorData.SponsorNames.GetValueOrDefault(level, level.ToString());
    }

    private PanelContainer BuildHeroPanel(Control topSection)
    {
        var hero = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.36f),
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
            SeparationOverride = 12,
        };

        _heroAccentLine = new PanelContainer
        {
            MinSize = new Vector2(0, 5),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = GetWindowAccent().WithAlpha(0.90f),
            },
        };
        stack.AddChild(_heroAccentLine);

        _heroTitleLabel = new Label
        {
            Text = Loc.GetString("ccm-sponsorship-header"),
            HorizontalAlignment = HAlignment.Left,
            FontColorOverride = StyleNano.LobbyMenuButtonBase,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 24),
            HorizontalExpand = true,
        };
        stack.AddChild(_heroTitleLabel);

        stack.AddChild(topSection);
        stack.AddChild(new Label
        {
            Text = Loc.GetString("ccm-sponsorship-intro"),
            FontColorOverride = Color.FromHex("#BAC7D4"),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 13),
        });

        hero.AddChild(stack);
        return hero;
    }

    private Control BuildTierCard(SponsorLevel level, bool current, bool featured)
    {
        var accent = GetTierAccent(level);
        var baseBackground = BlendTowards(GetTierCardBackground(level), Color.Black, 0.10f);
        var imageBackground = BlendTowards(GetTierImageBackground(level), Color.Black, 0.08f);
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
                BackgroundColor = baseBackground.WithAlpha(1.00f),
                BorderColor = accent.WithAlpha(current ? 0.98f : 0.94f),
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
                BackgroundColor = accent.WithAlpha(0.96f),
            },
        });

        var titleText = GetLevelDisplayName(level);
        // Длинные названия (например "Подмастерье Форжа") не помещаются в карточку при базовом
        // размере шрифта и вылезали за рамку — подбираем максимальный размер, который влезает.
        var fittedTitleSize = FitFontSize("/Fonts/Exo2/Exo2-Bold.ttf", titleText, titleSize, cardWidth - 28);
        content.AddChild(new Label
        {
            Text = titleText,
            HorizontalAlignment = HAlignment.Center,
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", fittedTitleSize),
            FontColorOverride = accent,
        });

        if (current)
        {
            var badge = new PanelContainer
            {
                HorizontalAlignment = HAlignment.Center,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = accent.WithAlpha(0.32f),
                    BorderColor = accent.WithAlpha(0.74f),
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
                BackgroundColor = imageBackground.WithAlpha(1.00f),
                BorderColor = accent.WithAlpha(0.62f),
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
        perks.SetMessage(BuildPerksMessage(level));

        content.AddChild(perks);
        panel.AddChild(content);
        return panel;
    }

    private int FitFontSize(string fontPath, string text, int maxSize, float maxWidth, int minSize = 14)
    {
        for (var size = maxSize; size > minSize; size--)
        {
            if (MeasureTextWidth(_resourceCache.GetFont(fontPath, size), text) <= maxWidth)
                return size;
        }

        return minSize;
    }

    private static float MeasureTextWidth(Font font, string text)
    {
        var width = 0f;
        foreach (var rune in text.EnumerateRunes())
        {
            if (font.TryGetCharMetrics(rune, 1f, out var metrics))
                width += metrics.Advance;
        }

        return width;
    }

    private static Color GetTierAccent(SponsorLevel level)
    {
        return level switch
        {
            >= SponsorLevel.Level3 => SponsorshipGoldAccent,
            SponsorLevel.Level2 => SponsorshipPurpleAccent,
            SponsorLevel.Level1 => SponsorshipBlueAccent,
            _ => SponsorshipBlueAccent,
        };
    }

    private static Color GetTierCardBackground(SponsorLevel level)
    {
        return level switch
        {
            >= SponsorLevel.Level3 => Color.FromHex("#3A311A"),
            SponsorLevel.Level2 => Color.FromHex("#39244A"),
            SponsorLevel.Level1 => Color.FromHex("#233E60"),
            _ => Color.FromHex("#233E60"),
        };
    }

    private static Color GetTierImageBackground(SponsorLevel level)
    {
        return level switch
        {
            >= SponsorLevel.Level3 => Color.FromHex("#5C4B1D"),
            SponsorLevel.Level2 => Color.FromHex("#6A3B8A"),
            SponsorLevel.Level1 => Color.FromHex("#447CB1"),
            _ => Color.FromHex("#447CB1"),
        };
    }

    private FormattedMessage BuildPerksMessage(SponsorLevel level)
    {
        var fontSize = level >= SponsorLevel.Level3 ? 12 : 11;
        var message = new FormattedMessage();

        foreach (var perkKey in GetTierPerkKeys(level))
        {
            message.AddMarkupOrThrow($"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size={fontSize}][color=#DCE5EE]- {Loc.GetString(perkKey)}[/color][/font]\n");
        }

        return message;
    }

    private static IReadOnlyList<string> GetTierPerkKeys(SponsorLevel level)
    {
        // На карточке показываем ТОЛЬКО новые для данного порога перки.
        // О том, что более высокий уровень включает все предыдущие, говорится в info-line-1.
        //   Level1  - приоритетный вход, цвет OOC, ckey в конце раунда
        //   Level2  - таймеры всех ролей, цвет LOOC, готовый OOC-тег, базовая кастомизация
        //   Level3+ - свой OOC-тег, скин призрака, скины ксеноморфов, расширенная кастомизация
        return level switch
        {
            >= SponsorLevel.Level3 =>
            [
                "ccm-sponsorship-perk-ooc-tag-custom",
                "ccm-sponsorship-perk-ghost-skin",
                "ccm-sponsorship-perk-xeno-skin",
                "ccm-sponsorship-extended-perk-customization"
            ],
            SponsorLevel.Level2 =>
            [
                "ccm-sponsorship-perk-role-timers",
                "ccm-sponsorship-perk-looc-color",
                "ccm-sponsorship-perk-ooc-tag-preset",
                "ccm-sponsorship-perk-customization"
            ],
            _ =>
            [
                "ccm-sponsorship-perk-queue",
                "ccm-sponsorship-perk-ooc-color",
                "ccm-sponsorship-perk-endgame-credits",
                "ccm-sponsorship-perk-thanks"
            ],
        };
    }

    private PanelContainer BuildSponsorInfoBlock()
    {
        var panel = new PanelContainer
        {
            MinSize = new Vector2(0, 90),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.30f),
                BorderColor = GetWindowAccent().WithAlpha(0.34f),
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

        _infoTitleLabel = new Label
        {
            Text = Loc.GetString("ccm-sponsorship-info-title"),
            FontColorOverride = GetWindowAccent(),
            FontOverride = _resourceCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13),
        };
        stack.AddChild(_infoTitleLabel);

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
                ? GetWindowAccent().WithAlpha(0.24f)
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
        return _currentLevel == SponsorLevel.None
            ? GetWindowAccent()
            : GetTierAccent(_currentLevel);
    }

    private static Color MakeButtonBackground(Color accent, float scale, float alpha)
    {
        return new Color(accent.R * scale, accent.G * scale, accent.B * scale, alpha);
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

    private void ApplyWindowTheme()
    {
        var theme = StyleNano.GetConfiguredTheme(_config);
        var windowAccent = GetWindowAccent();
        var headerColor = theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#171D24").WithAlpha(0.995f),
            _ => Color.FromHex("#041105").WithAlpha(0.995f),
        };
        var bodyColor = theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#1C232C").WithAlpha(0.995f),
            _ => Color.FromHex("#061507").WithAlpha(0.995f),
        };
        var borderColor = theme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#7B8898").WithAlpha(0.86f),
            _ => StyleNano.LobbyMenuButtonBase.WithAlpha(0.82f),
        };

        HeaderPanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = headerColor,
            BorderColor = borderColor,
            BorderThickness = new Thickness(1, 1, 1, 0),
        };

        BodyPanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = bodyColor,
            BorderColor = borderColor,
            BorderThickness = new Thickness(1, 0, 1, 1),
        };

        if (_heroPanel != null)
        {
            _heroPanel.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.36f),
                BorderColor = windowAccent.WithAlpha(0.46f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 14,
                ContentMarginTopOverride = 14,
                ContentMarginRightOverride = 14,
                ContentMarginBottomOverride = 14,
            };
        }

        if (_heroAccentLine != null)
        {
            _heroAccentLine.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = windowAccent.WithAlpha(0.96f),
            };
        }

        if (_heroTitleLabel != null)
            _heroTitleLabel.FontColorOverride = StyleNano.LobbyMenuButtonBase;

        if (_infoPanel != null)
        {
            _infoPanel.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.Black.WithAlpha(0.30f),
                BorderColor = windowAccent.WithAlpha(0.40f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 12,
                ContentMarginTopOverride = 10,
                ContentMarginRightOverride = 12,
                ContentMarginBottomOverride = 10,
            };
        }

        if (_infoTitleLabel != null)
            _infoTitleLabel.FontColorOverride = windowAccent;
    }

    private Color GetWindowAccent()
    {
        return StyleNano.LobbyMenuButtonBase;
    }
}
