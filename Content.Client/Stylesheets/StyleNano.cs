using System;
using System.Linq;
using System.Numerics;
using Content.Client._RMC14;
using Content.Client.ContextMenu.UI;
using Content.Client.Examine;
using Content.Client.PDA;
using Content.Client.Resources;
using Content.Client.Silicons.Laws.SiliconLawEditUi;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Controls.FancyTree;
using Content.Client.Verbs.UI;
using Content.Shared.Verbs;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.StylesheetHelpers;

namespace Content.Client.Stylesheets
{
    public static class ResCacheExtension
    {
        public static Font NotoStack(this IResourceCache resCache, string variation = "Regular", int size = 10, bool display = false)
        {
            var isBold = variation.StartsWith("Bold", StringComparison.Ordinal);
            var isItalic = variation.Contains("Italic", StringComparison.Ordinal);
            var style = isItalic
                ? (isBold ? "BoldItalic" : "Italic")
                : (isBold ? "Bold" : "Regular");
            return resCache.GetFont
            (
                // Ew, but ok
                new[]
                {
                    $"/Fonts/Exo2/Exo2-{style}.ttf",
                    "/Fonts/Exo2/Exo2-Regular.ttf"
                },
                size
            );

        }

    }
    // STLYE SHEETS WERE A MISTAKE. KILL ALL OF THIS WITH FIRE
    public sealed class StyleNano : StyleBase
    {
        public enum UiColorTheme
        {
            Green,
            Blue
        }

        public static UiColorTheme CurrentTheme { get; private set; } = UiColorTheme.Green;
        public const string StyleClassBorderedWindowPanel = "BorderedWindowPanel";
        public const string StyleClassInventorySlotBackground = "InventorySlotBackground";
        public const string StyleClassHandSlotHighlight = "HandSlotHighlight";
        public const string StyleClassChatPanel = "ChatPanel";
        public const string StyleClassChatSubPanel = "ChatSubPanel";
        public const string StyleClassChatOutput = "ChatOutput";
        public const string StyleClassTransparentBorderedWindowPanel = "TransparentBorderedWindowPanel";
        public const string StyleClassHotbarPanel = "HotbarPanel";
        public const string StyleClassTooltipPanel = "tooltipBox";
        public const string StyleClassTooltipAlertTitle = "tooltipAlertTitle";
        public const string StyleClassTooltipAlertDescription = "tooltipAlertDesc";
        public const string StyleClassTooltipAlertCooldown = "tooltipAlertCooldown";
        public const string StyleClassTooltipActionTitle = "tooltipActionTitle";
        public const string StyleClassTooltipActionDescription = "tooltipActionDesc";
        public const string StyleClassTooltipActionCooldown = "tooltipActionCooldown";
        public const string StyleClassTooltipActionDynamicMessage = "tooltipActionDynamicMessage";
        public const string StyleClassTooltipActionRequirements = "tooltipActionCooldown";
        public const string StyleClassTooltipActionCharges = "tooltipActionCharges";
        public const string StyleClassHotbarSlotNumber = "hotbarSlotNumber";
        public const string StyleClassActionSearchBox = "actionSearchBox";
        public const string StyleClassActionMenuItemRevoked = "actionMenuItemRevoked";
        public const string StyleClassChatLineEdit = "chatLineEdit";
        public const string StyleClassChatChannelSelectorButton = "chatSelectorOptionButton";
        public const string StyleClassChatFilterOptionButton = "chatFilterOptionButton";
        public const string StyleClassStorageButton = "storageButton";
        public const string StyleClassInset = "Inset";
        public const string StyleClassLobbyThemeCrt = "LobbyThemeCrt";
        public const string StyleClassLobbyThemeClean = "LobbyThemeClean";
        public const string StyleClassLobbyCenterPanel = "LobbyCenterPanel";
        public const string StyleClassLobbyCenterGlow = "LobbyCenterGlow";
        public const string StyleClassLobbyInfoPanel = "LobbyInfoPanel";
        public const string StyleClassLobbyInfoDivider = "LobbyInfoDivider";
        public const string StyleClassLobbyWelcomeLine1 = "LobbyWelcomeLine1";
        public const string StyleClassLobbyWelcomeLine2 = "LobbyWelcomeLine2";
        public const string StyleClassLobbyWelcomeLine3 = "LobbyWelcomeLine3";
        public const string StyleClassLobbyCountdown = "LobbyCountdown";
        public const string StyleClassLobbyInfoTitle = "LobbyInfoTitle";
        public const string StyleClassLobbyMusicHeader = "LobbyMusicHeader";
        public const string StyleClassLobbyInfoLine = "LobbyInfoLine";
        public const string StyleClassLobbyInfoText = "LobbyInfoText";
        public const string StyleClassLobbyMenuButton = "LobbyMenuButton";
        public const string StyleClassLobbyReadyButton = "LobbyReadyButton";
        public const string StyleClassLobbyMenuDivider = "LobbyMenuDivider";
        public const string StyleClassLobbyMenuIconButton = "LobbyMenuIconButton";
        public const string StyleClassLobbyTopButton = "LobbyTopButton";
        public const string StyleClassLobbyChatPanel = "LobbyChatPanel";
        public const string StyleClassLobbyMusicPanel = "LobbyMusicPanel";
        public const string StyleClassLobbyChatPanelInner = "LobbyChatPanelInner";
        public const string StyleClassLobbyChatInputPanel = "LobbyChatInputPanel";
        public const string StyleClassLobbyChatLineEdit = "LobbyChatLineEdit";
        public const string StyleClassLobbyChatSelectorButton = "LobbyChatSelectorButton";
        public const string StyleClassLobbyChatFilterButton = "LobbyChatFilterButton";
        public const string StyleClassLobbyEmblem = "LobbyEmblem";

        public const string StyleClassConsoleHeading = "ConsoleHeading";
        public const string StyleClassConsoleSubHeading = "ConsoleSubHeading";
        public const string StyleClassConsoleText = "ConsoleText";

        public const string StyleClassSliderRed = "Red";
        public const string StyleClassSliderGreen = "Green";
        public const string StyleClassSliderBlue = "Blue";
        public const string StyleClassSliderWhite = "White";

        public const string StyleClassLabelHeadingBigger = "LabelHeadingBigger";
        public const string StyleClassLabelKeyText = "LabelKeyText";
        public const string StyleClassLabelSecondaryColor = "LabelSecondaryColor";
        public const string StyleClassLabelBig = "LabelBig";
        public const string StyleClassLabelSmall = "LabelSmall";
        public const string StyleClassCMProfileFont = "CMProfileFont";
        public const string StyleClassButtonBig = "ButtonBig";

        public const string StyleClassButtonHelp = "HelpButton";

        public const string StyleClassPopupMessageSmall = "PopupMessageSmall";
        public const string StyleClassPopupMessageSmallCaution = "PopupMessageSmallCaution";
        public const string StyleClassPopupMessageMedium = "PopupMessageMedium";
        public const string StyleClassPopupMessageMediumCaution = "PopupMessageMediumCaution";
        public const string StyleClassPopupMessageLarge = "PopupMessageLarge";
        public const string StyleClassPopupMessageLargeCaution = "PopupMessageLargeCaution";

        public static Color PanelDark = Color.FromHex("#001304");

        public static Color NanoGold = Color.FromHex("#6CFF6C");
        public static Color GoodGreenFore = Color.FromHex("#3AFF6A");
        public static Color ConcerningOrangeFore = Color.FromHex("#78FF6A");
        public static Color DangerousRedFore = Color.FromHex("#48FF6A");
        public static Color DisabledFore = Color.FromHex("#284b32");

        public static Color ButtonColorDefault = Color.FromHex("#127E2B");
        public static Color ButtonColorDefaultRed = Color.FromHex("#127E2B");
        public static Color ButtonColorHovered = Color.FromHex("#24903A");
        public static Color ButtonColorHoveredRed = Color.FromHex("#24903A");
        public static Color ButtonColorPressed = Color.FromHex("#1A5E27");
        public static Color ButtonColorDisabled = Color.FromHex("#1C682F"); // CCM 10 > 15: lobby rework

        public static Color ButtonColorCautionDefault = Color.FromHex("#1F7A31");
        public static Color ButtonColorCautionHovered = Color.FromHex("#24903A");
        public static Color ButtonColorCautionPressed = Color.FromHex("#1A5E27");
        public static Color ButtonColorCautionDisabled = Color.FromHex("#1f5e2e");

        public static Color ButtonColorGoodDefault = Color.FromHex("#1F7A31");
        public static Color ButtonColorGoodHovered = Color.FromHex("#24903A");
        public static Color ButtonColorGoodDisabled = Color.FromHex("#164722");

        public static Color LobbyCrtAccent = Color.FromHex("#2EF241");
        public static Color LobbyCrtText = Color.FromHex("#0AE14A");
        public static Color LobbyCrtMutedText = Color.FromHex("#A0C7B1");
        public static Color LobbyCleanAccent = Color.FromHex("#2F6B46");
        public static Color LobbyCleanText = Color.FromHex("#D5FFE0");
        public static Color LobbyCleanMutedText = Color.FromHex("#9BC9A8");
        public static Color LobbyCrtGlow = Color.FromHex("#3AFF7899");
        public static Color LobbyMenuButtonBase = Color.FromHex("#26F239");
        public static Color LobbyMenuButtonPressed = Color.FromHex("#00BD1A");
        public static Color LobbyMenuButtonReadyPressed = Color.FromHex("#00BD1A");
        public static Color LobbyMenuButtonDisabledCrt = Color.FromHex("#00611F");
        public static Color LobbyMenuButtonDisabledClean = Color.FromHex("#009430");

        //NavMap
        public static Color PointRed = Color.FromHex("#2F6A3B");
        public static Color PointGreen = Color.FromHex("#38b026");
        public static Color PointMagenta = Color.FromHex("#00EB4C");

        // Context menu button colors
        public static Color ButtonColorContext = Color.FromHex("#0B2E15");
        public static Color ButtonColorContextHover = Color.FromHex("#0F381B");
        public static Color ButtonColorContextPressed = Color.FromHex("#082310");
        public static Color ButtonColorContextDisabled = Color.FromHex("#082310");

        // Examine button colors
        public static Color ExamineButtonColorContext = Color.FromHex("#0B2E15");
        public static Color ExamineButtonColorContextHover = Color.FromHex("#0F381B");
        public static Color ExamineButtonColorContextPressed = Color.FromHex("#082310");
        public static Color ExamineButtonColorContextDisabled = Color.FromHex("#082310");

        // Fancy Tree elements
        public static Color FancyTreeEvenRowColor = Color.FromHex("#0E1A11");
        public static Color FancyTreeOddRowColor = FancyTreeEvenRowColor * new Color(0.85f, 0.85f, 0.85f);
        public static Color FancyTreeSelectedRowColor = Color.FromHex("#13261A");

        //Used by the APC and SMES menus
        public const string StyleClassPowerStateNone = "PowerStateNone";
        public const string StyleClassPowerStateLow = "PowerStateLow";
        public const string StyleClassPowerStateGood = "PowerStateGood";

        public const string StyleClassItemStatus = "ItemStatus";
        public const string StyleClassItemStatusNotHeld = "ItemStatusNotHeld";
        public static Color ItemStatusNotHeldColor = Color.FromHex("#2D4A35");

        //Background
        public const string StyleClassBackgroundBaseDark = "PanelBackgroundBaseDark";

        //Buttons
        public const string StyleClassCrossButtonRed = "CrossButtonRed";
        public const string StyleClassButtonColorRed = "ButtonColorRed";
        public const string StyleClassButtonColorGreen = "ButtonColorGreen";

        public static Color ChatBackgroundColor = Color.FromHex("#0A120B");

        //Bwoink
        public const string StyleClassPinButtonPinned = "pinButtonPinned";
        public const string StyleClassPinButtonUnpinned = "pinButtonUnpinned";

        private static UiColorTheme ParseTheme(string theme)
        {
            return theme.Equals("blue", StringComparison.OrdinalIgnoreCase)
                ? UiColorTheme.Blue
                : UiColorTheme.Green;
        }

