// CM14 rework: non-RMC edit marker.
using System;
using System.Numerics;
using Content.Client._CCM.UserInterface.Controls;
using Content.Client.Info;
using Content.Client.Options.UI;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Shared.CCVar;
using Content.Shared.Localizations;
using Content.Shared._RMC14.CCVar;
using Robust.Client;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._CCM.Lobby;

public sealed class CCMLobbyWelcomeWindow : DefaultCMWindow
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IGameController _gameController = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly ContentLocalizationManager _contentLoc = default!;

    private readonly Font _titleFont;
    private readonly Font _sectionFont;
    private readonly Font _smallFont;
    private readonly ISawmill _sawmill;

    private readonly PanelContainer _heroPanel;
    private readonly PanelContainer _heroAccentLine;
    private readonly Label _titleLabel;
    private readonly RichTextLabel _subtitleLabel;
    private readonly RichTextLabel _supportLabel;
    private readonly BoxContainer _root;
    private readonly ScrollContainer _pageScroll;
    private readonly BoxContainer _pageRoot;
    private readonly BoxContainer _welcomePage;
    private readonly BoxContainer _rulesPage;
    private BoxContainer _welcomeRightColumn = default!;
    private RichTextLabel _welcomeBody = default!;
    private RichTextLabel _projectBody = default!;
    private PanelContainer _welcomeInfoCard = default!;
    private PanelContainer _languageCard = default!;
    private PanelContainer _themeCard = default!;
    private PanelContainer _rulesContentCard = default!;
    private Label _languageHeader = default!;
    private Label _themeHeader = default!;
    private Label _lobbyStyleHeader = default!;
    private CheckBox _chatTranslateCheckBox = default!;
    private RichTextLabel _languageHint = default!;
    private RichTextLabel _themeHint = default!;
    private RichTextLabel _lobbyStyleHint = default!;
    private CCMOptionButton _languageSelector = default!;
    private Button _greenThemeButton = default!;
    private Button _grayThemeButton = default!;
    private Button _newLobbyStyleButton = default!;
    private Button _oldLobbyStyleButton = default!;
    private RulesControl _rulesControl = default!;
    private readonly Button _backButton;
    private readonly Button _nextButton;
    private readonly Button _finishButton;
    private LanguageRestartConfirmWindow? _languageRestartWindow;
    private bool _ignoreLanguageSelection;
    private bool _centerAfterLayout;
    private string? _pendingLanguageCode;
    private string? _pendingPreviousLanguageCode;

    private WelcomePage _currentPage = WelcomePage.Intro;

    private const int LanguageRussian = 0;
    private const int LanguageEnglish = 1;
    private const float WelcomeMinWidth = 560f;
    private const float WelcomeMinHeight = 480f;
    private const string LobbyUiStyleNewClass = "LobbyUiStyleNew";
    private const string LobbyUiStyleOldClass = "LobbyUiStyleOld";
    public event Action? OnFinished;

    public CCMLobbyWelcomeWindow()
    {
        IoCManager.InjectDependencies(this);
        Stylesheet = IoCManager.Resolve<IStylesheetManager>().SheetNano;
        _sawmill = _logManager.GetSawmill("language-restart");

        Title = string.Empty;
        SetSize = new Vector2(700, 540);
        MinSize = new Vector2(WelcomeMinWidth, WelcomeMinHeight);

        _titleFont = _cache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 28);
        _sectionFont = _cache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 18);
        _smallFont = _cache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 11);

        _root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 10,
            Margin = new Thickness(12),
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        Contents.AddChild(_root);

        _heroPanel = new PanelContainer
        {
            HorizontalExpand = true,
            MinSize = new Vector2(0, 170),
        };
        _root.AddChild(_heroPanel);

        var heroContent = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 7,
            Margin = new Thickness(14, 12, 14, 12),
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        _heroPanel.AddChild(heroContent);

        _heroAccentLine = new PanelContainer
        {
            MinSize = new Vector2(0, 3),
            HorizontalExpand = true,
        };
        heroContent.AddChild(_heroAccentLine);

        _titleLabel = new Label
        {
            FontOverride = _titleFont,
            HorizontalAlignment = HAlignment.Left,
        };
        heroContent.AddChild(_titleLabel);

        _subtitleLabel = new RichTextLabel
        {
            HorizontalExpand = true,
            VerticalExpand = false,
            MinSize = new Vector2(0f, 32f),
        };
        heroContent.AddChild(_subtitleLabel);

        _supportLabel = new RichTextLabel
        {
            HorizontalExpand = true,
            VerticalExpand = false,
            MinSize = new Vector2(0f, 24f),
        };
        heroContent.AddChild(_supportLabel);

        _pageScroll = new ScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            HScrollEnabled = false,
        };
        _root.AddChild(_pageScroll);

        _pageRoot = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        _pageScroll.AddChild(_pageRoot);

        _welcomePage = BuildWelcomePage();
        _rulesPage = BuildRulesPage();
        _pageRoot.AddChild(_welcomePage);
        _pageRoot.AddChild(_rulesPage);

        var footer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
        };
        _root.AddChild(footer);

        _backButton = CreateMenuButton();
        _nextButton = CreateMenuButton();
        _finishButton = CreateMenuButton();

        footer.AddChild(new Control { HorizontalExpand = true });
        footer.AddChild(_backButton);
        footer.AddChild(_nextButton);
        footer.AddChild(_finishButton);
        footer.AddChild(new Control { HorizontalExpand = true });

        _backButton.OnPressed += _ => SetPage(WelcomePage.Intro);
        _nextButton.OnPressed += _ => SetPage(WelcomePage.Rules);
        _finishButton.OnPressed += _ =>
        {
            OnFinished?.Invoke();
            CloseAnimated();
        };

        _languageSelector.OnItemSelected += args =>
        {
            if (_ignoreLanguageSelection)
                return;

            _languageSelector.SelectId(args.Id);
            var locale = args.Id == LanguageEnglish ? "en-US" : "ru-RU";
            var currentLocale = _config.GetCVar(CCVars.ClientLocale);
            if (string.Equals(locale, currentLocale, StringComparison.OrdinalIgnoreCase))
                return;

            _pendingLanguageCode = locale;
            _pendingPreviousLanguageCode = currentLocale;
            OpenLanguageRestartConfirm();
        };

        _config.OnValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged, false);
        _config.OnValueChanged(RMCCVars.RMCLobbyUiStyle, OnLobbyStyleChanged, false);

        PopulateSelectors();
        RefreshLocalization();
        ApplyTheme();
        SetPage(WelcomePage.Intro);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _config.UnsubValueChanged(RMCCVars.RMCUIColorTheme, OnThemeChanged);
        _config.UnsubValueChanged(RMCCVars.RMCLobbyUiStyle, OnLobbyStyleChanged);
        _languageRestartWindow?.Dispose();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        UpdateResponsiveLayout();
        base.FrameUpdate(args);
    }

    public new void OpenCenteredAnimated()
    {
        _centerAfterLayout = true;
        base.OpenCenteredAnimated();
    }

    private void UpdateResponsiveLayout()
    {
        var viewport = Parent?.Size ?? Vector2.Zero;
        if (viewport.X <= 1f || viewport.Y <= 1f)
            return;

        var maxWidth = viewport.X * 0.58f;
        var maxHeight = viewport.Y * 0.78f;
        var minWidth = MathF.Min(WelcomeMinWidth, maxWidth);
        var minHeight = MathF.Min(WelcomeMinHeight, maxHeight);
        var targetSize = new Vector2(
            Math.Clamp(viewport.X * 0.48f, minWidth, maxWidth),
            Math.Clamp(viewport.Y * 0.68f, minHeight, maxHeight));

        if (Vector2.DistanceSquared(SetSize, targetSize) > 1f)
        {
            SetSize = targetSize;
            _centerAfterLayout = true;
        }

        var lowHeight = viewport.Y <= 800f;
        var compact = viewport.Y <= 900f;
        var outerMargin = lowHeight ? 10f : compact ? 11f : 12f;
        var rootGap = lowHeight ? 8 : 10;
        _root.Margin = new Thickness(outerMargin);
        _root.SeparationOverride = rootGap;

        var heroHeight = lowHeight ? 126f : compact ? 136f : 146f;
        _heroPanel.MinSize = new Vector2(0f, heroHeight);
        _heroPanel.SetHeight = heroHeight;

        _welcomePage.SeparationOverride = lowHeight ? 8 : 12;
        _welcomeRightColumn.SeparationOverride = lowHeight ? 10 : 12;
        var buttonHeight = lowHeight ? 34f : 36f;
        var buttonWidth = lowHeight ? 132f : 150f;
        _backButton.MinSize = new Vector2(buttonWidth, buttonHeight);
        _nextButton.MinSize = new Vector2(buttonWidth, buttonHeight);
        _finishButton.MinSize = new Vector2(buttonWidth, buttonHeight);

        var innerWidth = MathF.Max(0f, targetSize.X - outerMargin * 2f);
        var gap = lowHeight ? 8f : 12f;
        var minRightWidth = lowHeight ? 260f : 300f;
        var minLeftWidth = lowHeight ? 220f : 250f;
        var rightWidth = Math.Clamp(innerWidth * 0.44f, minRightWidth, 390f);
        var leftWidth = innerWidth - rightWidth - gap;
        if (leftWidth < minLeftWidth)
        {
            leftWidth = minLeftWidth;
            rightWidth = MathF.Max(minRightWidth, innerWidth - leftWidth - gap);
        }

        leftWidth = MathF.Max(220f, leftWidth);
        rightWidth = MathF.Max(200f, rightWidth);

        _welcomeInfoCard.SetWidth = leftWidth;
        _welcomeInfoCard.MinSize = new Vector2(leftWidth, 0f);
        _welcomeInfoCard.MaxSize = new Vector2(leftWidth, float.MaxValue);
        _welcomeRightColumn.SetWidth = rightWidth;
        _welcomeRightColumn.MinSize = new Vector2(rightWidth, 0f);
        _welcomeRightColumn.MaxSize = new Vector2(rightWidth, float.MaxValue);

        if (!_centerAfterLayout)
            return;

        LayoutContainer.SetPosition(this, Vector2.Max(Vector2.Zero, (viewport - targetSize) / 2f));
        _centerAfterLayout = false;
    }

    public void RefreshLocalization()
    {
        _titleLabel.Text = Loc.GetString("ccm-lobby-welcome-title");

        _backButton.Text = Loc.GetString("ccm-lobby-welcome-button-back");
        _nextButton.Text = Loc.GetString("ccm-lobby-welcome-button-next");
        _finishButton.Text = Loc.GetString("ccm-lobby-welcome-button-finish");

        _languageHeader.Text = Loc.GetString("ccm-lobby-welcome-language-title");
        _themeHeader.Text = Loc.GetString("ccm-lobby-welcome-theme-title");
        _chatTranslateCheckBox.Text = Loc.GetString("ccm-lobby-welcome-language-chat-translate");
        _languageHint.SetMessage(FormattedMessage.FromMarkupOrThrow(
            $"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size=12][color=#D7E1EB]{Loc.GetString("ccm-lobby-welcome-language-text") }[/color][/font]"));
        _themeHint.SetMessage(FormattedMessage.FromMarkupOrThrow(
            $"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size=12][color=#D7E1EB]{Loc.GetString("ccm-lobby-welcome-theme-text") }[/color][/font]"));
        _lobbyStyleHeader.Text = Loc.GetString("ccm-lobby-welcome-lobby-style-title");
        _lobbyStyleHint.SetMessage(FormattedMessage.FromMarkupOrThrow(
            $"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size=12][color=#D7E1EB]{Loc.GetString("ccm-lobby-welcome-lobby-style-text")}[/color][/font]"));

        PopulateSelectors();
        ApplyTheme();
    }

    private BoxContainer BuildWelcomePage()
    {
        var page = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 12,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        _welcomeInfoCard = BuildCard();
        _welcomeInfoCard.HorizontalExpand = false;
        _welcomeInfoCard.VerticalExpand = true;
        _welcomeInfoCard.MinSize = new Vector2(430, 0);
        _welcomeInfoCard.MaxSize = new Vector2(430, float.MaxValue);

        var leftContent = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 10,
            Margin = new Thickness(14),
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        _welcomeInfoCard.AddChild(leftContent);

        _welcomeBody = new RichTextLabel { HorizontalExpand = true };
        _projectBody = new RichTextLabel { HorizontalExpand = true, VerticalExpand = true };
        leftContent.AddChild(_welcomeBody);
        leftContent.AddChild(_projectBody);

        _welcomeRightColumn = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 12,
            HorizontalExpand = false,
            SizeFlagsStretchRatio = 1f,
            MinSize = new Vector2(320, 0),
        };

        _languageCard = BuildCard();
        var languageContent = BuildSettingsCardContent(_languageCard, out _languageHeader, out _languageHint, out _languageSelector);
        languageContent.AddChild(BuildChatTranslateCheckBox());

        _themeCard = BuildCard();
        var themeContent = BuildSettingsCardContent(_themeCard, out _themeHeader, out _themeHint);
        themeContent.AddChild(BuildThemeButtons());
        themeContent.AddChild(BuildLobbyStyleSection());

        _welcomeRightColumn.AddChild(_languageCard);
        _welcomeRightColumn.AddChild(_themeCard);

        page.AddChild(_welcomeInfoCard);
        page.AddChild(_welcomeRightColumn);
        return page;
    }

    private BoxContainer BuildRulesPage()
    {
        var page = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        _rulesContentCard = BuildCard();
        _rulesContentCard.HorizontalExpand = true;
        _rulesContentCard.VerticalExpand = true;
        var rulesContent = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(10),
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        _rulesContentCard.AddChild(rulesContent);
        _rulesControl = new RulesControl
        {
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        rulesContent.AddChild(_rulesControl);

        page.AddChild(_rulesContentCard);
        return page;
    }

    private PanelContainer BuildCard()
    {
        return new PanelContainer
        {
            MinSize = new Vector2(0, 0),
        };
    }

    private BoxContainer BuildSettingsCardContent(PanelContainer parent, out Label title, out RichTextLabel hint, out CCMOptionButton selector)
    {
        var content = BuildSettingsCardContent(parent, out title, out hint);
        selector = new CCMOptionButton
        {
            HorizontalExpand = true,
        };

        content.AddChild(selector);
        return content;
    }

    private BoxContainer BuildSettingsCardContent(PanelContainer parent, out Label title, out RichTextLabel hint)
    {
        var content = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 8,
            Margin = new Thickness(14, 12, 14, 12),
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        parent.AddChild(content);

        title = new Label
        {
            FontOverride = _sectionFont,
            HorizontalExpand = true,
        };
        hint = new RichTextLabel
        {
            HorizontalExpand = true,
            VerticalExpand = false,
        };

        content.AddChild(title);
        content.AddChild(hint);
        return content;
    }

    private CheckBox BuildChatTranslateCheckBox()
    {
        _chatTranslateCheckBox = new CheckBox
        {
            HorizontalExpand = true,
            ToggleMode = true,
            Pressed = _config.GetCVar(RMCCVars.RMCChatTranslateEnabled),
            Margin = new Thickness(0, 2, 0, 0),
        };

        _chatTranslateCheckBox.Label.FontOverride = _smallFont;
        _chatTranslateCheckBox.OnToggled += args =>
        {
            _config.SetCVar(RMCCVars.RMCChatTranslateEnabled, args.Pressed);
            _config.SaveToFile();
        };

        return _chatTranslateCheckBox;
    }

    private Control BuildThemeButtons()
    {
        var buttonRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
        };

        _greenThemeButton = CreateThemeButton();
        _greenThemeButton.OnPressed += _ => SetTheme("green");
        _grayThemeButton = CreateThemeButton();
        _grayThemeButton.OnPressed += _ => SetTheme("gray");

        buttonRow.AddChild(_greenThemeButton);
        buttonRow.AddChild(_grayThemeButton);
        return buttonRow;
    }

    private Control BuildLobbyStyleSection()
    {
        var section = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 8,
            Margin = new Thickness(0, 6, 0, 0),
            HorizontalExpand = true,
        };

        _lobbyStyleHeader = new Label
        {
            FontOverride = _sectionFont,
            HorizontalExpand = true,
        };

        _lobbyStyleHint = new RichTextLabel
        {
            HorizontalExpand = true,
            VerticalExpand = false,
        };

        section.AddChild(_lobbyStyleHeader);
        section.AddChild(_lobbyStyleHint);
        section.AddChild(BuildLobbyStyleButtons());
        return section;
    }

    private Control BuildLobbyStyleButtons()
    {
        var buttonRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            HorizontalExpand = true,
        };

        _newLobbyStyleButton = CreateThemeButton();
        _newLobbyStyleButton.OnPressed += _ => SetLobbyUiStyle("new");
        _oldLobbyStyleButton = CreateThemeButton();
        _oldLobbyStyleButton.OnPressed += _ => SetLobbyUiStyle("old");

        buttonRow.AddChild(_newLobbyStyleButton);
        buttonRow.AddChild(_oldLobbyStyleButton);
        return buttonRow;
    }

    private Button CreateThemeButton()
    {
        var button = new Button
        {
            HorizontalExpand = true,
            MinSize = new Vector2(0, 42),
            ToggleMode = true,
        };

        button.Label.Align = Label.AlignMode.Center;
        button.Label.Margin = new Thickness(0);
        button.Label.HorizontalAlignment = HAlignment.Center;
        button.Label.VerticalAlignment = VAlignment.Center;
        button.Label.FontOverride = _smallFont;
        return button;
    }

    private Button CreateMenuButton()
    {
        var button = new Button
        {
            StyleClasses =
            {
                StyleNano.StyleClassLobbyMenuButton,
                _config.GetCVar(RMCCVars.RMCLobbyCrtEnabled)
                    ? StyleNano.StyleClassLobbyThemeCrt
                    : StyleNano.StyleClassLobbyThemeClean,
            },
            MinSize = new Vector2(150, 36),
            HorizontalAlignment = HAlignment.Center,
        };

        button.Label.Align = Label.AlignMode.Center;
        button.Label.Margin = new Thickness(0);
        button.Label.HorizontalAlignment = HAlignment.Center;
        button.Label.VerticalAlignment = VAlignment.Center;
        button.Label.FontColorOverride = Color.Black;
        button.Label.FontColorShadowOverride = null;
        return button;
    }

    private void PopulateSelectors()
    {
        var currentLocale = _config.GetCVar(CCVars.ClientLocale) ?? "ru-RU";
        var currentTheme = _config.GetCVar(RMCCVars.RMCUIColorTheme) ?? "gray";
        var currentLobbyStyle = _config.GetCVar(RMCCVars.RMCLobbyUiStyle) ?? "new";

        _languageSelector.Clear();
        _chatTranslateCheckBox.Pressed = _config.GetCVar(RMCCVars.RMCChatTranslateEnabled);
        _languageSelector.AddItem(Loc.GetString("ccm-lobby-welcome-language-russian"), LanguageRussian);
        _languageSelector.AddItem(Loc.GetString("ccm-lobby-welcome-language-english"), LanguageEnglish);
        _languageSelector.SetItemTextColor(LanguageRussian, Color.FromHex("#F3F6FA"));
        _languageSelector.SetItemTextColor(LanguageEnglish, Color.FromHex("#F3F6FA"));
        _languageSelector.SelectId(currentLocale.Equals("en-US", StringComparison.OrdinalIgnoreCase) ? LanguageEnglish : LanguageRussian);

        _greenThemeButton.Text = Loc.GetString("ccm-lobby-welcome-theme-green");
        _grayThemeButton.Text = Loc.GetString("ccm-lobby-welcome-theme-gray");
        _newLobbyStyleButton.Text = Loc.GetString("ccm-lobby-welcome-lobby-style-new");
        _oldLobbyStyleButton.Text = Loc.GetString("ccm-lobby-welcome-lobby-style-old");
        ApplyThemeSwatchSelection(currentTheme);
        ApplyLobbyStyleSelection(currentLobbyStyle);
    }

    private void SetPage(WelcomePage page)
    {
        _currentPage = page;
        _welcomePage.Visible = page == WelcomePage.Intro;
        _rulesPage.Visible = page == WelcomePage.Rules;

        _backButton.Visible = page == WelcomePage.Rules;
        _nextButton.Visible = page == WelcomePage.Intro;
        _finishButton.Visible = page == WelcomePage.Rules;
    }

    private void ApplyTheme()
    {
        var oldStyle = string.Equals(_config.GetCVar(RMCCVars.RMCLobbyUiStyle), "old", StringComparison.OrdinalIgnoreCase);
        var effectiveTheme = StyleNano.GetConfiguredTheme(_config);
        var accent = oldStyle
            ? StyleNano.OldLobbyGold
            : StyleNano.LobbyMenuButtonBase;
        var bodyText = "#D7E1EB";
        var secondaryText = "#B4C2D2";
        var welcomeBodyText = "#E2EAF3";
        var body = oldStyle
            ? StyleNano.OldLobbyPanel.WithAlpha(0.97f)
            : (effectiveTheme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#171D24"),
            _ => Color.FromHex("#06170B"),
        }).WithAlpha(0.97f);
        var panel = oldStyle
            ? StyleNano.OldLobbyPanelSoft.WithAlpha(0.97f)
            : (effectiveTheme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#1D252E"),
            _ => Color.FromHex("#0A1B0C"),
        }).WithAlpha(0.97f);
        var subpanel = oldStyle
            ? Color.FromHex("#30343B").WithAlpha(0.97f)
            : (effectiveTheme switch
        {
            StyleNano.UiColorTheme.Gray => Color.FromHex("#232D38"),
            _ => Color.FromHex("#0D220F"),
        }).WithAlpha(0.97f);

        BodyPanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = body,
            BorderColor = accent.WithAlpha(0.82f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 0,
            ContentMarginTopOverride = 0,
            ContentMarginRightOverride = 0,
            ContentMarginBottomOverride = 0,
        };

        WindowTitleLabel.Text = string.Empty;
        _heroPanel.PanelOverride = BuildPanelStyle(panel, accent, 1, 14);
        _heroAccentLine.PanelOverride = new StyleBoxFlat { BackgroundColor = accent.WithAlpha(0.94f) };
        _languageCard.PanelOverride = BuildPanelStyle(subpanel, accent.WithAlpha(0.64f), 1, 12);
        _themeCard.PanelOverride = BuildPanelStyle(subpanel, accent.WithAlpha(0.64f), 1, 12);

        foreach (var panelContainer in new[] { _welcomeInfoCard, _rulesContentCard })
        {
            panelContainer.PanelOverride = BuildPanelStyle(panel, accent.WithAlpha(0.64f), 1, 12);
        }

        _titleLabel.FontColorOverride = accent;
        _languageHeader.FontColorOverride = accent;
        _themeHeader.FontColorOverride = accent;
        _lobbyStyleHeader.FontColorOverride = accent;
        _chatTranslateCheckBox.Label.FontColorOverride = Color.FromHex(bodyText);
        _chatTranslateCheckBox.Label.FontColorShadowOverride = null;

        var themeClass = _config.GetCVar(RMCCVars.RMCLobbyCrtEnabled)
            ? StyleNano.StyleClassLobbyThemeCrt
            : StyleNano.StyleClassLobbyThemeClean;

        foreach (var button in new[] { _backButton, _nextButton, _finishButton })
        {
            button.StyleClasses.Clear();
            button.StyleClasses.Add(StyleNano.StyleClassLobbyMenuButton);
            button.StyleClasses.Add(themeClass);
            button.Label.FontColorOverride = Color.Black;
            button.Label.FontColorShadowOverride = null;
        }

        _subtitleLabel.SetMessage(FormattedMessage.FromMarkupOrThrow(
            $"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size=13][color={bodyText}]{Loc.GetString("ccm-lobby-welcome-subtitle")}[/color][/font]"));
        _languageHint.SetMessage(FormattedMessage.FromMarkupOrThrow(
            $"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size=12][color={bodyText}]{Loc.GetString("ccm-lobby-welcome-language-text")}[/color][/font]"));
        _themeHint.SetMessage(FormattedMessage.FromMarkupOrThrow(
            $"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size=12][color={bodyText}]{Loc.GetString("ccm-lobby-welcome-theme-text")}[/color][/font]"));
        _lobbyStyleHint.SetMessage(FormattedMessage.FromMarkupOrThrow(
            $"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size=12][color={bodyText}]{Loc.GetString("ccm-lobby-welcome-lobby-style-text")}[/color][/font]"));
        _supportLabel.SetMessage(FormattedMessage.FromMarkupOrThrow(
            $"[font=\"/Fonts/Exo2/Exo2-Bold.ttf\" size=12][color={accent.ToHex()}]{Loc.GetString("ccm-lobby-welcome-support")}[/color][/font]"));
        _welcomeBody.SetMessage(FormattedMessage.FromMarkupOrThrow(
            $"[font=\"/Fonts/Exo2/Exo2-Bold.ttf\" size=18][color={accent.ToHex()}]{Loc.GetString("ccm-lobby-welcome-page1-title")}[/color][/font]\n\n" +
            $"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size=13][color={welcomeBodyText}]{Loc.GetString("ccm-lobby-welcome-page1-body")}[/color][/font]"));
        _projectBody.SetMessage(FormattedMessage.FromMarkupOrThrow(
            $"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size=13][color={bodyText}]{Loc.GetString("ccm-lobby-welcome-page1-project-body")}[/color][/font]\n" +
            $"[font=\"/Fonts/Exo2/Exo2-Regular.ttf\" size=12][color={secondaryText}]{Loc.GetString("ccm-lobby-welcome-page1-command", ("command", "welcome"))}[/color][/font]"));

        ApplyThemeSwatchSelection(_config.GetCVar(RMCCVars.RMCUIColorTheme) ?? "gray");
    }

    private void OnThemeChanged(string _)
    {
        ApplyTheme();
    }

    private void OnLobbyStyleChanged(string style)
    {
        ApplyLobbyStyleSelection(style);
    }

    private void SetTheme(string theme)
    {
        if (_config.GetCVar(RMCCVars.RMCUIColorTheme) == theme)
            return;

        _config.SetCVar(RMCCVars.RMCUIColorTheme, theme);
        _config.SaveToFile();
    }

    private void SetLobbyUiStyle(string style)
    {
        var current = _config.GetCVar(RMCCVars.RMCLobbyUiStyle) ?? "new";
        if (current.Equals(style, StringComparison.OrdinalIgnoreCase))
            return;

        _config.SetCVar(RMCCVars.RMCLobbyUiStyle, style);
        _config.SaveToFile();
    }

    private void OpenLanguageRestartConfirm()
    {
        if (_languageRestartWindow is { Disposed: false, IsOpen: true })
        {
            _languageRestartWindow.MoveToFront();
            return;
        }

        _languageRestartWindow?.Dispose();
        _languageRestartWindow = new LanguageRestartConfirmWindow();
        _languageRestartWindow.Confirmed += ConfirmLanguageRestart;
        _languageRestartWindow.Cancelled += CancelLanguageRestart;
        _languageRestartWindow.OpenCentered();
        _languageRestartWindow.MoveToFront();
    }

    private void ConfirmLanguageRestart()
    {
        if (string.IsNullOrWhiteSpace(_pendingLanguageCode))
            return;

        var language = _pendingLanguageCode;
        _pendingLanguageCode = null;
        _pendingPreviousLanguageCode = null;

        var chatTranslateTarget = string.Equals(language, "en-US", StringComparison.OrdinalIgnoreCase)
            ? "en"
            : "ru";

        _config.SetCVar(RMCCVars.RMCChatTranslateTarget, chatTranslateTarget);
        _config.SetCVar(CCVars.ClientLocale, language);
        _contentLoc.SetCulture(language);
        _config.SaveToFile();

        if (!RestartClientProcess())
            _sawmill.Error("Language changed and saved from welcome menu, but automatic restart failed.");
    }

    private void CancelLanguageRestart()
    {
        _pendingLanguageCode = null;
        var language = _pendingPreviousLanguageCode ?? _config.GetCVar(CCVars.ClientLocale);
        _pendingPreviousLanguageCode = null;
        SelectLanguage(language);
    }

    private void SelectLanguage(string languageCode)
    {
        _ignoreLanguageSelection = true;
        try
        {
            _languageSelector.SelectId(languageCode.Equals("en-US", StringComparison.OrdinalIgnoreCase)
                ? LanguageEnglish
                : LanguageRussian);
        }
        finally
        {
            _ignoreLanguageSelection = false;
        }
    }

    private bool RestartClientProcess()
    {
        var address = BuildRedialAddress();
        if (string.IsNullOrWhiteSpace(address))
            return false;

        try
        {
            _gameController.Redial(address, "Language restart requested");
            return true;
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to restart via launcher redial: {e}");
            return false;
        }
    }

    private string? BuildRedialAddress()
    {
        var launchState = _gameController.LaunchState;

        if (!string.IsNullOrWhiteSpace(launchState.Ss14Address))
            return NormalizeSs14Address(launchState.Ss14Address);

        if (string.IsNullOrWhiteSpace(launchState.ConnectAddress))
            return null;

        var address = launchState.ConnectAddress.Trim();
        if (address.StartsWith("ss14://", StringComparison.OrdinalIgnoreCase))
            return NormalizeSs14Address(address);

        if (address.StartsWith("udp://", StringComparison.OrdinalIgnoreCase))
            address = address["udp://".Length..];

        address = address.TrimStart('/');
        return NormalizeSs14Address($"ss14://{address}");
    }

    private static string NormalizeSs14Address(string address)
    {
        var normalized = address.Trim();
        if (!normalized.EndsWith('/'))
            normalized += "/";

        return normalized;
    }

    private static StyleBoxFlat BuildPanelStyle(Color background, Color border, int borderSize, int margin)
    {
        return new StyleBoxFlat
        {
            BackgroundColor = background,
            BorderColor = border,
            BorderThickness = new Thickness(borderSize),
            ContentMarginLeftOverride = margin,
            ContentMarginTopOverride = margin,
            ContentMarginRightOverride = margin,
            ContentMarginBottomOverride = margin,
        };
    }

    private void ApplyThemeSwatchSelection(string theme)
    {
        _greenThemeButton.Pressed = theme.Equals("green", StringComparison.OrdinalIgnoreCase);
        _grayThemeButton.Pressed = theme.Equals("gray", StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyLobbyStyleSelection(string style)
    {
        var oldStyle = style.Equals("old", StringComparison.OrdinalIgnoreCase);
        _newLobbyStyleButton.Pressed = !oldStyle;
        _oldLobbyStyleButton.Pressed = oldStyle;
        ApplyExclusiveStyleClass(_root, oldStyle);
        ApplyExclusiveStyleClass(_welcomePage, oldStyle);
        ApplyExclusiveStyleClass(_rulesPage, oldStyle);
        ApplyExclusiveStyleClass(_heroPanel, oldStyle);
    }

    private static void ApplyExclusiveStyleClass(Control control, bool oldStyle)
    {
        if (oldStyle)
        {
            control.AddStyleClass(LobbyUiStyleOldClass);
            control.RemoveStyleClass(LobbyUiStyleNewClass);
        }
        else
        {
            control.AddStyleClass(LobbyUiStyleNewClass);
            control.RemoveStyleClass(LobbyUiStyleOldClass);
        }
    }

    private enum WelcomePage
    {
        Intro,
        Rules,
    }
}