        private static void ApplyPalette(UiColorTheme theme)
        {
            CurrentTheme = theme;
            if (theme == UiColorTheme.Blue)
            {
                PanelDark = Color.FromHex("#072F6A");
                NanoGold = Color.FromHex("#6CB4FF");
                GoodGreenFore = Color.FromHex("#3A8CFF");
                ConcerningOrangeFore = Color.FromHex("#78B6FF");
                DangerousRedFore = Color.FromHex("#489CFF");
                DisabledFore = Color.FromHex("#1E3348");

                ButtonColorDefault = Color.FromHex("#18408A");
                ButtonColorDefaultRed = Color.FromHex("#18408A");
                ButtonColorHovered = Color.FromHex("#2A5FA0");
                ButtonColorHoveredRed = Color.FromHex("#2A5FA0");
                ButtonColorPressed = Color.FromHex("#1B4063");
                ButtonColorDisabled = Color.FromHex("#123044");

                ButtonColorCautionDefault = Color.FromHex("#1F4D7A");
                ButtonColorCautionHovered = Color.FromHex("#2A5FA0");
                ButtonColorCautionPressed = Color.FromHex("#1B4063");
                ButtonColorCautionDisabled = Color.FromHex("#123044");

                ButtonColorGoodDefault = Color.FromHex("#1F4D7A");
                ButtonColorGoodHovered = Color.FromHex("#2A5FA0");
                ButtonColorGoodDisabled = Color.FromHex("#123044");

                LobbyCrtAccent = Color.FromHex("#2E6BFF");
                LobbyCrtText = Color.FromHex("#0A70E1");
                LobbyCrtMutedText = Color.FromHex("#A0B7C7");
                LobbyCleanAccent = Color.FromHex("#2F4E6B");
                LobbyCleanText = Color.FromHex("#D5E8FF");
                LobbyCleanMutedText = Color.FromHex("#9BB3C9");
                LobbyCrtGlow = Color.FromHex("#3A7DFF99");
                LobbyMenuButtonBase = Color.FromHex("#0872F0");
                LobbyMenuButtonPressed = Color.FromHex("#074CC4");
                LobbyMenuButtonReadyPressed = Color.FromHex("#074CC4");
                LobbyMenuButtonDisabledCrt = Color.FromHex("#053A8F");
                LobbyMenuButtonDisabledClean = Color.FromHex("#053A8F");

                PointRed = Color.FromHex("#2F4B6A");
                PointGreen = Color.FromHex("#2F7ED6");
                PointMagenta = Color.FromHex("#00B0EB");

                ButtonColorContext = Color.FromHex("#08243A");
                ButtonColorContextHover = Color.FromHex("#0B2C47");
                ButtonColorContextPressed = Color.FromHex("#061B2F");
                ButtonColorContextDisabled = Color.FromHex("#061B2F");

                ExamineButtonColorContext = Color.FromHex("#08243A");
                ExamineButtonColorContextHover = Color.FromHex("#0B2C47");
                ExamineButtonColorContextPressed = Color.FromHex("#061B2F");
                ExamineButtonColorContextDisabled = Color.FromHex("#061B2F");

                FancyTreeEvenRowColor = Color.FromHex("#0E141A");
                FancyTreeSelectedRowColor = Color.FromHex("#132033");
                ItemStatusNotHeldColor = Color.FromHex("#2D3A4A");
                ChatBackgroundColor = Color.FromHex("#0A0F14");
            }
            else
            {
                PanelDark = Color.FromHex("#001304");
                NanoGold = Color.FromHex("#6CFF6C");
                GoodGreenFore = Color.FromHex("#3AFF6A");
                ConcerningOrangeFore = Color.FromHex("#78FF6A");
                DangerousRedFore = Color.FromHex("#48FF6A");
                DisabledFore = Color.FromHex("#23402B");

                ButtonColorDefault = Color.FromHex("#127E2B");
                ButtonColorDefaultRed = Color.FromHex("#127E2B");
                ButtonColorHovered = Color.FromHex("#24903A");
                ButtonColorHoveredRed = Color.FromHex("#24903A");
                ButtonColorPressed = Color.FromHex("#1A5E27");
                ButtonColorDisabled = Color.FromHex("#123A1C");

                ButtonColorCautionDefault = Color.FromHex("#1F7A31");
                ButtonColorCautionHovered = Color.FromHex("#24903A");
                ButtonColorCautionPressed = Color.FromHex("#1A5E27");
                ButtonColorCautionDisabled = Color.FromHex("#123A1C");

                ButtonColorGoodDefault = Color.FromHex("#1F7A31");
                ButtonColorGoodHovered = Color.FromHex("#24903A");
                ButtonColorGoodDisabled = Color.FromHex("#123A1C");

                LobbyCrtAccent = Color.FromHex("#2EF241");
                LobbyCrtText = Color.FromHex("#0AE14A");
                LobbyCrtMutedText = Color.FromHex("#A0C7B1");
                LobbyCleanAccent = Color.FromHex("#2F6B46");
                LobbyCleanText = Color.FromHex("#D5FFE0");
                LobbyCleanMutedText = Color.FromHex("#9BC9A8");
                LobbyCrtGlow = Color.FromHex("#3AFF7899");
                LobbyMenuButtonBase = Color.FromHex("#26F239");
                LobbyMenuButtonPressed = Color.FromHex("#00BD1A");
                LobbyMenuButtonReadyPressed = Color.FromHex("#00BD1A");
                LobbyMenuButtonDisabledCrt = Color.FromHex("#00611F");
                LobbyMenuButtonDisabledClean = Color.FromHex("#009430");

                PointRed = Color.FromHex("#2F6A3B");
                PointGreen = Color.FromHex("#38b026");
                PointMagenta = Color.FromHex("#00EB4C");

                ButtonColorContext = Color.FromHex("#0B2E15");
                ButtonColorContextHover = Color.FromHex("#0F381B");
                ButtonColorContextPressed = Color.FromHex("#082310");
                ButtonColorContextDisabled = Color.FromHex("#082310");

                ExamineButtonColorContext = Color.FromHex("#0B2E15");
                ExamineButtonColorContextHover = Color.FromHex("#0F381B");
                ExamineButtonColorContextPressed = Color.FromHex("#082310");
                ExamineButtonColorContextDisabled = Color.FromHex("#082310");

                FancyTreeEvenRowColor = Color.FromHex("#0E1A11");
                FancyTreeSelectedRowColor = Color.FromHex("#13261A");
                ItemStatusNotHeldColor = Color.FromHex("#2D4A35");
                ChatBackgroundColor = Color.FromHex("#0A120B");
            }

            FancyTreeOddRowColor = FancyTreeEvenRowColor * new Color(0.85f, 0.85f, 0.85f);
        }


        public override Stylesheet Stylesheet { get; }

        public StyleNano(IResourceCache resCache, string theme) : base(resCache)
        {
            ApplyPalette(ParseTheme(theme));
            var notoSans8 = resCache.NotoStack(size: 8);
            var notoSans10 = resCache.NotoStack(size: 10);
            var notoSansItalic10 = resCache.NotoStack(variation: "Italic", size: 10);
            var notoSans12 = resCache.NotoStack(size: 12);
            var notoSansItalic12 = resCache.NotoStack(variation: "Italic", size: 12);
            var notoSansBold12 = resCache.NotoStack(variation: "Bold", size: 12);
            var notoSansBold14 = resCache.NotoStack(variation: "Bold", size: 14);
            var notoSansBoldItalic12 = resCache.NotoStack(variation: "BoldItalic", size: 12);
            var notoSansBoldItalic14 = resCache.NotoStack(variation: "BoldItalic", size: 14);
            var notoSansBoldItalic16 = resCache.NotoStack(variation: "BoldItalic", size: 16);
            var notoSansDisplayBold14 = resCache.NotoStack(variation: "Bold", display: true, size: 14);
            var notoSansDisplayBold16 = resCache.NotoStack(variation: "Bold", display: true, size: 16);
            var notoSans15 = resCache.NotoStack(variation: "Regular", size: 15);
            var notoSans16 = resCache.NotoStack(variation: "Regular", size: 16);
            var notoSansBold16 = resCache.NotoStack(variation: "Bold", size: 16);
            var notoSansBold18 = resCache.NotoStack(variation: "Bold", size: 18);
            var notoSansBold20 = resCache.NotoStack(variation: "Bold", size: 20);
            var exo2Regular12 = resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 12);
            var bedstead12 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 12);
            var bedstead14 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 14);
            var bedstead15 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 15);
            var bedstead16 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 16);
            var bedstead20 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 20);
            var notoSansMono = resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", size: 12);
            var robotoMonoBold11 = resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", size: 11);
            var robotoMonoBold12 = resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", size: 12);
            var robotoMonoBold14 = resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", size: 14);

            var windowHeader = new StyleBoxFlat
            {
                BackgroundColor = (CurrentTheme == UiColorTheme.Blue
                    ? Color.FromHex("#0F2A55")
                    : Color.FromHex("#0E2A16")).WithAlpha(0.4f),
                ContentMarginBottomOverride = 0
            };
            var windowHeaderAlert = new StyleBoxFlat
            {
                BackgroundColor = (CurrentTheme == UiColorTheme.Blue
                    ? Color.FromHex("#0F2A55")
                    : Color.FromHex("#0E2A16")).WithAlpha(0.4f),
                ContentMarginBottomOverride = 0
            };
            var uiWindowBackgroundTint = PanelDark.WithAlpha(0.99f);
            var windowBackground = new StyleBoxFlat
            {
                BackgroundColor = uiWindowBackgroundTint,
            };

            var optionsWindowBackground = new StyleBoxTexture
            {
                Texture = resCache.GetTexture("/Textures/_CCM14/Lobby/rightside_chat_bg.png"),
                Modulate = Color.White.WithAlpha(1f),
            };
            optionsWindowBackground.SetPatchMargin(StyleBox.Margin.All, 2);

            var borderedWindowBackgroundTex = resCache.GetTexture("/Textures/Interface/Nano/window_background_bordered.png");
            var borderedWindowBackground = new StyleBoxFlat
            {
                BackgroundColor = uiWindowBackgroundTint,
                BorderThickness = new Thickness(1),
                BorderColor = uiWindowBackgroundTint.WithAlpha(1f),
            };

            var contextMenuBackground = new StyleBoxFlat
            {
                BackgroundColor = uiWindowBackgroundTint,
                BorderThickness = new Thickness(1),
                BorderColor = uiWindowBackgroundTint.WithAlpha(1f),
            };

            var invSlotBgTex = resCache.GetTexture("/Textures/Interface/Inventory/inv_slot_background.png");
            var invSlotBg = new StyleBoxTexture
            {
                Texture = invSlotBgTex,
            };
            invSlotBg.SetPatchMargin(StyleBox.Margin.All, 2);
            invSlotBg.SetContentMarginOverride(StyleBox.Margin.All, 0);

            var handSlotHighlightTex = resCache.GetTexture("/Textures/Interface/Inventory/hand_slot_highlight.png");
            var handSlotHighlight = new StyleBoxTexture
            {
                Texture = handSlotHighlightTex,
            };
            handSlotHighlight.SetPatchMargin(StyleBox.Margin.All, 2);

            var borderedTransparentWindowBackgroundTex = resCache.GetTexture("/Textures/Interface/Nano/transparent_window_background_bordered.png");
            var borderedTransparentWindowBackground = new StyleBoxFlat
            {
                BackgroundColor = uiWindowBackgroundTint.WithAlpha(0.85f),
                BorderThickness = new Thickness(1),
                BorderColor = uiWindowBackgroundTint.WithAlpha(1f),
            };

            var hotbarBackground = new StyleBoxFlat
            {
                BackgroundColor = uiWindowBackgroundTint,
                BorderThickness = new Thickness(1),
                BorderColor = uiWindowBackgroundTint.WithAlpha(1f),
            };

            var buttonStorage = new StyleBoxTexture(BaseButton);
            buttonStorage.SetPatchMargin(StyleBox.Margin.All, 10);
            buttonStorage.SetPadding(StyleBox.Margin.All, 0);
            buttonStorage.SetContentMarginOverride(StyleBox.Margin.Vertical, 0);
            buttonStorage.SetContentMarginOverride(StyleBox.Margin.Horizontal, 4);

            var buttonContext = new StyleBoxTexture { Texture = Texture.White };
            buttonContext.SetPadding(StyleBox.Margin.All, 0);
            buttonContext.SetContentMarginOverride(StyleBox.Margin.All, 2);

            var buttonRectTex = resCache.GetTexture("/Textures/Interface/Nano/light_panel_background_bordered.png");
            var buttonRect = new StyleBoxTexture(BaseButton)
            {
                Texture = buttonRectTex
            };
            buttonRect.SetPatchMargin(StyleBox.Margin.All, 2);
            buttonRect.SetPadding(StyleBox.Margin.All, 2);
            buttonRect.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
            buttonRect.SetContentMarginOverride(StyleBox.Margin.Horizontal, 2);

            var buttonRectHover = new StyleBoxTexture(buttonRect)
            {
                Modulate = ButtonColorHovered
            };

            var buttonRectPressed = new StyleBoxTexture(buttonRect)
            {
                Modulate = ButtonColorPressed
            };

            var buttonRectDisabled = new StyleBoxTexture(buttonRect)
            {
                Modulate = ButtonColorDisabled
            };

            var buttonRectActionMenuItemTex = resCache.GetTexture("/Textures/Interface/Nano/black_panel_light_thin_border.png");
            var buttonRectActionMenuRevokedItemTex = resCache.GetTexture("/Textures/Interface/Nano/black_panel_red_thin_border.png");
            var buttonRectActionMenuItem = new StyleBoxTexture(BaseButton)
            {
                Texture = buttonRectActionMenuItemTex
            };
            buttonRectActionMenuItem.SetPatchMargin(StyleBox.Margin.All, 2);
            buttonRectActionMenuItem.SetPadding(StyleBox.Margin.All, 2);
            buttonRectActionMenuItem.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
            buttonRectActionMenuItem.SetContentMarginOverride(StyleBox.Margin.Horizontal, 2);
            var buttonRectActionMenuItemRevoked = new StyleBoxTexture(buttonRectActionMenuItem)
            {
                Texture = buttonRectActionMenuRevokedItemTex
            };
            var buttonRectActionMenuItemHover = new StyleBoxTexture(buttonRectActionMenuItem)
            {
                Modulate = ButtonColorHovered
            };
            var buttonRectActionMenuItemPressed = new StyleBoxTexture(buttonRectActionMenuItem)
            {
                Modulate = ButtonColorPressed
            };

            var buttonTex = resCache.GetTexture("/Textures/Interface/Nano/button.svg.96dpi.png");
            var topButtonBase = new StyleBoxTexture
            {
                Texture = buttonTex,
            };
            topButtonBase.SetPatchMargin(StyleBox.Margin.All, 10);
            topButtonBase.SetPadding(StyleBox.Margin.All, 0);
            topButtonBase.SetContentMarginOverride(StyleBox.Margin.All, 0);

            var topButtonOpenRight = new StyleBoxTexture(topButtonBase)
            {
                Texture = buttonTex,
            };

            var topButtonOpenLeft = new StyleBoxTexture(topButtonBase)
            {
                Texture = buttonTex,
            };

            var topButtonSquare = new StyleBoxTexture(topButtonBase)
            {
                Texture = buttonTex,
            };

            var chatChannelButtonTex = resCache.GetTexture("/Textures/Interface/Nano/rounded_button.svg.96dpi.png");
            var chatChannelButton = new StyleBoxTexture
            {
                Texture = chatChannelButtonTex,
            };
            chatChannelButton.SetPatchMargin(StyleBox.Margin.All, 5);
            chatChannelButton.SetPadding(StyleBox.Margin.All, 1);

            var chatFilterButtonTex = resCache.GetTexture("/Textures/Interface/Nano/rounded_button_bordered.svg.96dpi.png");
            var chatFilterButton = new StyleBoxTexture
            {
                Texture = chatFilterButtonTex,
            };
            chatFilterButton.SetPatchMargin(StyleBox.Margin.All, 5);
            chatFilterButton.SetPadding(StyleBox.Margin.All, 2);

            var outputPanelScrollDownButtonTex = resCache.GetTexture("/Textures/Interface/Nano/rounded_button_half_bordered.svg.96dpi.png");
            var outputPanelScrollDownButton = new StyleBoxTexture
            {
                Texture = outputPanelScrollDownButtonTex,
            };
            outputPanelScrollDownButton.SetPatchMargin(StyleBox.Margin.All, 5);
            outputPanelScrollDownButton.SetPadding(StyleBox.Margin.All, 2);
            outputPanelScrollDownButton.SetPadding(StyleBox.Margin.Top, 0);
            outputPanelScrollDownButton.SetPadding(StyleBox.Margin.Bottom, 0);

            var smallButtonTex = resCache.GetTexture("/Textures/Interface/Nano/button_small.svg.96dpi.png");
            var smallButtonBase = new StyleBoxTexture
            {
                Texture = smallButtonTex,
            };

            var textureInvertedTriangle = resCache.GetTexture("/Textures/Interface/Nano/inverted_triangle.svg.png");

            var lineEditTex = resCache.GetTexture("/Textures/Interface/Nano/lineedit.png");
            var lineEdit = new StyleBoxTexture
            {
                Texture = lineEditTex,
            };
            lineEdit.SetPatchMargin(StyleBox.Margin.All, 3);
            lineEdit.SetContentMarginOverride(StyleBox.Margin.Horizontal, 5);

            var chatBg = new StyleBoxFlat
            {
                BackgroundColor = ChatBackgroundColor
            };

            var chatSubBg = new StyleBoxFlat
            {
                BackgroundColor = ChatBackgroundColor,
            };
            chatSubBg.SetContentMarginOverride(StyleBox.Margin.All, 2);

            var lobbyPanelCrt = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0B100E").WithAlpha(0.92f),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };
            lobbyPanelCrt.SetContentMarginOverride(StyleBox.Margin.All, 6);

            var lobbyPanelGlowCrt = new StyleBoxFlat
            {
                BackgroundColor = Color.Transparent,
                BorderColor = LobbyCrtAccent.WithAlpha(0.5f),
                BorderThickness = new Thickness(2)
            };
            lobbyPanelGlowCrt.SetContentMarginOverride(StyleBox.Margin.All, 0);

            var lobbyPanelClean = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0D1712").WithAlpha(0.95f),
                BorderColor = LobbyCleanAccent,
                BorderThickness = new Thickness(1)
            };
            lobbyPanelClean.SetContentMarginOverride(StyleBox.Margin.All, 6);

            var lobbyPanelGlowClean = new StyleBoxFlat
            {
                BackgroundColor = Color.Transparent,
                BorderColor = LobbyCleanAccent.WithAlpha(0.4f),
                BorderThickness = new Thickness(2)
            };
            lobbyPanelGlowClean.SetContentMarginOverride(StyleBox.Margin.All, 0);

            var lobbyInfoPanelCrt = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0A0E0C").WithAlpha(0.8f),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };
            lobbyInfoPanelCrt.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyInfoPanelClean = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0D1611").WithAlpha(0.9f),
                BorderColor = LobbyCleanAccent,
                BorderThickness = new Thickness(1)
            };
            lobbyInfoPanelClean.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyMusicPanelCrt = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0A0E0C").WithAlpha(0.9f),
                BorderColor = LobbyCrtAccent.WithAlpha(0.6f),
                BorderThickness = new Thickness(1)
            };
            lobbyMusicPanelCrt.SetContentMarginOverride(StyleBox.Margin.All, 2);

            var lobbyMusicPanelClean = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0D1611").WithAlpha(0.95f),
                BorderColor = LobbyCleanAccent.WithAlpha(0.6f),
                BorderThickness = new Thickness(1)
            };
            lobbyMusicPanelClean.SetContentMarginOverride(StyleBox.Margin.All, 2);

            var lobbyInfoDividerCrt = new StyleBoxFlat
            {
                BackgroundColor = LobbyCrtAccent
            };

            var lobbyInfoDividerClean = new StyleBoxFlat
            {
                BackgroundColor = LobbyCleanAccent
            };

            var lobbyMenuDividerCrt = new StyleBoxFlat
            {
                BackgroundColor = LobbyCrtAccent.WithAlpha(0.9f),
                BorderColor = LobbyCrtAccent.WithAlpha(0.6f),
                BorderThickness = new Thickness(1)
            };

            var lobbyMenuDividerClean = new StyleBoxFlat
            {
                BackgroundColor = LobbyCleanAccent.WithAlpha(0.9f),
                BorderColor = LobbyCleanAccent.WithAlpha(0.6f),
                BorderThickness = new Thickness(1)
            };

            var lobbyButtonCrt = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0A120D"),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyButtonCrtHover = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#122319"),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyButtonCrtPressed = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#15301F"),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyButtonClean = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#14241A"),
                BorderColor = LobbyCleanAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyButtonCleanHover = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#1A3324"),
                BorderColor = LobbyCleanAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyButtonCleanPressed = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#1C3A28"),
                BorderColor = LobbyCleanAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyMenuButtonCrt = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonBase,
                BorderColor = LobbyMenuButtonBase,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCrt.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyMenuButtonCrtHover = new StyleBoxFlat
            {
                BackgroundColor = LobbyCrtAccent.WithAlpha(0f),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCrtHover.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyMenuButtonCrtPressed = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonPressed,
                BorderColor = LobbyMenuButtonPressed,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCrtPressed.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyMenuButtonCrtReadyPressed = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonReadyPressed,
                BorderColor = LobbyMenuButtonReadyPressed,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCrtReadyPressed.SetContentMarginOverride(StyleBox.Margin.All, 4);



            var lobbyMenuButtonCrtDisabled = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonDisabledCrt,
                BorderColor = LobbyMenuButtonDisabledCrt,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCrtDisabled.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyMenuButtonClean = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonBase,
                BorderColor = LobbyMenuButtonBase,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonClean.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyMenuButtonCleanHover = new StyleBoxFlat
            {
                BackgroundColor = LobbyCrtAccent.WithAlpha(0f),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCleanHover.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyMenuButtonCleanPressed = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonPressed,
                BorderColor = LobbyMenuButtonPressed,
                BorderThickness = new Thickness(2)
            };
            lobbyMenuButtonCleanPressed.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyMenuButtonCleanReadyPressed = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonReadyPressed,
                BorderColor = LobbyMenuButtonReadyPressed,
                BorderThickness = new Thickness(2)
            };
            lobbyMenuButtonCleanReadyPressed.SetContentMarginOverride(StyleBox.Margin.All, 4);



            var lobbyMenuButtonCleanDisabled = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonDisabledClean,
                BorderColor = LobbyMenuButtonDisabledClean,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCleanDisabled.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyChatPanelCrt = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0C1011").WithAlpha(0.68f),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyChatPanelClean = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0D1813").WithAlpha(0.72f),
                BorderColor = LobbyCleanAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyChatInputCrt = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#090D0B"),
                BorderColor = Color.Transparent,
                BorderThickness = new Thickness(0)
            };

            var lobbyChatInputClean = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0A130E"),
                BorderColor = Color.Transparent,
                BorderThickness = new Thickness(0)
            };

            var actionSearchBoxTex = resCache.GetTexture("/Textures/Interface/Nano/black_panel_dark_thin_border.png");
            var actionSearchBox = new StyleBoxTexture
            {
                Texture = actionSearchBoxTex,
            };
            actionSearchBox.SetPatchMargin(StyleBox.Margin.All, 3);
            actionSearchBox.SetContentMarginOverride(StyleBox.Margin.Horizontal, 5);

            var tabContainerPanel = new StyleBoxFlat
            {
                BackgroundColor = PanelDark.WithAlpha(0.95f),
            };

            var tabContainerBoxActive = new StyleBoxFlat { BackgroundColor = PanelDark.WithAlpha(0.98f) };
            tabContainerBoxActive.SetContentMarginOverride(StyleBox.Margin.Horizontal, 8);
            tabContainerBoxActive.SetContentMarginOverride(StyleBox.Margin.Vertical, 3);
            var tabContainerBoxInactive = new StyleBoxFlat { BackgroundColor = PanelDark.WithAlpha(0.9f) };
            tabContainerBoxInactive.SetContentMarginOverride(StyleBox.Margin.Horizontal, 8);
            tabContainerBoxInactive.SetContentMarginOverride(StyleBox.Margin.Vertical, 3);

            var progressBarBackground = new StyleBoxFlat
            {
                BackgroundColor = new Color(0.25f, 0.25f, 0.25f)
            };
            progressBarBackground.SetContentMarginOverride(StyleBox.Margin.Vertical, 14.5f);

            var progressBarForeground = new StyleBoxFlat
            {
                BackgroundColor = new Color(0.25f, 0.50f, 0.25f)
            };
            progressBarForeground.SetContentMarginOverride(StyleBox.Margin.Vertical, 14.5f);

            // Monotone (unfilled)
            var monotoneButton = new StyleBoxTexture
            {
                Texture = resCache.GetTexture("/Textures/Interface/Nano/Monotone/monotone_button.svg.96dpi.png"),
            };
            monotoneButton.SetPatchMargin(StyleBox.Margin.All, 11);
            monotoneButton.SetPadding(StyleBox.Margin.All, 1);
            monotoneButton.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
            monotoneButton.SetContentMarginOverride(StyleBox.Margin.Horizontal, 14);

            var monotoneButtonOpenLeft = new StyleBoxTexture(monotoneButton)
            {
                Texture = monotoneButton.Texture,
            };

            var monotoneButtonOpenRight = new StyleBoxTexture(monotoneButton)
            {
                Texture = monotoneButton.Texture,
            };

            var monotoneButtonOpenBoth = new StyleBoxTexture(monotoneButton)
            {
                Texture = monotoneButton.Texture,
            };

            // Monotone (filled)
            var monotoneFilledButton = new StyleBoxTexture(monotoneButton)
            {
                Texture = buttonTex,
            };

            var monotoneFilledButtonOpenLeft = new StyleBoxTexture(monotoneButton)
            {
                Texture = buttonTex,
            };

            var monotoneFilledButtonOpenRight = new StyleBoxTexture(monotoneButton)
            {
                Texture = buttonTex,
            };

            var monotoneFilledButtonOpenBoth = new StyleBoxTexture(monotoneButton)
            {
                Texture = buttonTex,
            };
            var optionButtonOpenBoth = new StyleBoxTexture
            {
                Texture = buttonTex,
            };
            optionButtonOpenBoth.SetPatchMargin(StyleBox.Margin.All, 10);
            optionButtonOpenBoth.SetPadding(StyleBox.Margin.All, 1);
            optionButtonOpenBoth.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
            optionButtonOpenBoth.SetContentMarginOverride(StyleBox.Margin.Horizontal, 14);

            // CheckBox
            var checkBoxTextureChecked = resCache.GetTexture("/Textures/Interface/Nano/checkbox_checked.svg.96dpi.png");
            var checkBoxTextureUnchecked = resCache.GetTexture("/Textures/Interface/Nano/checkbox_unchecked.svg.96dpi.png");
            var monotoneCheckBoxTextureChecked = resCache.GetTexture("/Textures/Interface/Nano/Monotone/monotone_checkbox_checked.svg.96dpi.png");
            var monotoneCheckBoxTextureUnchecked = resCache.GetTexture("/Textures/Interface/Nano/Monotone/monotone_checkbox_unchecked.svg.96dpi.png");

            // Tooltip box
            var tooltipTexture = resCache.GetTexture("/Textures/Interface/Nano/tooltip.png");
            var tooltipBox = new StyleBoxTexture
            {
                Texture = tooltipTexture,
            };
            tooltipBox.SetPatchMargin(StyleBox.Margin.All, 2);
            tooltipBox.SetContentMarginOverride(StyleBox.Margin.Horizontal, 7);

            // Whisper box
            var whisperTexture = resCache.GetTexture("/Textures/Interface/Nano/whisper.png");
            var whisperBox = new StyleBoxTexture
            {
                Texture = whisperTexture,
            };
            whisperBox.SetPatchMargin(StyleBox.Margin.All, 2);
            whisperBox.SetContentMarginOverride(StyleBox.Margin.Horizontal, 7);

            // Placeholder
            var placeholderTexture = resCache.GetTexture("/Textures/Interface/Nano/placeholder.png");
            var placeholder = new StyleBoxTexture { Texture = placeholderTexture };
            placeholder.SetPatchMargin(StyleBox.Margin.All, 19);
            placeholder.SetExpandMargin(StyleBox.Margin.All, -5);
            placeholder.Mode = StyleBoxTexture.StretchMode.Tile;

            var itemListBackgroundSelected = new StyleBoxFlat { BackgroundColor = new Color(75, 75, 86) };
            itemListBackgroundSelected.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
            itemListBackgroundSelected.SetContentMarginOverride(StyleBox.Margin.Horizontal, 4);
            var itemListItemBackgroundDisabled = new StyleBoxFlat { BackgroundColor = new Color(10, 10, 12) };
            itemListItemBackgroundDisabled.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
            itemListItemBackgroundDisabled.SetContentMarginOverride(StyleBox.Margin.Horizontal, 4);
            var itemListItemBackground = new StyleBoxFlat { BackgroundColor = new Color(55, 55, 68) };
            itemListItemBackground.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
            itemListItemBackground.SetContentMarginOverride(StyleBox.Margin.Horizontal, 4);
            var itemListItemBackgroundTransparent = new StyleBoxFlat { BackgroundColor = Color.Transparent };
            itemListItemBackgroundTransparent.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
            itemListItemBackgroundTransparent.SetContentMarginOverride(StyleBox.Margin.Horizontal, 4);

            var squareTex = resCache.GetTexture("/Textures/Interface/Nano/square.png");
            var listContainerButton = new StyleBoxTexture
            {
                Texture = squareTex,
                ContentMarginLeftOverride = 10
            };

            // NanoHeading
            var nanoHeadingTex = resCache.GetTexture("/Textures/Interface/Nano/nanoheading.svg.96dpi.png");
            var nanoHeadingBox = new StyleBoxTexture
            {
                Texture = nanoHeadingTex,
                PatchMarginRight = 10,
                PatchMarginTop = 10,
                ContentMarginTopOverride = 2,
                ContentMarginLeftOverride = 10,
                PaddingTop = 4
            };

            nanoHeadingBox.SetPatchMargin(StyleBox.Margin.Left | StyleBox.Margin.Bottom, 2);

            // Stripe background
            var stripeBackTex = resCache.GetTexture("/Textures/Interface/Nano/stripeback.svg.96dpi.png");
            var stripeBack = new StyleBoxTexture
            {
                Texture = stripeBackTex,
                Mode = StyleBoxTexture.StretchMode.Tile,
                Modulate = (CurrentTheme == UiColorTheme.Blue
                    ? Color.FromHex("#0B1E3A")
                    : Color.FromHex("#06130B")).WithAlpha(0.6f)
            };
            // CCM rework lobby - start
            var scrollBarNormal = new StyleBoxFlat
            {
                BackgroundColor = ButtonColorDefault.WithAlpha(0.55f),
                ContentMarginLeftOverride = 10,
                ContentMarginTopOverride = 10
            };
            var scrollBarHovered = new StyleBoxFlat
            {
                BackgroundColor = CurrentTheme == UiColorTheme.Blue
                    ? Color.FromHex("#123A78").WithAlpha(0.9f)
                    : Color.FromHex("#1A5A2B").WithAlpha(0.9f),
                ContentMarginLeftOverride = 10,
                ContentMarginTopOverride = 10
            };
            var scrollBarGrabbed = new StyleBoxFlat
            {
                BackgroundColor = ButtonColorPressed.WithAlpha(0.8f),
                ContentMarginLeftOverride = 10,
                ContentMarginTopOverride = 10
            };
            // CCM rework lobby - end

            // Slider
            var sliderOutlineTex = resCache.GetTexture("/Textures/Interface/Nano/slider_outline.svg.96dpi.png");
            var sliderFillTex = resCache.GetTexture("/Textures/Interface/Nano/slider_fill.svg.96dpi.png");
            var sliderGrabTex = resCache.GetTexture("/Textures/Interface/Nano/slider_grabber.svg.96dpi.png");

            var sliderFillBox = new StyleBoxTexture
            {
                Texture = sliderFillTex,
                Modulate = LobbyMenuButtonBase * new Color(0.85f, 0.85f, 0.85f, 1f)
            };

            var sliderBackBox = new StyleBoxTexture
            {
                Texture = sliderFillTex,
                Modulate = CurrentTheme == UiColorTheme.Blue ? Color.FromHex("#0B3B7A") : PanelDark,
            };

            var sliderForeBox = new StyleBoxTexture
            {
                Texture = sliderOutlineTex,
                Modulate = CurrentTheme == UiColorTheme.Blue ? Color.FromHex("#082B55") : Color.FromHex("#1E3A28")
            };

            var sliderGrabBox = new StyleBoxTexture
            {
                Texture = sliderGrabTex,
                Modulate = LobbyMenuButtonBase * new Color(0.85f, 0.85f, 0.85f, 1f)
            };

            sliderFillBox.SetPatchMargin(StyleBox.Margin.All, 12);
            sliderBackBox.SetPatchMargin(StyleBox.Margin.All, 12);
            sliderForeBox.SetPatchMargin(StyleBox.Margin.All, 12);
            sliderGrabBox.SetPatchMargin(StyleBox.Margin.All, 12);

            var sliderFillGreen = new StyleBoxTexture(sliderFillBox) { Modulate = Color.LimeGreen };
            var sliderFillRed = new StyleBoxTexture(sliderFillBox) { Modulate = Color.Red };
            var sliderFillBlue = new StyleBoxTexture(sliderFillBox) { Modulate = Color.Blue };
            var sliderFillWhite = new StyleBoxTexture(sliderFillBox) { Modulate = Color.FromHex("#D5FFE0") };

            var optionsSliderBack = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0F1A13"),
                BorderThickness = new Thickness(1),
                BorderColor = Color.FromHex("#2F6B46"),
            };

            var optionsSliderFore = new StyleBoxFlat
            {
                BackgroundColor = Color.Transparent,
                BorderThickness = new Thickness(1),
                BorderColor = Color.FromHex("#2F6B46"),
            };

            var optionsSliderFill = new StyleBoxFlat
            {
                BackgroundColor = CurrentTheme == UiColorTheme.Blue
                    ? ButtonColorHovered
                    : ButtonColorHovered,
            };

            var optionsSliderGrab = new StyleBoxFlat
            {
                BackgroundColor = CurrentTheme == UiColorTheme.Blue
                    ? ButtonColorHovered
                    : ButtonColorHovered,
                BorderThickness = new Thickness(1),
                BorderColor = CurrentTheme == UiColorTheme.Blue
                    ? ButtonColorHovered
                    : ButtonColorHovered,
            };

            var boxFont13 = resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 13);

            var insetBack = new StyleBoxTexture
            {
                Texture = buttonTex,
                Modulate = Color.FromHex("#0F1A13"),
            };
            insetBack.SetPatchMargin(StyleBox.Margin.All, 10);

            // Default paper background:
            var paperBackground = new StyleBoxTexture
            {
                Texture = resCache.GetTexture("/Textures/Interface/Paper/paper_background_default.svg.96dpi.png"),
                Modulate = Color.FromHex("#DFFFE6"), // A light cream
            };
            paperBackground.SetPatchMargin(StyleBox.Margin.All, 16.0f);

            var contextMenuExpansionTexture = resCache.GetTexture("/Textures/Interface/VerbIcons/group.svg.192dpi.png");
            var verbMenuConfirmationTexture = resCache.GetTexture("/Textures/Interface/VerbIcons/group.svg.192dpi.png");

            // south-facing arrow:
            var directionIconArrowTex = resCache.GetTexture("/Textures/Interface/VerbIcons/drop.svg.192dpi.png");
            var directionIconQuestionTex = resCache.GetTexture("/Textures/Interface/VerbIcons/information.svg.192dpi.png");
            var directionIconHereTex = resCache.GetTexture("/Textures/Interface/VerbIcons/dot.svg.192dpi.png");

            Stylesheet = new Stylesheet(BaseRules.Concat(new[]
            {
                Element().Class("monospace")
                    .Prop("font", notoSansMono),
                // Window title.
                new StyleRule(
                    new SelectorElement(typeof(Label), new[] {DefaultWindow.StyleClassWindowTitle}, null, null),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, Color.Transparent),
                        new StyleProperty(Label.StylePropertyFont, notoSansDisplayBold14),
                    }),
                // Alert (white) window title.
                new StyleRule(
                    new SelectorElement(typeof(Label), new[] {"windowTitleAlert"}, null, null),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#D5FFE0")),
                        new StyleProperty(Label.StylePropertyFont, notoSansDisplayBold14),
                    }),
                // Window background.
                new StyleRule(
                    new SelectorElement(null, new[] {DefaultWindow.StyleClassWindowPanel}, null, null),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, windowBackground),
                    }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(PanelContainer), new[] {DefaultWindow.StyleClassWindowPanel}, null, null)),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, optionsWindowBackground),
                    }),
                Element<PanelContainer>().Class("OptionsGeneralBackground")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxTexture
                    {
                        Texture = resCache.GetTexture("/Textures/_CCM14/Lobby/rightside_chat_bg.png"),
                        Modulate = PanelDark.WithAlpha(1f),
                    }),
                // CCM rework ui - start
                Element<PanelContainer>().Class("CCMEscapeMenuBackground")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = CurrentTheme == UiColorTheme.Blue
                            ? Color.FromHex("#061223").WithAlpha(0.9f)
                            : Color.FromHex("#071B0D").WithAlpha(0.9f),
                    }),
                // CCM rework ui - end
                // bordered window background
                new StyleRule(
                    new SelectorElement(null, new[] {StyleClassBorderedWindowPanel}, null, null),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, borderedWindowBackground),
                    }),
                new StyleRule(
                    new SelectorElement(null, new[] {StyleClassTransparentBorderedWindowPanel}, null, null),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, borderedTransparentWindowBackground),
                    }),
                // inventory slot background
                new StyleRule(
                    new SelectorElement(null, new[] {StyleClassInventorySlotBackground}, null, null),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, invSlotBg),
                    }),
                // hand slot highlight
                new StyleRule(
                    new SelectorElement(null, new[] {StyleClassHandSlotHighlight}, null, null),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, handSlotHighlight),
                    }),
                // Hotbar background
                new StyleRule(new SelectorElement(typeof(PanelContainer), new[] {StyleClassHotbarPanel}, null, null),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, hotbarBackground),
                    }),
                // Window header.
                new StyleRule(
                    new SelectorElement(typeof(PanelContainer), new[] {DefaultWindow.StyleClassWindowHeader}, null, null),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, windowHeader),
                    }),
                // Alert (red) window header.
                new StyleRule(
                    new SelectorElement(typeof(PanelContainer), new[] {"windowHeaderAlert"}, null, null),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, windowHeaderAlert),
                    }),

                // Shapes for the buttons.
                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Prop(ContainerButton.StylePropertyStyleBox, BaseButton),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Class(ButtonOpenRight)
                    .Prop(ContainerButton.StylePropertyStyleBox, BaseButtonOpenRight),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Class(ButtonOpenLeft)
                    .Prop(ContainerButton.StylePropertyStyleBox, BaseButtonOpenLeft),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Class(ButtonOpenBoth)
                    .Prop(ContainerButton.StylePropertyStyleBox, BaseButtonOpenBoth),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Class(ButtonSquare)
                    .Prop(ContainerButton.StylePropertyStyleBox, BaseButtonSquare),

                Element<OptionButton>()
                    .Prop(ContainerButton.StylePropertyStyleBox, optionButtonOpenBoth),

                Element<MultiselectOptionButton<object>>()
                    .Prop(ContainerButton.StylePropertyStyleBox, optionButtonOpenBoth),

                new StyleRule(new SelectorElement(typeof(Label), new[] { Button.StyleClassButton }, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                }),

                // Colors for the buttons.
                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorDefault),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorHovered),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorPressed),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorDisabled),

                // Colors for the caution buttons.
                Element<ContainerButton>().Class(ContainerButton.StyleClassButton).Class(ButtonCaution)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionDefault),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton).Class(ButtonCaution)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionHovered),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton).Class(ButtonCaution)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionPressed),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton).Class(ButtonCaution)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionDisabled),

                // Colors for confirm buttons confirm states.
                Element<ConfirmButton>()
                    .Pseudo(ConfirmButton.ConfirmPrefix + ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionDefault),

                Element<ConfirmButton>()
                    .Pseudo(ConfirmButton.ConfirmPrefix + ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionHovered),

                Element<ConfirmButton>()
                    .Pseudo(ConfirmButton.ConfirmPrefix + ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionPressed),

                Element<ConfirmButton>()
                    .Pseudo(ConfirmButton.ConfirmPrefix + ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionDisabled),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), null, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty("font-color", Color.FromHex("#D5FFD481")),
                    }),

                // ItemStatus for hands
                Element()
                    .Class(StyleClassItemStatusNotHeld)
                    .Prop("font", notoSansItalic10)
                    .Prop("font-color", ItemStatusNotHeldColor)
                    .Prop(nameof(Control.Margin), new Thickness(4, 0, 0, 2)),

                Element()
                    .Class(StyleClassItemStatus)
                    .Prop(nameof(RichTextLabel.LineHeightScale), 0.7f)
                    .Prop(nameof(Control.Margin), new Thickness(4, 0, 0, 2)),

                // Context Menu window
                Element<PanelContainer>().Class(ContextMenuPopup.StyleClassContextMenuPopup)
                    .Prop(PanelContainer.StylePropertyPanel, contextMenuBackground),

                // Context menu buttons
                Element<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton)
                    .Prop(ContainerButton.StylePropertyStyleBox, buttonContext),

                Element<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorContext),

                Element<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorContextHover),

                Element<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorContextPressed),

                Element<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorContextDisabled),

                // Context Menu Labels
                Element<RichTextLabel>().Class(InteractionVerb.DefaultTextStyleClass)
                    .Prop(Label.StylePropertyFont, notoSansBoldItalic12),

                Element<RichTextLabel>().Class(ActivationVerb.DefaultTextStyleClass)
                    .Prop(Label.StylePropertyFont, notoSansBold12),

                Element<RichTextLabel>().Class(AlternativeVerb.DefaultTextStyleClass)
                    .Prop(Label.StylePropertyFont, notoSansItalic12),

                Element<RichTextLabel>().Class(Verb.DefaultTextStyleClass)
                    .Prop(Label.StylePropertyFont, notoSans12),

                Element<TextureRect>().Class(ContextMenuElement.StyleClassContextMenuExpansionTexture)
                    .Prop(TextureRect.StylePropertyTexture, contextMenuExpansionTexture),

                Element<TextureRect>().Class(VerbMenuElement.StyleClassVerbMenuConfirmationTexture)
                    .Prop(TextureRect.StylePropertyTexture, verbMenuConfirmationTexture),

                // Context menu confirm buttons
                Element<ContextMenuElement>().Class(ConfirmationMenuElement.StyleClassConfirmationContextMenuButton)
                    .Prop(ContainerButton.StylePropertyStyleBox, buttonContext),

                Element<ContextMenuElement>().Class(ConfirmationMenuElement.StyleClassConfirmationContextMenuButton)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionDefault),

                Element<ContextMenuElement>().Class(ConfirmationMenuElement.StyleClassConfirmationContextMenuButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionHovered),

                Element<ContextMenuElement>().Class(ConfirmationMenuElement.StyleClassConfirmationContextMenuButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionPressed),

                Element<ContextMenuElement>().Class(ConfirmationMenuElement.StyleClassConfirmationContextMenuButton)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorCautionDisabled),

                // Examine buttons
                Element<ExamineButton>().Class(ExamineButton.StyleClassExamineButton)
                    .Prop(ContainerButton.StylePropertyStyleBox, buttonContext),

                Element<ExamineButton>().Class(ExamineButton.StyleClassExamineButton)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ExamineButtonColorContext),

                Element<ExamineButton>().Class(ExamineButton.StyleClassExamineButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ExamineButtonColorContextHover),

                Element<ExamineButton>().Class(ExamineButton.StyleClassExamineButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, ExamineButtonColorContextPressed),

                Element<ExamineButton>().Class(ExamineButton.StyleClassExamineButton)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, ExamineButtonColorContextDisabled),

                // Direction / arrow icon
                Element<DirectionIcon>().Class(DirectionIcon.StyleClassDirectionIconArrow)
                    .Prop(TextureRect.StylePropertyTexture, directionIconArrowTex),

                Element<DirectionIcon>().Class(DirectionIcon.StyleClassDirectionIconUnknown)
                    .Prop(TextureRect.StylePropertyTexture, directionIconQuestionTex),

                Element<DirectionIcon>().Class(DirectionIcon.StyleClassDirectionIconHere)
                    .Prop(TextureRect.StylePropertyTexture, directionIconHereTex),

                // Thin buttons (No padding nor vertical margin)
                Element<ContainerButton>().Class(StyleClassStorageButton)
                    .Prop(ContainerButton.StylePropertyStyleBox, buttonStorage),

                Element<ContainerButton>().Class(StyleClassStorageButton)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorDefault),

                Element<ContainerButton>().Class(StyleClassStorageButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorHovered),

                Element<ContainerButton>().Class(StyleClassStorageButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorPressed),

                Element<ContainerButton>().Class(StyleClassStorageButton)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorDisabled),
// ListContainer
                Element<ContainerButton>().Class(ListContainer.StyleClassListContainerButton)
                    .Prop(ContainerButton.StylePropertyStyleBox, listContainerButton),

                Element<ContainerButton>().Class(ListContainer.StyleClassListContainerButton)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, new Color(55, 55, 68)),

                Element<ContainerButton>().Class(ListContainer.StyleClassListContainerButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, new Color(75, 75, 86)),

                Element<ContainerButton>().Class(ListContainer.StyleClassListContainerButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, new Color(75, 75, 86)),

                Element<ContainerButton>().Class(ListContainer.StyleClassListContainerButton)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, new Color(10, 10, 12)),

                // Main menu: Make those buttons bigger.
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), null, "mainMenu", null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty("font", notoSansBold16),
                    }),

                // Main menu: also make those buttons slightly more separated.
                new StyleRule(new SelectorElement(typeof(BoxContainer), null, "mainMenuVBox", null),
                    new[]
                    {
                        new StyleProperty(BoxContainer.StylePropertySeparation, 2),
                    }),

                // Fancy LineEdit
                new StyleRule(new SelectorElement(typeof(LineEdit), null, null, null),
                    new[]
                    {
                        new StyleProperty(LineEdit.StylePropertyStyleBox, lineEdit),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(LineEdit), new[] {LineEdit.StyleClassLineEditNotEditable}, null, null),
                    new[]
                    {
                        new StyleProperty("font-color", new Color(192, 192, 192)),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(LineEdit), null, null, new[] {LineEdit.StylePseudoClassPlaceholder}),
                    new[]
                    {
                        new StyleProperty("font-color", Color.FromHex("#3A6B47")),
                    }),

                Element<TextEdit>().Pseudo(TextEdit.StylePseudoClassPlaceholder)
                    .Prop("font-color", Color.FromHex("#3A6B47")),

                // chat subpanels (chat lineedit backing, popup backings)
                new StyleRule(new SelectorElement(typeof(PanelContainer), new[] {StyleClassChatPanel}, null, null),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, chatBg),
                    }),

                Element<PanelContainer>().Class(StyleClassLobbyCenterPanel).Class(StyleClassLobbyThemeCrt)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyPanelCrt),

                Element<PanelContainer>().Class(StyleClassLobbyCenterGlow).Class(StyleClassLobbyThemeCrt)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyPanelGlowCrt)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#D5FFE0")),

                Element<PanelContainer>().Class(StyleClassLobbyCenterPanel).Class(StyleClassLobbyThemeClean)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyPanelClean),

                Element<PanelContainer>().Class(StyleClassLobbyCenterGlow).Class(StyleClassLobbyThemeClean)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyPanelGlowClean)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#D5FFE0")),

                Element<PanelContainer>().Class(StyleClassLobbyInfoPanel).Class(StyleClassLobbyThemeCrt)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyInfoPanelCrt),

                Element<PanelContainer>().Class(StyleClassLobbyInfoPanel).Class(StyleClassLobbyThemeClean)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyInfoPanelClean),

                Element<PanelContainer>().Class(StyleClassLobbyMusicPanel).Class(StyleClassLobbyThemeCrt)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyMusicPanelCrt),

                Element<PanelContainer>().Class(StyleClassLobbyMusicPanel).Class(StyleClassLobbyThemeClean)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyMusicPanelClean),

                Element<PanelContainer>().Class(StyleClassLobbyInfoDivider).Class(StyleClassLobbyThemeCrt)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyInfoDividerCrt),

                Element<PanelContainer>().Class(StyleClassLobbyInfoDivider).Class(StyleClassLobbyThemeClean)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyInfoDividerClean),

                Element<PanelContainer>().Class(StyleClassLobbyChatPanel).Class(StyleClassLobbyThemeCrt)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyChatPanelCrt),

                Element<PanelContainer>().Class(StyleClassLobbyChatPanel).Class(StyleClassLobbyThemeClean)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyChatPanelClean),

                Element<PanelContainer>().Class(StyleClassLobbyChatPanelInner).Class(StyleClassLobbyThemeCrt)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyChatPanelCrt),

                Element<PanelContainer>().Class(StyleClassLobbyChatPanelInner).Class(StyleClassLobbyThemeClean)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyChatPanelClean),

                Element<PanelContainer>().Class(StyleClassLobbyChatInputPanel).Class(StyleClassLobbyThemeCrt)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyChatInputCrt),

                Element<PanelContainer>().Class(StyleClassLobbyChatInputPanel).Class(StyleClassLobbyThemeClean)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyChatInputClean),

                Element<PanelContainer>().Class(StyleClassLobbyMenuDivider).Class(StyleClassLobbyThemeCrt)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyMenuDividerCrt),

                Element<PanelContainer>().Class(StyleClassLobbyMenuDivider).Class(StyleClassLobbyThemeClean)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyMenuDividerClean),

                Element<TextureRect>().Class(StyleClassLobbyEmblem).Class(StyleClassLobbyThemeCrt)
                    .Prop(Control.StylePropertyModulateSelf, LobbyMenuButtonBase),

                Element<TextureRect>().Class(StyleClassLobbyEmblem).Class(StyleClassLobbyThemeClean)
                    .Prop(Control.StylePropertyModulateSelf, LobbyMenuButtonBase),

                Element<Label>().Class(StyleClassLobbyWelcomeLine1).Class(StyleClassLobbyThemeCrt)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, LobbyCrtText),

                Element<Label>().Class(StyleClassLobbyWelcomeLine2).Class(StyleClassLobbyThemeCrt)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, notoSansBold18)
                    .Prop(Label.StylePropertyFontColor, LobbyMenuButtonBase),

                Element<Label>().Class(StyleClassLobbyWelcomeLine3).Class(StyleClassLobbyThemeCrt)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, notoSansBold16)
                    .Prop(Label.StylePropertyFontColor, LobbyCrtMutedText),

                Element<Label>().Class(StyleClassLobbyWelcomeLine1).Class(StyleClassLobbyThemeClean)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, LobbyCleanText),

                Element<Label>().Class(StyleClassLobbyWelcomeLine2).Class(StyleClassLobbyThemeClean)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, notoSansBold18)
                    .Prop(Label.StylePropertyFontColor, LobbyMenuButtonBase),

                Element<Label>().Class(StyleClassLobbyWelcomeLine3).Class(StyleClassLobbyThemeClean)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, notoSansBold16)
                    .Prop(Label.StylePropertyFontColor, LobbyCleanMutedText),

                Element<Label>().Class(StyleClassLobbyCountdown).Class(StyleClassLobbyThemeCrt)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Left)
                    .Prop(Label.StylePropertyFont, notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, LobbyCrtAccent),

                Element<Label>().Class(StyleClassLobbyCountdown).Class(StyleClassLobbyThemeClean)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Left)
                    .Prop(Label.StylePropertyFont, notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, LobbyCleanAccent),

                Element<Label>().Class(StyleClassLobbyInfoTitle).Class(StyleClassLobbyThemeCrt)
                    .Prop(Label.StylePropertyFont, bedstead14)
                    .Prop(Label.StylePropertyFontColor, LobbyCrtAccent),

                Element<Label>().Class(StyleClassLobbyInfoTitle).Class(StyleClassLobbyThemeClean)
                    .Prop(Label.StylePropertyFont, bedstead14)
                    .Prop(Label.StylePropertyFontColor, LobbyCleanAccent),

                Element<Label>().Class(StyleClassLobbyMusicHeader).Class(StyleClassLobbyThemeCrt)
                    .Prop(Label.StylePropertyFont, bedstead12)
                    .Prop(Label.StylePropertyFontColor, LobbyCrtMutedText),

                Element<Label>().Class(StyleClassLobbyMusicHeader).Class(StyleClassLobbyThemeClean)
                    .Prop(Label.StylePropertyFont, bedstead12)
                    .Prop(Label.StylePropertyFontColor, LobbyCleanMutedText),

                Element<Label>().Class(StyleClassLobbyInfoLine).Class(StyleClassLobbyThemeCrt)
                    .Prop(Label.StylePropertyFont, notoSans12)
                    .Prop(Label.StylePropertyFontColor, LobbyCrtMutedText),

                Element<Label>().Class(StyleClassLobbyInfoLine).Class(StyleClassLobbyThemeClean)
                    .Prop(Label.StylePropertyFont, notoSans12)
                    .Prop(Label.StylePropertyFontColor, LobbyCleanMutedText),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassLobbyInfoText, StyleClassLobbyThemeCrt}, null, null),
                    new SelectorElement(typeof(RichTextLabel), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSans12),
                        new StyleProperty("font-color", LobbyCrtMutedText)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassLobbyInfoText, StyleClassLobbyThemeClean}, null, null),
                    new SelectorElement(typeof(RichTextLabel), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSans12),
                        new StyleProperty("font-color", LobbyCleanMutedText)
                    }),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCrt),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCrtHover),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCrtPressed),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyReadyButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCrtReadyPressed),



                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCrtDisabled),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeClean)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonClean),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCleanHover),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCleanPressed),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyReadyButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCleanReadyPressed),



                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCleanDisabled),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassLobbyMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeCrt}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                          new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Left),
                            new StyleProperty(nameof(Control.Margin), new Thickness(40, 0, 0, 0)),
                          new StyleProperty(Label.StylePropertyFont, bedstead15),
                          new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#000000"))
                      }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeClean}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                          new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Left),
                            new StyleProperty(nameof(Control.Margin), new Thickness(40, 0, 0, 0)),
                          new StyleProperty(Label.StylePropertyFont, bedstead15),
                          new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#000000"))
                      }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeCrt}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, LobbyCrtAccent)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeClean}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, LobbyCrtAccent)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeCrt}, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#000000"))
                    }),

                  new StyleRule(new SelectorChild(
                      new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeClean}, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                      new SelectorElement(typeof(Label), null, null, null)),
                      new[]
                      {
                          new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#000000"))
                      }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeCrt}, null, null),
                    new SelectorElement(typeof(TextureRect), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.FromHex("#000000"))
                    }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeClean}, null, null),
                    new SelectorElement(typeof(TextureRect), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.FromHex("#000000"))
                    }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeCrt}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(TextureRect), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, LobbyCrtAccent)
                    }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeClean}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(TextureRect), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, LobbyCrtAccent)
                    }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeCrt}, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                    new SelectorElement(typeof(TextureRect), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.FromHex("#000000"))
                    }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassLobbyThemeClean}, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                    new SelectorElement(typeof(TextureRect), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.FromHex("#000000"))
                    }),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton).Class(StyleClassLobbyThemeCrt)
                    .Prop(ContainerButton.StylePropertyStyleBox, lobbyMenuButtonCrt),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(ContainerButton.StylePropertyStyleBox, lobbyMenuButtonCrtHover),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(ContainerButton.StylePropertyStyleBox, lobbyMenuButtonCrtPressed),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(ContainerButton.StylePropertyStyleBox, lobbyMenuButtonCrtDisabled),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton).Class(StyleClassLobbyThemeClean)
                    .Prop(ContainerButton.StylePropertyStyleBox, lobbyMenuButtonClean),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(ContainerButton.StylePropertyStyleBox, lobbyMenuButtonCleanHover),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(ContainerButton.StylePropertyStyleBox, lobbyMenuButtonCleanPressed),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(ContainerButton.StylePropertyStyleBox, lobbyMenuButtonCleanDisabled),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#D5FFE0")),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#D5FFE0")),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#D5FFE0")),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#D5FFE0")),

                  new StyleRule(new SelectorChild(
                      new SelectorElement(typeof(ContainerButton), new[] {StyleClassLobbyMenuIconButton}, null, null),
                      new SelectorElement(typeof(TextureRect), null, null, null)),
                      new[]
                      {
                          new StyleProperty(Control.StylePropertyModulateSelf, Color.FromHex("#000000"))
                      }),

                  new StyleRule(new SelectorChild(
                      new SelectorElement(typeof(ContainerButton), new[] {StyleClassLobbyMenuIconButton}, null, new[] {ContainerButton.StylePseudoClassHover}),
                      new SelectorElement(typeof(TextureRect), null, null, null)),
                      new[]
                      {
                          new StyleProperty(Control.StylePropertyModulateSelf, LobbyCrtAccent)
                      }),

                Element<Button>().Class(StyleClassLobbyTopButton).Class(StyleClassLobbyThemeCrt)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCrt),

                Element<Button>().Class(StyleClassLobbyTopButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCrtHover),

                Element<Button>().Class(StyleClassLobbyTopButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCrtPressed),

                Element<Button>().Class(StyleClassLobbyTopButton).Class(StyleClassLobbyThemeClean)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonClean),

                Element<Button>().Class(StyleClassLobbyTopButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCleanHover),

                Element<Button>().Class(StyleClassLobbyTopButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCleanPressed),

                Element<Button>().Class(StyleClassLobbyTopButton)
                    .Prop(nameof(Control.Margin), new Thickness(4, 0, 4, 0)),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyTopButton, StyleClassLobbyThemeCrt}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                        new StyleProperty(Label.StylePropertyFontColor, LobbyCrtText),
                        new StyleProperty(nameof(Control.Margin), new Thickness(8, 0, 8, 0))
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyTopButton, StyleClassLobbyThemeClean}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                        new StyleProperty(Label.StylePropertyFontColor, LobbyCleanText),
                        new StyleProperty(nameof(Control.Margin), new Thickness(8, 0, 8, 0))
                    }),

                Element<Button>().Class(StyleClassLobbyChatSelectorButton).Class(StyleClassLobbyThemeCrt)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCrt),

                Element<Button>().Class(StyleClassLobbyChatSelectorButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCrtHover),

                Element<Button>().Class(StyleClassLobbyChatSelectorButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCrtPressed),

                Element<Button>().Class(StyleClassLobbyChatSelectorButton).Class(StyleClassLobbyThemeClean)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonClean),

                Element<Button>().Class(StyleClassLobbyChatSelectorButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCleanHover),

                Element<Button>().Class(StyleClassLobbyChatSelectorButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCleanPressed),

                Element<Button>().Class(StyleClassLobbyChatFilterButton).Class(StyleClassLobbyThemeCrt)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCrt),

                Element<Button>().Class(StyleClassLobbyChatFilterButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCrtHover),

                Element<Button>().Class(StyleClassLobbyChatFilterButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCrtPressed),

                Element<Button>().Class(StyleClassLobbyChatFilterButton).Class(StyleClassLobbyThemeClean)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonClean),

                Element<Button>().Class(StyleClassLobbyChatFilterButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCleanHover),

                Element<Button>().Class(StyleClassLobbyChatFilterButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyButtonCleanPressed),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyChatSelectorButton, StyleClassLobbyThemeCrt}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSans12),
                        new StyleProperty(Label.StylePropertyFontColor, LobbyCrtText)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyChatSelectorButton, StyleClassLobbyThemeClean}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSans12),
                        new StyleProperty(Label.StylePropertyFontColor, LobbyCleanText)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyChatFilterButton, StyleClassLobbyThemeCrt}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSans12),
                        new StyleProperty(Label.StylePropertyFontColor, LobbyCrtText)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyChatFilterButton, StyleClassLobbyThemeClean}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSans12),
                        new StyleProperty(Label.StylePropertyFontColor, LobbyCleanText)
                    }),

                Element<LineEdit>().Class(StyleClassLobbyChatLineEdit).Class(StyleClassLobbyThemeCrt)
                    .Prop("font-color", LobbyCrtText)
                    .Prop(LineEdit.StylePropertyCursorColor, LobbyCrtAccent)
                    .Prop(LineEdit.StylePropertySelectionColor, LobbyCrtAccent.WithAlpha(0.35f)),

                Element<LineEdit>().Class(StyleClassLobbyChatLineEdit).Class(StyleClassLobbyThemeClean)
                    .Prop("font-color", LobbyCleanText)
                    .Prop(LineEdit.StylePropertyCursorColor, LobbyCleanAccent)
                    .Prop(LineEdit.StylePropertySelectionColor, LobbyCleanAccent.WithAlpha(0.35f)),

                Element<LineEdit>().Class(StyleClassLobbyChatLineEdit).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(LineEdit.StylePseudoClassPlaceholder)
                    .Prop("font-color", LobbyCrtMutedText),

                Element<LineEdit>().Class(StyleClassLobbyChatLineEdit).Class(StyleClassLobbyThemeClean)
                    .Pseudo(LineEdit.StylePseudoClassPlaceholder)
                    .Prop("font-color", LobbyCleanMutedText),

                // Chat lineedit - we don't actually draw a stylebox around the lineedit itself, we put it around the
                // input + other buttons, so we must clear the default stylebox
                new StyleRule(new SelectorElement(typeof(LineEdit), new[] {StyleClassChatLineEdit}, null, null),
                    new[]
                    {
                        new StyleProperty(LineEdit.StylePropertyStyleBox, new StyleBoxEmpty()),
                    }),

                new StyleRule(new SelectorElement(typeof(LineEdit), new[] {StyleClassChatLineEdit}, null, null),
                    new[]
                    {
                        new StyleProperty("font", exo2Regular12),
                    }),

                new StyleRule(new SelectorElement(typeof(LineEdit), new[] {StyleClassLobbyChatLineEdit}, null, null),
                    new[]
                    {
                        new StyleProperty("font", exo2Regular12),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(PanelContainer), new[] {StyleClassChatPanel}, null, null),
                    new SelectorElement(typeof(OutputPanel), null, null, null)),
                    new[]
                    {
                        new StyleProperty("font", exo2Regular12),
                    }),

                Element<OutputPanel>().Class(StyleClassChatOutput)
                    .Prop("font", exo2Regular12),

                // Action searchbox lineedit
                new StyleRule(new SelectorElement(typeof(LineEdit), new[] {StyleClassActionSearchBox}, null, null),
                    new[]
                    {
                        new StyleProperty(LineEdit.StylePropertyStyleBox, actionSearchBox),
                    }),

                // TabContainer
                new StyleRule(new SelectorElement(typeof(TabContainer), null, null, null),
                    new[]
                    {
                        new StyleProperty("font", notoSansBold12),
                        new StyleProperty(TabContainer.StylePropertyPanelStyleBox, tabContainerPanel),
                        new StyleProperty(TabContainer.StylePropertyTabStyleBox, tabContainerBoxActive),
                        new StyleProperty(TabContainer.StylePropertyTabStyleBoxInactive, tabContainerBoxInactive),
                    }),
                // CCM rework lobby - start
                new StyleRule(new SelectorElement(typeof(Content.Client._CCM.UserInterface.Controls.CenteredTabContainer), null, null, null),
                    new[]
                    {
                        new StyleProperty("font", notoSansBold12),
                        new StyleProperty(TabContainer.StylePropertyPanelStyleBox, tabContainerPanel),
                        new StyleProperty(TabContainer.StylePropertyTabStyleBox, tabContainerBoxActive),
                        new StyleProperty(TabContainer.StylePropertyTabStyleBoxInactive, tabContainerBoxInactive),
                    }),
                // CCM rework lobby - end

                // ProgressBar
                new StyleRule(new SelectorElement(typeof(ProgressBar), null, null, null),
                    new[]
                    {
                        new StyleProperty(ProgressBar.StylePropertyBackground, progressBarBackground),
                        new StyleProperty(ProgressBar.StylePropertyForeground, progressBarForeground)
                    }),

                // CheckBox
                new StyleRule(new SelectorElement(typeof(TextureRect), new [] { CheckBox.StyleClassCheckBox }, null, null), new[]
                {
                    new StyleProperty(TextureRect.StylePropertyTexture, checkBoxTextureUnchecked),
                }),

                new StyleRule(new SelectorElement(typeof(TextureRect), new [] { CheckBox.StyleClassCheckBox, CheckBox.StyleClassCheckBoxChecked }, null, null), new[]
                {
                    new StyleProperty(TextureRect.StylePropertyTexture, checkBoxTextureChecked),
                }),

                new StyleRule(new SelectorElement(typeof(BoxContainer), new [] { CheckBox.StyleClassCheckBox }, null, null), new[]
                {
                    new StyleProperty(BoxContainer.StylePropertySeparation, 10),
                }),

                // MonotoneCheckBox
                new StyleRule(new SelectorElement(typeof(TextureRect), new [] { MonotoneCheckBox.StyleClassMonotoneCheckBox }, null, null), new[]
                {
                    new StyleProperty(TextureRect.StylePropertyTexture, monotoneCheckBoxTextureUnchecked),
                }),

                new StyleRule(new SelectorElement(typeof(TextureRect), new [] { MonotoneCheckBox.StyleClassMonotoneCheckBox, CheckBox.StyleClassCheckBoxChecked }, null, null), new[]
                {
                    new StyleProperty(TextureRect.StylePropertyTexture, monotoneCheckBoxTextureChecked),
                }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(TextureRect), new [] { CheckBox.StyleClassCheckBox }, null, null)), new[]
                {
                    new StyleProperty(TextureRect.StylePropertyTexture, monotoneCheckBoxTextureUnchecked),
                }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(TextureRect), new [] { CheckBox.StyleClassCheckBox, CheckBox.StyleClassCheckBoxChecked }, null, null)), new[]
                {
                    new StyleProperty(TextureRect.StylePropertyTexture, monotoneCheckBoxTextureChecked),
                }),

                // Tooltip
                new StyleRule(new SelectorElement(typeof(Tooltip), null, null, null), new[]
                {
                    new StyleProperty(PanelContainer.StylePropertyPanel, tooltipBox)
                }),

                new StyleRule(new SelectorElement(typeof(PanelContainer), new [] { StyleClassTooltipPanel }, null, null), new[]
                {
                    new StyleProperty(PanelContainer.StylePropertyPanel, tooltipBox)
                }),

                new StyleRule(new SelectorElement(typeof(PanelContainer), new[] {"speechBox", "sayBox"}, null, null), new[]
                {
                    new StyleProperty(PanelContainer.StylePropertyPanel, tooltipBox)
                }),

                new StyleRule(new SelectorElement(typeof(PanelContainer), new[] {"speechBox", "whisperBox"}, null, null), new[]
                {
                    new StyleProperty(PanelContainer.StylePropertyPanel, whisperBox)
                }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(PanelContainer), new[] {"speechBox", "whisperBox"}, null, null),
                    new SelectorElement(typeof(RichTextLabel), new[] {"bubbleContent"}, null, null)),
                    new[]
                {
                    new StyleProperty("font", notoSansItalic12),
                }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(PanelContainer), new[] {"speechBox", "emoteBox"}, null, null),
                    new SelectorElement(typeof(RichTextLabel), null, null, null)),
                    new[]
                {
                    new StyleProperty("font", notoSansItalic12),
                }),

                // RMC14
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(PanelContainer), new[] { "speechBox", "commanderSpeech" }, null, null),
                    new SelectorElement(typeof(RichTextLabel), new[] { "bubbleContent" }, null, null)),
                    new[]
                {
                    new StyleProperty("font", notoSansBold16),
                }),

                // RMC14
                new StyleRule(new SelectorElement(typeof(PanelContainer), new[] {"speechBox", "commanderSpeech"}, null, null), new[]
                {
                    new StyleProperty(PanelContainer.StylePropertyPanel, tooltipBox)
                }),

                // RMC14
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(PanelContainer), new[] { "speechBox", "megaphoneSpeech" }, null, null),
                    new SelectorElement(typeof(RichTextLabel), new[] { "bubbleContent" }, null, null)),
                    new[]
                {
                    new StyleProperty("font", resCache.NotoStack(variation: "Bold", size: 20)),
                }),

                // RMC14
                new StyleRule(new SelectorElement(typeof(PanelContainer), new[] {"speechBox", "megaphoneSpeech"}, null, null), new[]
                {
                    new StyleProperty(PanelContainer.StylePropertyPanel, tooltipBox)
                }),

                new StyleRule(new SelectorElement(typeof(RichTextLabel), new[] {StyleClassLabelKeyText}, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                    new StyleProperty( Control.StylePropertyModulateSelf, NanoGold)
                }),

                // alert tooltip
                new StyleRule(new SelectorElement(typeof(RichTextLabel), new[] {StyleClassTooltipAlertTitle}, null, null), new[]
                {
                    new StyleProperty("font", notoSansBold18)
                }),
                new StyleRule(new SelectorElement(typeof(RichTextLabel), new[] {StyleClassTooltipAlertDescription}, null, null), new[]
                {
                    new StyleProperty("font", notoSans16)
                }),
                new StyleRule(new SelectorElement(typeof(RichTextLabel), new[] {StyleClassTooltipAlertCooldown}, null, null), new[]
                {
                    new StyleProperty("font", notoSans16)
                }),

                // action tooltip
                new StyleRule(new SelectorElement(typeof(RichTextLabel), new[] {StyleClassTooltipActionTitle}, null, null), new[]
                {
                    new StyleProperty("font", notoSansBold16)
                }),
                new StyleRule(new SelectorElement(typeof(RichTextLabel), new[] {StyleClassTooltipActionDescription}, null, null), new[]
                {
                    new StyleProperty("font", notoSans15)
                }),
                new StyleRule(new SelectorElement(typeof(RichTextLabel), new[] {StyleClassTooltipActionCooldown}, null, null), new[]
                {
                    new StyleProperty("font", notoSans15)
                }),
                new StyleRule(new SelectorElement(typeof(RichTextLabel), new[] {StyleClassTooltipActionDynamicMessage}, null, null), new[]
                {
                    new StyleProperty("font", notoSans15)
                }),
                new StyleRule(new SelectorElement(typeof(RichTextLabel), new[] {StyleClassTooltipActionRequirements}, null, null), new[]
                {
                    new StyleProperty("font", notoSans15)
                }),
                new StyleRule(new SelectorElement(typeof(RichTextLabel), new[] {StyleClassTooltipActionCharges}, null, null), new[]
                {
                    new StyleProperty("font", notoSans15)
                }),

                // small number for the entity counter in the entity menu
                new StyleRule(new SelectorElement(typeof(Label), new[] {ContextMenuElement.StyleClassEntityMenuIconLabel}, null, null), new[]
                {
                    new StyleProperty("font", notoSans10),
                    new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Right),
                }),

                // hotbar slot
                new StyleRule(new SelectorElement(typeof(RichTextLabel), new[] {StyleClassHotbarSlotNumber}, null, null), new[]
                {
                    new StyleProperty("font", notoSansDisplayBold16)
                }),

                // Entity tooltip
                new StyleRule(
                    new SelectorElement(typeof(PanelContainer), new[] {ExamineSystem.StyleClassEntityTooltip}, null,
                        null), new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, tooltipBox)
                    }),

                // ItemList
                new StyleRule(new SelectorElement(typeof(ItemList), null, null, null), new[]
                {
                    new StyleProperty(ItemList.StylePropertyBackground,
                        new StyleBoxFlat {BackgroundColor = new Color(32, 32, 40)}),
                    new StyleProperty(ItemList.StylePropertyItemBackground,
                        itemListItemBackground),
                    new StyleProperty(ItemList.StylePropertyDisabledItemBackground,
                        itemListItemBackgroundDisabled),
                    new StyleProperty(ItemList.StylePropertySelectedItemBackground,
                        itemListBackgroundSelected)
                }),

                new StyleRule(new SelectorElement(typeof(ItemList), new[] {"transparentItemList"}, null, null), new[]
                {
                    new StyleProperty(ItemList.StylePropertyBackground,
                        new StyleBoxFlat {BackgroundColor = Color.Transparent}),
                    new StyleProperty(ItemList.StylePropertyItemBackground,
                        itemListItemBackgroundTransparent),
                    new StyleProperty(ItemList.StylePropertyDisabledItemBackground,
                        itemListItemBackgroundDisabled),
                    new StyleProperty(ItemList.StylePropertySelectedItemBackground,
                        itemListBackgroundSelected)
                }),

                 new StyleRule(new SelectorElement(typeof(ItemList), new[] {"transparentBackgroundItemList"}, null, null), new[]
                {
                    new StyleProperty(ItemList.StylePropertyBackground,
                        new StyleBoxFlat {BackgroundColor = Color.Transparent}),
                    new StyleProperty(ItemList.StylePropertyItemBackground,
                        itemListItemBackground),
                    new StyleProperty(ItemList.StylePropertyDisabledItemBackground,
                        itemListItemBackgroundDisabled),
                    new StyleProperty(ItemList.StylePropertySelectedItemBackground,
                        itemListBackgroundSelected)
                }),

                // Tree
                new StyleRule(new SelectorElement(typeof(Tree), null, null, null), new[]
                {
                    new StyleProperty(Tree.StylePropertyBackground,
                        new StyleBoxFlat {BackgroundColor = new Color(32, 32, 40)}),
                    new StyleProperty(Tree.StylePropertyItemBoxSelected, new StyleBoxFlat
                    {
                        BackgroundColor = new Color(55, 55, 68),
                        ContentMarginLeftOverride = 4
                    })
                }),

                // Placeholder
                new StyleRule(new SelectorElement(typeof(Placeholder), null, null, null), new[]
                {
                    new StyleProperty(PanelContainer.StylePropertyPanel, placeholder),
                }),

                new StyleRule(
                    new SelectorElement(typeof(Label), new[] {Placeholder.StyleClassPlaceholderText}, null, null), new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSans16),
                        new StyleProperty(Label.StylePropertyFontColor, new Color(103, 103, 103, 128)),
                    }),

                // Big Label
                new StyleRule(new SelectorElement(typeof(Label), new[] {StyleClassLabelHeading}, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyFont, bedstead16),
                    new StyleProperty(Label.StylePropertyFontColor, NanoGold),
                }),

                // Bigger Label
                new StyleRule(new SelectorElement(typeof(Label), new[] {StyleClassLabelHeadingBigger}, null, null),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, bedstead20),
                        new StyleProperty(Label.StylePropertyFontColor, NanoGold),
                    }),

                // Small Label
                new StyleRule(new SelectorElement(typeof(Label), new[] {StyleClassLabelSubText}, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyFont, exo2Regular12),
                    new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#3A6B47")),
                }),

                new StyleRule(new SelectorElement(typeof(Label), new[] {"OptionSettingLabel"}, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyFont, exo2Regular12),
                    new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#B3B3B3")),
                }),

                // Label Key
                new StyleRule(new SelectorElement(typeof(Label), new[] {StyleClassLabelKeyText}, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyFont, exo2Regular12),
                    new StyleProperty(Label.StylePropertyFontColor, NanoGold)
                }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleBase.StyleClassVerticalTabButton}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 14)),
                        new StyleProperty(Label.StylePropertyFontColor, NanoGold),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, exo2Regular12),
                        new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#B3B3B3")),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(RichTextLabel), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, exo2Regular12),
                        new StyleProperty("font-color", Color.FromHex("#B3B3B3")),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(LineEdit), null, null, null)),
                    new[]
                    {
                        new StyleProperty("font", exo2Regular12),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                        new SelectorElement(typeof(Button), null, null, null)),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, exo2Regular12),
                        new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#B3B3B3")),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                        new SelectorElement(typeof(OptionButton), null, null, null)),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, exo2Regular12),
                        new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#B3B3B3")),
                    }),

                new StyleRule(new SelectorElement(typeof(Label), new[] {StyleClassLabelSecondaryColor}, null, null),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSans12),
                        new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#3A6B47")),
                    }),

                // Console text
                new StyleRule(new SelectorElement(typeof(Label), new[] {StyleClassConsoleText}, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyFont, robotoMonoBold11)
                }),

                new StyleRule(new SelectorElement(typeof(Label), new[] {StyleClassConsoleSubHeading}, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyFont, robotoMonoBold12)
                }),

                new StyleRule(new SelectorElement(typeof(Label), new[] {StyleClassConsoleHeading}, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyFont, robotoMonoBold14)
                }),

                // Big Button
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassButtonBig}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty("font", notoSans16)
                    }),

                //APC and SMES power state label colors
                new StyleRule(new SelectorElement(typeof(Label), new[] {StyleClassPowerStateNone}, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyFontColor, new Color(0.8f, 0.0f, 0.0f))
                }),

                new StyleRule(new SelectorElement(typeof(Label), new[] {StyleClassPowerStateLow}, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyFontColor, new Color(0.9f, 0.36f, 0.0f))
                }),

                new StyleRule(new SelectorElement(typeof(Label), new[] {StyleClassPowerStateGood}, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyFontColor, new Color(0.024f, 0.8f, 0.0f))
                }),

                // Those top menu buttons.
                // these use slight variations on the various BaseButton styles so that the content within them appears centered,
                // which is NOT the case for the default BaseButton styles (OpenLeft/OpenRight adds extra padding on one of the sides
                // which makes the TopButton icons appear off-center, which we don't want).
                new StyleRule(
                    new SelectorElement(typeof(MenuButton), new[] {ButtonSquare}, null, null),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, topButtonSquare),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MenuButton), new[] {ButtonOpenLeft}, null, null),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, topButtonOpenLeft),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MenuButton), new[] {ButtonOpenRight}, null, null),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, topButtonOpenRight),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MenuButton), null, null, new[] {Button.StylePseudoClassNormal}),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyModulateSelf, ButtonColorDefault),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MenuButton), new[] {MenuButton.StyleClassRedTopButton}, null, new[] {Button.StylePseudoClassNormal}),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyModulateSelf, ButtonColorDefaultRed),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MenuButton), null, null, new[] {Button.StylePseudoClassNormal}),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyModulateSelf, ButtonColorDefault),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MenuButton), null, null, new[] {Button.StylePseudoClassPressed}),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyModulateSelf, ButtonColorPressed),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MenuButton), null, null, new[] {Button.StylePseudoClassHover}),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyModulateSelf, ButtonColorHovered),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MenuButton), new[] {MenuButton.StyleClassRedTopButton}, null, new[] {Button.StylePseudoClassHover}),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyModulateSelf, ButtonColorHoveredRed),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(Label), new[] {MenuButton.StyleClassLabelTopButton}, null, null),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansDisplayBold14),
                    }),

                // MonotoneButton (unfilled)
                new StyleRule(
                    new SelectorElement(typeof(MonotoneButton), null, null, null),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, monotoneButton),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MonotoneButton), new[] { ButtonOpenLeft }, null, null),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, monotoneButtonOpenLeft),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MonotoneButton), new[] { ButtonOpenRight }, null, null),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, monotoneButtonOpenRight),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MonotoneButton), new[] { ButtonOpenBoth }, null, null),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, monotoneButtonOpenBoth),
                    }),

                // MonotoneButton (filled)
                new StyleRule(
                    new SelectorElement(typeof(MonotoneButton), null, null, new[] { Button.StylePseudoClassPressed }),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, monotoneFilledButton),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MonotoneButton), new[] { ButtonOpenLeft }, null, new[] { Button.StylePseudoClassPressed }),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, monotoneFilledButtonOpenLeft),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MonotoneButton), new[] { ButtonOpenRight }, null, new[] { Button.StylePseudoClassPressed }),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, monotoneFilledButtonOpenRight),
                    }),

                new StyleRule(
                    new SelectorElement(typeof(MonotoneButton), new[] { ButtonOpenBoth }, null, new[] { Button.StylePseudoClassPressed }),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, monotoneFilledButtonOpenBoth),
                    }),

                // NanoHeading

                new StyleRule(
                    new SelectorChild(
                        SelectorElement.Type(typeof(NanoHeading)),
                        SelectorElement.Type(typeof(PanelContainer))),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, nanoHeadingBox),
                    }),

                // StripeBack
                new StyleRule(
                    SelectorElement.Type(typeof(StripeBack)),
                    new[]
                    {
                        new StyleProperty(StripeBack.StylePropertyBackground, stripeBack),
                    }),
                // CCM rework lobby - start
                new StyleRule(SelectorElement.Type(typeof(VScrollBar)), new[]
                {
                    new StyleProperty(ScrollBar.StylePropertyGrabber, scrollBarNormal),
                }),
                new StyleRule(new SelectorElement(typeof(VScrollBar), null, null, new[] { ScrollBar.StylePseudoClassHover }), new[]
                {
                    new StyleProperty(ScrollBar.StylePropertyGrabber, scrollBarHovered),
                }),
                new StyleRule(new SelectorElement(typeof(VScrollBar), null, null, new[] { ScrollBar.StylePseudoClassGrabbed }), new[]
                {
                    new StyleProperty(ScrollBar.StylePropertyGrabber, scrollBarGrabbed),
                }),
                new StyleRule(SelectorElement.Type(typeof(HScrollBar)), new[]
                {
                    new StyleProperty(ScrollBar.StylePropertyGrabber, scrollBarNormal),
                }),
                new StyleRule(new SelectorElement(typeof(HScrollBar), null, null, new[] { ScrollBar.StylePseudoClassHover }), new[]
                {
                    new StyleProperty(ScrollBar.StylePropertyGrabber, scrollBarHovered),
                }),
                new StyleRule(new SelectorElement(typeof(HScrollBar), null, null, new[] { ScrollBar.StylePseudoClassGrabbed }), new[]
                {
                    new StyleProperty(ScrollBar.StylePropertyGrabber, scrollBarGrabbed),
                }),
                // CCM rework lobby - end

                // StyleClassItemStatus
                new StyleRule(SelectorElement.Class(StyleClassItemStatus), new[]
                {
                    new StyleProperty("font", notoSans10),
                }),

                Element()
                    .Class(StyleClassItemStatusNotHeld)
                    .Prop("font", notoSansItalic10)
                    .Prop("font-color", ItemStatusNotHeldColor),

                Element<RichTextLabel>()
                    .Class(StyleClassItemStatus)
                    .Prop(nameof(RichTextLabel.LineHeightScale), 0.7f)
                    .Prop(nameof(Control.Margin), new Thickness(0, 0, 0, -6)),

                // Slider
                new StyleRule(SelectorElement.Type(typeof(Slider)), new []
                {
                    new StyleProperty(Slider.StylePropertyBackground, sliderBackBox),
                    new StyleProperty(Slider.StylePropertyForeground, sliderForeBox),
                    new StyleProperty(Slider.StylePropertyGrabber, sliderGrabBox),
                    new StyleProperty(Slider.StylePropertyFill, sliderFillBox),
                }),

                new StyleRule(SelectorElement.Type(typeof(ColorableSlider)), new []
                {
                    new StyleProperty(ColorableSlider.StylePropertyFillWhite, sliderFillWhite),
                    new StyleProperty(ColorableSlider.StylePropertyBackgroundWhite, sliderFillWhite),
                }),

                new StyleRule(new SelectorElement(typeof(Slider), new []{StyleClassSliderRed}, null, null), new []
                {
                    new StyleProperty(Slider.StylePropertyFill, sliderFillRed),
                }),

                new StyleRule(new SelectorElement(typeof(Slider), new []{StyleClassSliderGreen}, null, null), new []
                {
                    new StyleProperty(Slider.StylePropertyFill, sliderFillGreen),
                }),

                new StyleRule(new SelectorElement(typeof(Slider), new []{StyleClassSliderBlue}, null, null), new []
                {
                    new StyleProperty(Slider.StylePropertyFill, sliderFillBlue),
                }),

                new StyleRule(new SelectorElement(typeof(Slider), new []{StyleClassSliderWhite}, null, null), new []
                {
                    new StyleProperty(Slider.StylePropertyFill, sliderFillWhite),
                }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Slider), null, null, null)), new []
                {
                    new StyleProperty(Slider.StylePropertyBackground, optionsSliderBack),
                    new StyleProperty(Slider.StylePropertyForeground, optionsSliderFore),
                    new StyleProperty(Slider.StylePropertyGrabber, optionsSliderGrab),
                    new StyleProperty(Slider.StylePropertyFill, optionsSliderFill),
                }),

                // chat channel option selector
                new StyleRule(new SelectorElement(typeof(Button), new[] {StyleClassChatChannelSelectorButton}, null, null), new[]
                {
                    new StyleProperty(Button.StylePropertyStyleBox, chatChannelButton),
                }),

                // chat filter button
                new StyleRule(new SelectorElement(typeof(ContainerButton), new[] {StyleClassChatFilterOptionButton}, null, null), new[]
                {
                    new StyleProperty(ContainerButton.StylePropertyStyleBox, chatFilterButton),
                }),
                new StyleRule(new SelectorElement(typeof(ContainerButton), new[] {StyleClassChatFilterOptionButton}, null, new[] {ContainerButton.StylePseudoClassNormal}), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf, ButtonColorDefault),
                }),
                new StyleRule(new SelectorElement(typeof(ContainerButton), new[] {StyleClassChatFilterOptionButton}, null, new[] {ContainerButton.StylePseudoClassHover}), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf, ButtonColorHovered),
                }),
                new StyleRule(new SelectorElement(typeof(ContainerButton), new[] {StyleClassChatFilterOptionButton}, null, new[] {ContainerButton.StylePseudoClassPressed}), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf, ButtonColorPressed),
                }),
                new StyleRule(new SelectorElement(typeof(ContainerButton), new[] {StyleClassChatFilterOptionButton}, null, new[] {ContainerButton.StylePseudoClassDisabled}), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf, ButtonColorDisabled),
                }),

                // output panel scroll button
                Element<Button>()
                    .Class(OutputPanel.StyleClassOutputPanelScrollDownButton)
                    .Prop(Button.StylePropertyStyleBox, outputPanelScrollDownButton),

                // OptionButton
                new StyleRule(new SelectorElement(typeof(OptionButton), null, null, null), new[]
                {
                    new StyleProperty(ContainerButton.StylePropertyStyleBox, BaseButton),
                }),
                new StyleRule(new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassNormal}), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf,
                        CurrentTheme == UiColorTheme.Blue ? ButtonColorDefault : Color.FromHex("#15A31E")),
                }),
                new StyleRule(new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassHover}), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf,
                        CurrentTheme == UiColorTheme.Blue ? ButtonColorHovered : Color.FromHex("#138A1A")),
                }),
                new StyleRule(new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassPressed}), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf,
                        CurrentTheme == UiColorTheme.Blue ? ButtonColorPressed : Color.FromHex("#0F7116")),
                }),
                new StyleRule(new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassDisabled}), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf, ButtonColorDisabled),
                }),

                new StyleRule(new SelectorElement(typeof(TextureRect), new[] {OptionButton.StyleClassOptionTriangle}, null, null), new[]
                {
                    new StyleProperty(TextureRect.StylePropertyTexture, textureInvertedTriangle),
                    //new StyleProperty(Control.StylePropertyModulateSelf, Color.FromHex("#FFFFFF")),
                }),

                new StyleRule(new SelectorElement(typeof(Label), new[] { OptionButton.StyleClassOptionButton }, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                }),

                Element<PanelContainer>().Class(OptionButton.StyleClassOptionsBackground)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat(PanelDark.WithAlpha(1f))),

                new StyleRule(new SelectorElement(typeof(PanelContainer), new []{ ClassHighDivider}, null, null), new []
                {
                    new StyleProperty(PanelContainer.StylePropertyPanel, new StyleBoxFlat { BackgroundColor = NanoGold, ContentMarginBottomOverride = 2, ContentMarginLeftOverride = 2}),
                }),

                Element<TextureButton>()
                    .Class(StyleClassButtonHelp)
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/VerbIcons/information.svg.192dpi.png")),

                // Labels ---
                Element<Label>().Class(StyleClassLabelBig)
                    .Prop(Label.StylePropertyFont, notoSans16),

                Element<Label>().Class(StyleClassLabelSmall)
                 .Prop(Label.StylePropertyFont, notoSans10),
                // ---

                // Different Background shapes ---
                Element<PanelContainer>().Class(ClassAngleRect)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = PanelDark.WithAlpha(0.95f),
                        BorderThickness = new Thickness(1),
                        BorderColor = PanelDark.WithAlpha(1f),
                    }),

                // CCM rework lobby - start
                Element<PanelContainer>().Class("FancyWindowFrame")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = Color.Transparent,
                        BorderThickness = new Thickness(1),
                        BorderColor = PanelDark.WithAlpha(1f),
                    }),

                Element<PanelContainer>().Class("FancyWindowBodyBackground")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = PanelDark.WithAlpha(0.95f),
                    }),
                // CCM rework lobby - end

                // CCM rework lobby - start
                Element<PanelContainer>().Class("CMWindowFrame")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = CurrentTheme == UiColorTheme.Blue
                            ? Color.FromHex("#0A1A33").WithAlpha(0.9f)
                            : Color.FromHex("#06130B").WithAlpha(0.9f),
                        BorderThickness = new Thickness(1),
                        BorderColor = CurrentTheme == UiColorTheme.Blue
                            ? Color.FromHex("#123D72").WithAlpha(0.9f)
                            : Color.FromHex("#1E3A28").WithAlpha(0.9f),
                    }),

                Element<PanelContainer>().Class("CMWindowBodyBackground")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = CurrentTheme == UiColorTheme.Blue
                            ? Color.FromHex("#0B1E3A").WithAlpha(0.85f)
                            : Color.FromHex("#001304").WithAlpha(0.85f),
                    }),
                // CCM rework lobby - end

                Element<PanelContainer>().Class("BackgroundOpenRight")
                    .Prop(PanelContainer.StylePropertyPanel, BaseButtonOpenRight)
                    .Prop(Control.StylePropertyModulateSelf, PanelDark.WithAlpha(0.95f)),

                Element<PanelContainer>().Class("BackgroundOpenLeft")
                    .Prop(PanelContainer.StylePropertyPanel, BaseButtonOpenLeft)
                    .Prop(Control.StylePropertyModulateSelf, PanelDark.WithAlpha(0.95f)),
                // ---

                // Dividers
                Element<PanelContainer>().Class(ClassLowDivider)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = Color.FromHex("#1F3527"),
                        ContentMarginLeftOverride = 2,
                        ContentMarginBottomOverride = 2
                    }),

                // Window Headers
                Element<Label>().Class("FancyWindowTitle")
                    .Prop("font", boxFont13)
                    .Prop("font-color", Color.Transparent),

                Element<PanelContainer>().Class("WindowHeadingBackground")
                    .Prop("panel", new StyleBoxFlat
                    {
                        BackgroundColor = Color.FromHex("#393940").WithAlpha(0.90f),
                    }),

                Element<PanelContainer>().Class("WindowHeadingBackgroundLight")
                    .Prop("panel", new StyleBoxFlat
                    {
                        BackgroundColor = Color.FromHex("#393940").WithAlpha(0.70f),
                    }),

                // CCM rework lobby - start
                Element<TextureButton>().Class("windowCloseButton")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Nano/cross.svg.png"))
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#C1C1C1")),

                Element<TextureButton>().Class("windowCloseButton").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#C1C1C1").WithAlpha(0.6f)),

                Element<TextureButton>().Class("windowCloseButton").Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#C1C1C1")),
                // CCM rework lobby - end

                // Window Header Help Button
                Element<TextureButton>().Class(FancyWindow.StyleClassWindowHelpButton)
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Nano/help.png"))
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#2D5A3A")),

                Element<TextureButton>().Class(FancyWindow.StyleClassWindowHelpButton).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#1F4A31")),

                Element<TextureButton>().Class(FancyWindow.StyleClassWindowHelpButton).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#1B402B")),

                //The lengths you have to go through to change a background color smh
                Element<PanelContainer>().Class("PanelBackgroundBaseDark")
                    .Prop("panel", new StyleBoxTexture(BaseButtonOpenBoth) { Padding = default })
                    .Prop(Control.StylePropertyModulateSelf, PanelDark.WithAlpha(0.95f)),

                Element<PanelContainer>().Class("PanelBackgroundLight")
                    .Prop("panel", new StyleBoxTexture(BaseButtonOpenBoth) { Padding = default })
                    .Prop(Control.StylePropertyModulateSelf, PanelDark.WithAlpha(0.9f)),

                // Window Footer
                Element<TextureRect>().Class("NTLogoDark")
                    .Prop(TextureRect.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Nano/ntlogo.svg.png"))
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#3A6B47")),

                Element<Label>().Class("WindowFooterText")
                    .Prop(Label.StylePropertyFont, notoSans8)
                    .Prop(Label.StylePropertyFontColor, Color.FromHex("#3A6B47")),

                // X Texture button ---
                Element<TextureButton>().Class("CrossButtonRed")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Nano/cross.svg.png"))
                    .Prop(Control.StylePropertyModulateSelf, DangerousRedFore),

                Element<TextureButton>().Class("CrossButtonRed").Pseudo(TextureButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#1F4A31")),

                Element<TextureButton>().Class("CrossButtonRed").Pseudo(TextureButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#1B402B")),

                //
                Element<TextureButton>().Class("Refresh")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Nano/circular_arrow.svg.96dpi.png")),
                // ---

                // Profile Editor
                Element<TextureButton>().Class("SpeciesInfoDefault")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/VerbIcons/information.svg.192dpi.png")),

                Element<TextureButton>().Class("SpeciesInfoWarning")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/info.svg.192dpi.png"))
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#6CFF6C")),

                // The default look of paper in UIs. Pages can have components which override this
                Element<PanelContainer>().Class("PaperDefaultBorder")
                    .Prop(PanelContainer.StylePropertyPanel, paperBackground),
                Element<RichTextLabel>().Class("PaperWrittenText")
                    .Prop(Label.StylePropertyFont, notoSans12)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#0B140E")),

                Element<RichTextLabel>().Class("LabelSubText")
                    .Prop(Label.StylePropertyFont, notoSans10)
                    .Prop(Label.StylePropertyFontColor, Color.FromHex("#3A6B47")),

                Element<LineEdit>().Class("PaperLineEdit")
                    .Prop(LineEdit.StylePropertyStyleBox, new StyleBoxEmpty()),

                // Red Button ---
                Element<Button>().Class("ButtonColorRed")
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorDefaultRed),

                Element<Button>().Class("ButtonColorRed").Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorDefaultRed),

                Element<Button>().Class("ButtonColorRed").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorHoveredRed),
                // ---

                // Green Button ---
                Element<Button>().Class("ButtonColorGreen")
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorGoodDefault),

                Element<Button>().Class("ButtonColorGreen").Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorGoodDefault),

                Element<Button>().Class("ButtonColorGreen").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorGoodHovered),

                // Accept button (merge with green button?) ---
                Element<Button>().Class("ButtonAccept")
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorGoodDefault),

                Element<Button>().Class("ButtonAccept").Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorGoodDefault),

                Element<Button>().Class("ButtonAccept").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorGoodHovered),

                Element<Button>().Class("ButtonAccept").Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorGoodDisabled),

                // ---

                // Character setup top action buttons
                Element<Button>().Class("CharacterSetupActionButton")
                    .Prop(Control.StylePropertyModulateSelf, CurrentTheme == UiColorTheme.Blue
                        ? Color.FromHex("#123667")
                        : Color.FromHex("#0F6B24")),

                Element<Button>().Class("CharacterSetupActionButton").Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, CurrentTheme == UiColorTheme.Blue
                        ? Color.FromHex("#123667")
                        : Color.FromHex("#0F6B24")),

                Element<Button>().Class("CharacterSetupActionButton").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, CurrentTheme == UiColorTheme.Blue
                        ? Color.FromHex("#194583")
                        : Color.FromHex("#14842D")),

                Element<Button>().Class("CharacterSetupActionButton").Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, CurrentTheme == UiColorTheme.Blue
                        ? Color.FromHex("#0E274D")
                        : Color.FromHex("#0B531B")),

                Element<Button>().Class("CharacterSetupActionButton").Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorDisabled),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {"CharacterSetupActionButton"}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                        new StyleProperty(Label.StylePropertyFontColor, CurrentTheme == UiColorTheme.Blue
                            ? Color.FromHex("#86BBF2")
                            : Color.FromHex("#9DFFB2")),
                    }),

                Element<Label>().Class("CharacterCarouselName")
                    .Prop(Label.StylePropertyFont, notoSansBold12),

                Element<Label>().Class("CharacterCarouselNameSelected")
                    .Prop(Label.StylePropertyFont, notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, CurrentTheme == UiColorTheme.Blue
                        ? Color.FromHex("#9FC8F2")
                        : Color.FromHex("#BFFFD0")),

                Element<Label>().Class("CharacterEditorSectionTitle")
                    .Prop(Label.StylePropertyFont, notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, CurrentTheme == UiColorTheme.Blue
                        ? Color.FromHex("#89B8E8")
                        : Color.FromHex("#AFFFBD")),

                // ---

                // Small Button ---
                Element<Button>().Class("ButtonSmall")
                    .Prop(ContainerButton.StylePropertyStyleBox, smallButtonBase),

                Child().Parent(Element<Button>().Class("ButtonSmall"))
                    .Child(Element<Label>())
                    .Prop(Label.StylePropertyFont, notoSans8),
                // ---

                Element<Label>().Class("StatusFieldTitle")
                    .Prop("font-color", NanoGold),

                Element<Label>().Class("Good")
                    .Prop("font-color", GoodGreenFore),

                Element<Label>().Class("Caution")
                    .Prop("font-color", ConcerningOrangeFore),

                Element<Label>().Class("Danger")
                    .Prop("font-color", DangerousRedFore),

                Element<Label>().Class("Disabled")
                    .Prop("font-color", DisabledFore),

                // Radial menu buttons
                Element<TextureButton>().Class("RadialMenuButton")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Radial/button_normal.png")),
                Element<TextureButton>().Class("RadialMenuButton")
                    .Pseudo(TextureButton.StylePseudoClassHover)
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Radial/button_hover.png")),

                Element<TextureButton>().Class("RadialMenuCloseButton")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Radial/close_normal.png")),
                Element<TextureButton>().Class("RadialMenuCloseButton")
                    .Pseudo(TextureButton.StylePseudoClassHover)
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Radial/close_hover.png")),

                Element<TextureButton>().Class("RadialMenuBackButton")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Radial/back_normal.png")),
                Element<TextureButton>().Class("RadialMenuBackButton")
                    .Pseudo(TextureButton.StylePseudoClassHover)
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Radial/back_hover.png")),

                //PDA - Backgrounds
                Element<PanelContainer>().Class("PdaContentBackground")
                    .Prop(PanelContainer.StylePropertyPanel, BaseButtonOpenBoth)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#0F1B13")),

                Element<PanelContainer>().Class("PdaBackground")
                    .Prop(PanelContainer.StylePropertyPanel, BaseButtonOpenBoth)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#000000")),

                Element<PanelContainer>().Class("PdaBackgroundRect")
                    .Prop(PanelContainer.StylePropertyPanel, BaseAngleRect)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#254B34")),

                Element<PanelContainer>().Class("PdaBorderRect")
                    .Prop(PanelContainer.StylePropertyPanel, AngleBorderRect),

                Element<PanelContainer>().Class("BackgroundDark")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat(PanelDark.WithAlpha(0.95f))),

                Element<PanelContainer>().Class("VerticalTabListBackground")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = CurrentTheme == UiColorTheme.Blue
                            ? Color.FromHex("#08264F").WithAlpha(0.85f)
                            : PanelDark.WithAlpha(0.75f),
                        BorderThickness = new Thickness(2, 0, 0, 0),
                        BorderColor = LobbyMenuButtonBase.WithAlpha(0.95f),
                    }),

                Element<PanelContainer>().Class("VerticalTabContentBackground")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat(PanelDark.WithAlpha(0.9f))),

                Element<Button>().Class(StyleBase.StyleClassVerticalTabButton)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = CurrentTheme == UiColorTheme.Blue
                            ? Color.FromHex("#123D72").WithAlpha(0.9f)
                            : PanelDark.WithAlpha(0.9f),
                        BorderThickness = new Thickness(1),
                        BorderColor = PanelDark.WithAlpha(1f),
                    }),

                Element<Button>().Class(StyleBase.StyleClassVerticalTabButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = CurrentTheme == UiColorTheme.Blue
                            ? Color.FromHex("#1D4AA1").WithAlpha(0.8f)
                            : LobbyMenuButtonBase.WithAlpha(0.7f),
                        BorderThickness = new Thickness(1),
                        BorderColor = CurrentTheme == UiColorTheme.Blue
                            ? Color.FromHex("#1D4AA1").WithAlpha(0.9f)
                            : LobbyMenuButtonBase.WithAlpha(0.8f),
                    }),

                Element<Button>().Class(StyleBase.StyleClassVerticalTabButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = LobbyMenuButtonPressed.WithAlpha(0.7f),
                        BorderThickness = new Thickness(1),
                        BorderColor = LobbyMenuButtonPressed.WithAlpha(0.9f),
                    }),


                //PDA - Buttons
                Element<PdaSettingsButton>().Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.NormalBgColor))
                    .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.EnabledFgColor)),

                Element<PdaSettingsButton>().Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.HoverColor))
                    .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.EnabledFgColor)),

                Element<PdaSettingsButton>().Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.PressedColor))
                    .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.EnabledFgColor)),

                Element<PdaSettingsButton>().Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.NormalBgColor))
                    .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.DisabledFgColor)),

                Element<PdaProgramItem>().Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(PdaProgramItem.StylePropertyBgColor, Color.FromHex(PdaProgramItem.NormalBgColor)),

                Element<PdaProgramItem>().Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(PdaProgramItem.StylePropertyBgColor, Color.FromHex(PdaProgramItem.HoverColor)),

                Element<PdaProgramItem>().Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(PdaProgramItem.StylePropertyBgColor, Color.FromHex(PdaProgramItem.HoverColor)),

                //PDA - Text
                Element<Label>().Class("PdaContentFooterText")
                    .Prop(Label.StylePropertyFont, notoSans10)
                    .Prop(Label.StylePropertyFontColor, Color.FromHex("#3A6B47")),

                Element<Label>().Class("PdaWindowFooterText")
                    .Prop(Label.StylePropertyFont, notoSans10)
                    .Prop(Label.StylePropertyFontColor, Color.FromHex("#234837")),

                // CCM rework lobby - start
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassCMProfileFont}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, bedstead15),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassCMProfileFont}, null, null),
                    new SelectorElement(typeof(RichTextLabel), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, bedstead15),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassCMProfileFont}, null, null),
                    new SelectorElement(typeof(LineEdit), null, null, null)),
                    new[]
                    {
                        new StyleProperty("font", bedstead15),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassCMProfileFont}, null, null),
                    new SelectorElement(typeof(Content.Client._CCM.UserInterface.Controls.CenteredTabContainer), null, null, null)),
                    new[]
                    {
                        new StyleProperty("font", bedstead15),
                        new StyleProperty(TabContainer.stylePropertyTabFontColor, CurrentTheme == UiColorTheme.Blue
                            ? Color.FromHex("#8FC4F6")
                            : Color.FromHex("#B7FFC8")),
                        new StyleProperty(TabContainer.StylePropertyTabFontColorInactive, CurrentTheme == UiColorTheme.Blue
                            ? Color.FromHex("#679CCB")
                            : Color.FromHex("#94D5A3")),
                        new StyleProperty(TabContainer.StylePropertyTabStyleBox, new StyleBoxFlat
                        {
                            BackgroundColor = CurrentTheme == UiColorTheme.Blue
                                ? Color.FromHex("#0F2A52").WithAlpha(0.92f)
                                : Color.FromHex("#0A2C18").WithAlpha(0.9f),
                            BorderColor = Color.Transparent,
                            BorderThickness = new Thickness(0f),
                            ContentMarginLeftOverride = 10f,
                            ContentMarginRightOverride = 10f,
                            ContentMarginTopOverride = 3f,
                            ContentMarginBottomOverride = 3f
                        }),
                        new StyleProperty(TabContainer.StylePropertyTabStyleBoxInactive, new StyleBoxFlat
                        {
                            BackgroundColor = CurrentTheme == UiColorTheme.Blue
                                ? Color.FromHex("#0E2548").WithAlpha(0.86f)
                                : Color.FromHex("#0A2C18").WithAlpha(0.82f),
                            BorderColor = Color.Transparent,
                            BorderThickness = new Thickness(0f),
                            ContentMarginLeftOverride = 10f,
                            ContentMarginRightOverride = 10f,
                            ContentMarginTopOverride = 3f,
                            ContentMarginBottomOverride = 3f
                        }),
                    }),
                // CCM rework lobby - end

                // Fancy Tree
                Element<ContainerButton>().Identifier(TreeItem.StyleIdentifierTreeButton)
                    .Class(TreeItem.StyleClassEvenRow)
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = FancyTreeEvenRowColor,
                    }),

                Element<ContainerButton>().Identifier(TreeItem.StyleIdentifierTreeButton)
                    .Class(TreeItem.StyleClassOddRow)
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = FancyTreeOddRowColor,
                    }),

                Element<ContainerButton>().Identifier(TreeItem.StyleIdentifierTreeButton)
                    .Class(TreeItem.StyleClassSelected)
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = FancyTreeSelectedRowColor,
                    }),

                Element<ContainerButton>().Identifier(TreeItem.StyleIdentifierTreeButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = FancyTreeSelectedRowColor,
                    }),

                // Silicon law edit ui
                Element<Label>().Class(SiliconLawContainer.StyleClassSiliconLawPositionLabel)
                    .Prop(Label.StylePropertyFontColor, NanoGold),
                // Pinned button style
                new StyleRule(
                    new SelectorElement(typeof(TextureButton), new[] { StyleClassPinButtonPinned }, null, null),
                    new[]
                    {
                        new StyleProperty(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Bwoink/pinned.png"))
                    }),

                // Unpinned button style
                new StyleRule(
                    new SelectorElement(typeof(TextureButton), new[] { StyleClassPinButtonUnpinned }, null, null),
                    new[]
                    {
                        new StyleProperty(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Bwoink/un_pinned.png"))
                    }),

                Element<PanelContainer>()
                    .Class(StyleClassInset)
                    .Prop(PanelContainer.StylePropertyPanel, insetBack),

                // RMC14
                new StyleRule(new SelectorElement(typeof(Label), new[] { CMStyleClasses.CMLabelAlignLeft }, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Left),
                }),
            }).ToList());
        }
    }
}

// # CCM priority rework


