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
using Content.Shared._RMC14.CCVar;
using Content.Shared.Verbs;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Configuration;
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
            Gray
        }

        public static UiColorTheme CurrentTheme { get; private set; } = UiColorTheme.Gray;
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
        public const string StyleClassLobbyTaskbarLabel = "LobbyTaskbarLabel";
        public const string StyleClassLobbyTaskbarMenuLabel = "LobbyTaskbarMenuLabel";
        public const string StyleClassLobbyTaskbarMenuIcon = "LobbyTaskbarMenuIcon";
        public const string StyleClassLobbyTaskbarLabelSmall = "TaskbarLabelSmall";
        public const string StyleClassLobbyMenuButton = "LobbyMenuButton";
        public const string StyleClassEscapeMenuButton = "EscapeMenuButton";
        public const string StyleClassLobbyReadyButton = "LobbyReadyButton";
        public const string StyleClassLobbyMenuDivider = "LobbyMenuDivider";
        public const string StyleClassLobbyMenuIconButton = "LobbyMenuIconButton";
        public const string StyleClassLobbyTopButton = "LobbyTopButton";
        public const string StyleClassLobbyDiscordLinkButton = "LobbyDiscordLinkButton";
        public const string StyleClassLobbyDiscordLinkWarningPanel = "LobbyDiscordLinkWarningPanel";
        public const string StyleClassLobbyDiscordLinkWarningText = "LobbyDiscordLinkWarningText";
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
        public const string StyleClassTacticalMapSlider = "TacticalMapSlider";

        public const string StyleClassLabelHeadingBigger = "LabelHeadingBigger";
        public const string StyleClassLabelKeyText = "LabelKeyText";
        public const string StyleClassLabelSecondaryColor = "LabelSecondaryColor";
        public const string StyleClassLabelBig = "LabelBig";
        public const string StyleClassLabelSmall = "LabelSmall";
        public const string StyleClassLoadoutNamePanel = "LoadoutNamePanel";
        public const string StyleClassLoadoutSectionPanel = "LoadoutSectionPanel";
        public const string StyleClassLoadoutItemButton = "LoadoutItemButton";
        public const string StyleClassLoadoutItemButtonAlt = "LoadoutItemButtonAlt";
        public const string StyleClassLoadoutSpriteFrame = "LoadoutSpriteFrame";
        public const string StyleClassLoadoutSubgroupPanel = "LoadoutSubgroupPanel";
        public const string StyleClassLoadoutToggleButton = "LoadoutToggleButton";
        public const string StyleClassGuidebookTreePanel = "GuidebookTreePanel";
        public const string StyleClassGuidebookContentPanel = "GuidebookContentPanel";
        public const string StyleClassGuidebookSearchPanel = "GuidebookSearchPanel";
        public const string StyleClassGuidebookSearchBar = "GuidebookSearchBar";
        public const string StyleClassGuidebookPlaceholderPanel = "GuidebookPlaceholderPanel";
        public const string StyleClassGuidebookEmbedCard = "GuidebookEmbedCard";
        public const string StyleClassGuidebookEmbedHeader = "GuidebookEmbedHeader";
        public const string StyleClassGuidebookEmbedSectionHeading = "GuidebookEmbedSectionHeading";
        public const string StyleClassGuidebookEmbedSectionBody = "GuidebookEmbedSectionBody";
        public const string StyleClassVotePanel = "VotePanel";
        public const string StyleClassVoteSectionPanel = "VoteSectionPanel";
        public const string StyleClassVoteActionButton = "VoteActionButton";
        public const string StyleClassVoteCreateButton = "VoteCreateButton";
        public const string StyleClassVoteProgressBar = "VoteProgressBar";
        public const string StyleClassVoteTimerText = "VoteTimerText";
        public const string StyleClassOptionsFooterPanel = "OptionsFooterPanel";
        public const string StyleClassOptionsFooterButton = "OptionsFooterButton";
        public const string StyleClassCMProfileFont = "CMProfileFont";
        public const string StyleClassButtonBig = "ButtonBig";
        public const string StyleClassOldLobbyButton = "OldLobbyButton";
        public const string StyleClassOldLobbyButtonRed = "OldLobbyButtonRed";
        public const string StyleClassOldLobbyAngleRect = "OldLobbyAngleRect";
        public const string StyleClassOldLobbyStripeBack = "OldLobbyStripeBack";
        public const string StyleClassOldLobbyHeading = "OldLobbyHeading";
        public const string StyleClassOldLobbyTitle = "OldLobbyTitle";
        public const string StyleClassOldLobbyMutedText = "OldLobbyMutedText";
        public const string StyleClassOldLobbyCenteredText = "OldLobbyCenteredText";
        public const string StyleClassOldLobbyGoldDivider = "OldLobbyGoldDivider";

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

        public static Color ButtonColorDefault = Color.FromHex("#023106");
        public static Color ButtonColorDefaultRed = Color.FromHex("#023106");
        public static Color ButtonColorHovered = Color.FromHex("#044B0B");
        public static Color ButtonColorHoveredRed = Color.FromHex("#044B0B");
        public static Color ButtonColorPressed = Color.FromHex("#033B08");
        public static Color ButtonColorDisabled = Color.FromHex("#012205"); // CCM 10 > 15: lobby rework

        public static Color ButtonColorCautionDefault = Color.FromHex("#023106");
        public static Color ButtonColorCautionHovered = Color.FromHex("#044B0B");
        public static Color ButtonColorCautionPressed = Color.FromHex("#033B08");
        public static Color ButtonColorCautionDisabled = Color.FromHex("#012205");

        public static Color ButtonColorGoodDefault = Color.FromHex("#023106");
        public static Color ButtonColorGoodHovered = Color.FromHex("#044B0B");
        public static Color ButtonColorGoodDisabled = Color.FromHex("#012205");

        public static Color LobbyCrtAccent = Color.FromHex("#0EE13E");
        public static Color LobbyCrtText = Color.FromHex("#0EE13E");
        public static Color LobbyCrtMutedText = Color.FromHex("#A0C7B1");
        public static Color LobbyCleanAccent = Color.FromHex("#2F6B46");
        public static Color LobbyCleanText = Color.FromHex("#D5FFE0");
        public static Color LobbyCleanMutedText = Color.FromHex("#9BC9A8");
        public static Color LobbyCrtGlow = Color.FromHex("#0EE13EAA");
        public static Color LobbyMenuButtonBase = Color.FromHex("#0CD137");
        public static Color LobbyMenuButtonPressed = Color.FromHex("#0AA92C");
        public static Color LobbyMenuButtonReadyPressed = Color.FromHex("#0AA92C");
        public static Color LobbyMenuButtonDisabledCrt = Color.FromHex("#088F25");
        public static Color LobbyMenuButtonDisabledClean = Color.FromHex("#097425");
        public static Color UiButtonBorder = Color.FromHex("#022606");

        //NavMap
        public static Color PointRed = Color.FromHex("#2F6A3B");
        public static Color PointGreen = Color.FromHex("#38b026");
        public static Color PointMagenta = Color.FromHex("#00EB4C");

        // Context menu button colors
        public static Color ButtonColorContext = Color.FromHex("#023106");
        public static Color ButtonColorContextHover = Color.FromHex("#044B0B");
        public static Color ButtonColorContextPressed = Color.FromHex("#033B08");
        public static Color ButtonColorContextDisabled = Color.FromHex("#012205");
        public static Color DropdownButtonColorContext = Color.FromHex("#023106");
        public static Color DropdownButtonColorContextHover = Color.FromHex("#044B0B");
        public static Color DropdownButtonColorContextPressed = Color.FromHex("#033B08");
        public static Color DropdownButtonColorContextDisabled = Color.FromHex("#012205");

        // Examine button colors
        public static Color ExamineButtonColorContext = Color.FromHex("#023106");
        public static Color ExamineButtonColorContextHover = Color.FromHex("#044B0B");
        public static Color ExamineButtonColorContextPressed = Color.FromHex("#033B08");
        public static Color ExamineButtonColorContextDisabled = Color.FromHex("#012205");

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
        public static readonly Color OldLobbyGold = Color.FromHex("#B69A68");
        public static readonly Color OldLobbyPanel = Color.FromHex("#293147");
        public static readonly Color OldLobbyPanelSoft = Color.FromHex("#36415D");
        public static readonly Color OldLobbyButton = Color.FromHex("#52637F");
        public static readonly Color OldLobbyButtonHover = Color.FromHex("#667A9C");
        public static readonly Color OldLobbyButtonPressed = Color.FromHex("#485A76");
        public static readonly Color OldLobbyButtonDisabled = Color.FromHex("#404B63");
        public static readonly Color OldLobbyButtonBorder = Color.FromHex("#7B91BA");
        public static readonly Color OldLobbyButtonBorderHover = Color.FromHex("#95ACD5");
        public static readonly Color OldLobbyButtonBorderPressed = Color.FromHex("#6C81A8");
        public static readonly Color OldLobbyButtonBorderDisabled = Color.FromHex("#5C6D8C");
        public static readonly Color OldLobbyButtonRed = Color.FromHex("#A55252");
        public static readonly Color OldLobbyButtonRedHover = Color.FromHex("#BE6464");
        public static readonly Color OldLobbyButtonRedBorder = Color.FromHex("#C27575");
        public static readonly Color OldLobbyButtonRedBorderHover = Color.FromHex("#D68989");
        public static readonly Color OldLobbyText = Color.FromHex("#ECE4D4");
        public static readonly Color OldLobbyMuted = Color.FromHex("#C0B59F");

        //Bwoink
        public const string StyleClassPinButtonPinned = "pinButtonPinned";
        public const string StyleClassPinButtonUnpinned = "pinButtonUnpinned";

        private static UiColorTheme ParseTheme(string theme)
        {
            if (theme.Equals("blue", StringComparison.OrdinalIgnoreCase))
                return UiColorTheme.Gray;

            if (theme.Equals("gray", StringComparison.OrdinalIgnoreCase))
                return UiColorTheme.Gray;

            if (theme.Equals("green", StringComparison.OrdinalIgnoreCase))
                return UiColorTheme.Green;

            return UiColorTheme.Gray;
        }

        public static bool IsOldLobbyStyle(IConfigurationManager config)
        {
            return string.Equals(
                config.GetCVar(RMCCVars.RMCLobbyUiStyle),
                "old",
                StringComparison.OrdinalIgnoreCase);
        }

        public static UiColorTheme GetConfiguredTheme(IConfigurationManager config)
        {
            if (IsOldLobbyStyle(config))
                return UiColorTheme.Gray;

            var theme = config.GetCVar(RMCCVars.RMCUIColorTheme) ?? "gray";
            return ParseTheme(theme);
        }

        private static T ThemeValue<T>(T removedBlue, T gray, T green)
        {
            return CurrentTheme switch
            {
                UiColorTheme.Gray => gray,
                _ => green,
            };
        }

        private static void ApplyPalette(UiColorTheme theme)
        {
            CurrentTheme = theme;
            if (theme == UiColorTheme.Gray)
            {
                PanelDark = Color.FromHex("#15171B");
                NanoGold = Color.FromHex("#D9DCE1");
                GoodGreenFore = Color.FromHex("#CDD1D6");
                ConcerningOrangeFore = Color.FromHex("#E0E2E5");
                DangerousRedFore = Color.FromHex("#C3C7CD");
                DisabledFore = Color.FromHex("#737780");

                ButtonColorDefault = Color.FromHex("#686F80");
                ButtonColorDefaultRed = Color.FromHex("#686F80");
                ButtonColorHovered = Color.FromHex("#788194");
                ButtonColorHoveredRed = Color.FromHex("#788194");
                ButtonColorPressed = Color.FromHex("#565D6C");
                ButtonColorDisabled = Color.FromHex("#454B57");

                ButtonColorCautionDefault = Color.FromHex("#686F80");
                ButtonColorCautionHovered = Color.FromHex("#788194");
                ButtonColorCautionPressed = Color.FromHex("#565D6C");
                ButtonColorCautionDisabled = Color.FromHex("#454B57");

                ButtonColorGoodDefault = Color.FromHex("#686F80");
                ButtonColorGoodHovered = Color.FromHex("#788194");
                ButtonColorGoodDisabled = Color.FromHex("#454B57");

                LobbyCrtAccent = Color.FromHex("#BFC3C8");
                LobbyCrtText = Color.FromHex("#BFC3C8");
                LobbyCrtMutedText = Color.FromHex("#AAADB2");
                LobbyCleanAccent = Color.FromHex("#747C8D");
                LobbyCleanText = Color.FromHex("#F0F1F3");
                LobbyCleanMutedText = Color.FromHex("#C7CACF");
                LobbyCrtGlow = Color.FromHex("#BFC3C888");
                LobbyMenuButtonBase = Color.FromHex("#686F80");
                LobbyMenuButtonPressed = Color.FromHex("#565D6C");
                LobbyMenuButtonReadyPressed = Color.FromHex("#565D6C");
                LobbyMenuButtonDisabledCrt = Color.FromHex("#454B57");
                LobbyMenuButtonDisabledClean = Color.FromHex("#454B57");
                UiButtonBorder = Color.FromHex("#565E6F");

                PointRed = Color.FromHex("#686F80");
                PointGreen = Color.FromHex("#9AA1AD");
                PointMagenta = Color.FromHex("#C5C8CE");

                ButtonColorContext = Color.FromHex("#686F80");
                ButtonColorContextHover = Color.FromHex("#788194");
                ButtonColorContextPressed = Color.FromHex("#565D6C");
                ButtonColorContextDisabled = Color.FromHex("#454B57");
                DropdownButtonColorContext = Color.FromHex("#686F80");
                DropdownButtonColorContextHover = Color.FromHex("#788194");
                DropdownButtonColorContextPressed = Color.FromHex("#565D6C");
                DropdownButtonColorContextDisabled = Color.FromHex("#454B57");

                ExamineButtonColorContext = Color.FromHex("#686F80");
                ExamineButtonColorContextHover = Color.FromHex("#788194");
                ExamineButtonColorContextPressed = Color.FromHex("#565D6C");
                ExamineButtonColorContextDisabled = Color.FromHex("#454B57");

                FancyTreeEvenRowColor = Color.FromHex("#181A1E");
                FancyTreeSelectedRowColor = Color.FromHex("#22252B");
                ItemStatusNotHeldColor = Color.FromHex("#4A505C");
                ChatBackgroundColor = Color.FromHex("#101114");
            }
            else
            {
                PanelDark = Color.FromHex("#0E1B11");
                NanoGold = Color.FromHex("#D5F0D9");
                GoodGreenFore = Color.FromHex("#C8E7CF");
                ConcerningOrangeFore = Color.FromHex("#E4F5E7");
                DangerousRedFore = Color.FromHex("#B6DCBE");
                DisabledFore = Color.FromHex("#667B6B");

                ButtonColorDefault = Color.FromHex("#2F8F49");
                ButtonColorDefaultRed = Color.FromHex("#2F8F49");
                ButtonColorHovered = Color.FromHex("#3EAE5C");
                ButtonColorHoveredRed = Color.FromHex("#3EAE5C");
                ButtonColorPressed = Color.FromHex("#266E39");
                ButtonColorDisabled = Color.FromHex("#26372B");

                ButtonColorCautionDefault = Color.FromHex("#2F8F49");
                ButtonColorCautionHovered = Color.FromHex("#3EAE5C");
                ButtonColorCautionPressed = Color.FromHex("#266E39");
                ButtonColorCautionDisabled = Color.FromHex("#26372B");

                ButtonColorGoodDefault = Color.FromHex("#2F8F49");
                ButtonColorGoodHovered = Color.FromHex("#3EAE5C");
                ButtonColorGoodDisabled = Color.FromHex("#26372B");

                LobbyCrtAccent = Color.FromHex("#49D26A");
                LobbyCrtText = Color.FromHex("#49D26A");
                LobbyCrtMutedText = Color.FromHex("#B8DEBF");
                LobbyCleanAccent = Color.FromHex("#3DA45A");
                LobbyCleanText = Color.FromHex("#EEF9F0");
                LobbyCleanMutedText = Color.FromHex("#C3E4CB");
                LobbyCrtGlow = Color.FromHex("#49D26A88");
                LobbyMenuButtonBase = Color.FromHex("#43B05F");
                LobbyMenuButtonPressed = Color.FromHex("#327E48");
                LobbyMenuButtonReadyPressed = Color.FromHex("#327E48");
                LobbyMenuButtonDisabledCrt = Color.FromHex("#325D40");
                LobbyMenuButtonDisabledClean = Color.FromHex("#304C39");

                PointRed = Color.FromHex("#4A7A58");
                PointGreen = Color.FromHex("#49B567");
                PointMagenta = Color.FromHex("#A9E0B8");

                ButtonColorContext = Color.FromHex("#2C6E3C");
                ButtonColorContextHover = Color.FromHex("#37884A");
                ButtonColorContextPressed = Color.FromHex("#255C33");
                ButtonColorContextDisabled = Color.FromHex("#2D4133");
                DropdownButtonColorContext = Color.FromHex("#2C6E3C");
                DropdownButtonColorContextHover = Color.FromHex("#37884A");
                DropdownButtonColorContextPressed = Color.FromHex("#255C33");
                DropdownButtonColorContextDisabled = Color.FromHex("#2D4133");

                ExamineButtonColorContext = Color.FromHex("#2C6E3C");
                ExamineButtonColorContextHover = Color.FromHex("#37884A");
                ExamineButtonColorContextPressed = Color.FromHex("#255C33");
                ExamineButtonColorContextDisabled = Color.FromHex("#2D4133");
                UiButtonBorder = Color.FromHex("#2E4A36");

                FancyTreeEvenRowColor = Color.FromHex("#101912");
                FancyTreeSelectedRowColor = Color.FromHex("#16311D");
                ItemStatusNotHeldColor = Color.FromHex("#34513A");
                ChatBackgroundColor = Color.FromHex("#0B120D");
            }

            FancyTreeOddRowColor = FancyTreeEvenRowColor * new Color(0.85f, 0.85f, 0.85f);
        }

        private static void ApplyNeutralPalette()
        {
            CurrentTheme = UiColorTheme.Gray;
            PanelDark = OldLobbyPanel;
            NanoGold = OldLobbyGold;
            GoodGreenFore = Color.FromHex("#31843E");
            ConcerningOrangeFore = Color.FromHex("#A5762F");
            DangerousRedFore = Color.FromHex("#BB3232");
            DisabledFore = Color.FromHex("#5A5A5A");

            ButtonColorDefault = OldLobbyButton;
            ButtonColorDefaultRed = OldLobbyButtonRed;
            ButtonColorHovered = OldLobbyButtonHover;
            ButtonColorHoveredRed = OldLobbyButtonRedHover;
            ButtonColorPressed = OldLobbyButtonPressed;
            ButtonColorDisabled = OldLobbyButtonDisabled;

            ButtonColorCautionDefault = OldLobbyButtonRed;
            ButtonColorCautionHovered = OldLobbyButtonRedHover;
            ButtonColorCautionPressed = Color.FromHex("#742C2C");
            ButtonColorCautionDisabled = Color.FromHex("#402D34");

            ButtonColorGoodDefault = Color.FromHex("#3E6C45");
            ButtonColorGoodHovered = Color.FromHex("#31843E");
            ButtonColorGoodDisabled = Color.FromHex("#164420");

            LobbyCrtAccent = OldLobbyGold;
            LobbyCrtText = OldLobbyGold;
            LobbyCrtMutedText = OldLobbyMuted;
            LobbyCleanAccent = OldLobbyGold.WithAlpha(0.78f);
            LobbyCleanText = OldLobbyText;
            LobbyCleanMutedText = OldLobbyMuted;
            LobbyCrtGlow = OldLobbyGold.WithAlpha(0.55f);
            LobbyMenuButtonBase = OldLobbyButton;
            LobbyMenuButtonPressed = OldLobbyButtonPressed;
            LobbyMenuButtonReadyPressed = Color.FromHex("#314F3A");
            LobbyMenuButtonDisabledCrt = Color.FromHex("#60503A");
            LobbyMenuButtonDisabledClean = OldLobbyButtonDisabled;
            UiButtonBorder = Color.FromHex("#2F313B");

            PointRed = Color.FromHex("#B02E26");
            PointGreen = Color.FromHex("#38B026");
            PointMagenta = Color.FromHex("#FF00FF");

            ButtonColorContext = OldLobbyButton;
            ButtonColorContextHover = OldLobbyButtonHover;
            ButtonColorContextPressed = OldLobbyButtonPressed;
            ButtonColorContextDisabled = OldLobbyButtonDisabled;
            DropdownButtonColorContext = OldLobbyButton;
            DropdownButtonColorContextHover = OldLobbyButtonHover;
            DropdownButtonColorContextPressed = OldLobbyButtonPressed;
            DropdownButtonColorContextDisabled = OldLobbyButtonDisabled;

            ExamineButtonColorContext = Color.Transparent;
            ExamineButtonColorContextHover = Color.DarkSlateGray;
            ExamineButtonColorContextPressed = Color.LightSlateGray;
            ExamineButtonColorContextDisabled = Color.FromHex("#5A5A5A");

            FancyTreeEvenRowColor = OldLobbyPanelSoft;
            FancyTreeSelectedRowColor = OldLobbyButtonHover;
            ItemStatusNotHeldColor = Color.Gray;
            ChatBackgroundColor = Color.FromHex("#131313");
            FancyTreeOddRowColor = FancyTreeEvenRowColor * new Color(0.8f, 0.8f, 0.8f);
        }


        public override Stylesheet Stylesheet { get; }

        public StyleNano(IResourceCache resCache, string theme, bool useNeutralPalette = false, bool useOldLobbyPalette = false) : base(resCache)
        {
            if (useNeutralPalette)
                ApplyNeutralPalette();
            else
                ApplyPalette(ParseTheme(theme));
            static Color BlendTowards(Color source, Color target, float factor)
            {
                factor = Math.Clamp(factor, 0f, 1f);
                return new Color(
                    source.R + (target.R - source.R) * factor,
                    source.G + (target.G - source.G) * factor,
                    source.B + (target.B - source.B) * factor,
                    source.A + (target.A - source.A) * factor);
            }

            var optionsButtonBase = useOldLobbyPalette
                ? OldLobbyButton
                : ThemeValue(
                    ButtonColorDefault,
                    ButtonColorDefault,
                    ButtonColorContext);
            var optionsButtonHover = useOldLobbyPalette
                ? OldLobbyButtonHover
                : ThemeValue(
                    ButtonColorHovered,
                    ButtonColorHovered,
                    ButtonColorContextHover);
            var optionsButtonPressed = useOldLobbyPalette
                ? OldLobbyButtonPressed
                : ThemeValue(
                    ButtonColorPressed,
                    ButtonColorPressed,
                    ButtonColorContextPressed);
            var optionsButtonDisabled = useOldLobbyPalette
                ? OldLobbyButtonDisabled
                : ThemeValue(
                    ButtonColorDisabled,
                    ButtonColorDisabled,
                    ButtonColorContextDisabled);
            var optionsButtonText = useOldLobbyPalette ? OldLobbyText : Color.FromHex("#EEF4FB");
            var optionsButtonTextDisabled = optionsButtonText.WithAlpha(0.72f);
            var optionsOptionsBackground = useOldLobbyPalette
                ? OldLobbyPanelSoft.WithAlpha(0.96f)
                : ThemeValue(
                    Color.FromHex("#1C1E22").WithAlpha(0.95f),
                    Color.FromHex("#1C1E22").WithAlpha(0.95f),
                    optionsButtonBase.WithAlpha(0.96f));
            var optionsOptionsBorder = useOldLobbyPalette
                ? OldLobbyGold.WithAlpha(0.88f)
                : ThemeValue(
                    Color.FromHex("#686D76").WithAlpha(0.88f),
                    Color.FromHex("#686D76").WithAlpha(0.88f),
                    UiButtonBorder.WithAlpha(0.96f));
            var themedPanel = ThemeValue(
                Color.FromHex("#1A1C20").WithAlpha(0.94f),
                Color.FromHex("#1A1C20").WithAlpha(0.94f),
                Color.FromHex("#07170B").WithAlpha(0.94f));
            var themedPanelAlt = ThemeValue(
                Color.FromHex("#24272D").WithAlpha(0.92f),
                Color.FromHex("#24272D").WithAlpha(0.92f),
                Color.FromHex("#0D2513").WithAlpha(0.92f));
            var themedPanelRaised = ThemeValue(
                Color.FromHex("#686F80").WithAlpha(0.92f),
                Color.FromHex("#686F80").WithAlpha(0.92f),
                ButtonColorContext.WithAlpha(0.92f));
            var themedBorder = ThemeValue(
                Color.FromHex("#686D76").WithAlpha(0.95f),
                Color.FromHex("#686D76").WithAlpha(0.95f),
                UiButtonBorder.WithAlpha(0.95f));
            var themedBorderSoft = ThemeValue(
                Color.FromHex("#535861").WithAlpha(0.88f),
                Color.FromHex("#535861").WithAlpha(0.88f),
                UiButtonBorder.WithAlpha(0.88f));
            var themedText = ThemeValue(
                Color.FromHex("#EAF2FB"),
                Color.FromHex("#EEF2F6"),
                Color.FromHex("#E4F4E8"));
            var themedTextMuted = ThemeValue(
                Color.FromHex("#C1D2E5"),
                Color.FromHex("#C7CFD8"),
                Color.FromHex("#C7E6CC"));
            var loadoutButtonBase = ThemeValue(
                ButtonColorDefault,
                ButtonColorDefault,
                ButtonColorContext);
            var loadoutButtonAlt = ThemeValue(
                BlendTowards(ButtonColorDefault, PanelDark, 0.16f),
                BlendTowards(ButtonColorDefault, PanelDark, 0.16f),
                ButtonColorContext);
            var loadoutButtonHover = ThemeValue(
                ButtonColorHovered,
                ButtonColorHovered,
                ButtonColorContextHover);
            var loadoutButtonPressed = ThemeValue(
                BlendTowards(loadoutButtonBase, Color.White, 0.20f),
                BlendTowards(loadoutButtonBase, Color.White, 0.24f),
                BlendTowards(loadoutButtonHover, Color.White, 0.14f));
            var loadoutButtonPressedAlt = ThemeValue(
                BlendTowards(loadoutButtonAlt, Color.White, 0.24f),
                BlendTowards(loadoutButtonAlt, Color.White, 0.28f),
                BlendTowards(loadoutButtonHover, Color.White, 0.14f));
            var loadoutButtonPressedBorder = ThemeValue(
                BlendTowards(themedBorder, Color.White, 0.42f),
                BlendTowards(themedBorder, Color.White, 0.34f),
                BlendTowards(themedBorder, Color.White, 0.22f));
            var loadoutButtonPressedText = ThemeValue(
                Color.FromHex("#F5FAFF"),
                Color.FromHex("#FAFCFF"),
                Color.FromHex("#F2FFF4"));
            var voteButtonBase = ThemeValue(
                ButtonColorDefault,
                ButtonColorDefault,
                ButtonColorContext);
            var voteButtonHover = ThemeValue(
                ButtonColorHovered,
                ButtonColorHovered,
                ButtonColorContextHover);
            var voteButtonPressed = ThemeValue(
                ButtonColorPressed,
                ButtonColorPressed,
                ButtonColorContextPressed);
            var voteProgressBackground = ThemeValue(
                Color.FromHex("#121821").WithAlpha(0.95f),
                Color.FromHex("#121821").WithAlpha(0.95f),
                Color.FromHex("#051008").WithAlpha(0.95f));
            var voteProgressForeground = ThemeValue(
                Color.FromHex("#858A92").WithAlpha(0.95f),
                Color.FromHex("#858A92").WithAlpha(0.95f),
                Color.FromHex("#30B53C").WithAlpha(0.95f));
            var contextMenuText = useNeutralPalette
                ? Color.FromHex("#E7EEE8")
                : ThemeValue(
                    Color.FromHex("#EAF2FB"),
                    Color.FromHex("#EEF2F6"),
                    Color.FromHex("#E6F3E9"));
            var dropdownButtonText = useOldLobbyPalette ? OldLobbyText : Color.FromHex("#C5CED8");
            var dropdownButtonTextDisabled = dropdownButtonText.WithAlpha(0.72f);
            var optionsDropdownNormal = ThemeValue(
                DropdownButtonColorContext,
                DropdownButtonColorContext,
                DropdownButtonColorContext);
            var optionsDropdownHover = ThemeValue(
                DropdownButtonColorContextHover,
                DropdownButtonColorContextHover,
                DropdownButtonColorContextHover);
            var optionsDropdownPressed = ThemeValue(
                DropdownButtonColorContextPressed,
                DropdownButtonColorContextPressed,
                DropdownButtonColorContextPressed);
            var optionsDropdownDisabled = ThemeValue(
                DropdownButtonColorContextDisabled,
                DropdownButtonColorContextDisabled,
                DropdownButtonColorContextDisabled);
            var dropdownButtonNormal = new StyleBoxFlat
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButton : optionsDropdownNormal,
                BorderColor = useOldLobbyPalette ? OldLobbyButtonBorder : UiButtonBorder,
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 6,
                ContentMarginTopOverride = 4,
                ContentMarginRightOverride = 6,
                ContentMarginBottomOverride = 4,
            };
            var dropdownButtonHover = new StyleBoxFlat
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonHover : optionsDropdownHover,
                BorderColor = useOldLobbyPalette ? OldLobbyButtonBorderHover : UiButtonBorder,
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 6,
                ContentMarginTopOverride = 4,
                ContentMarginRightOverride = 6,
                ContentMarginBottomOverride = 4,
            };
            var dropdownButtonPressed = new StyleBoxFlat
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonPressed : optionsDropdownPressed,
                BorderColor = useOldLobbyPalette ? OldLobbyButtonBorderPressed : UiButtonBorder,
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 6,
                ContentMarginTopOverride = 4,
                ContentMarginRightOverride = 6,
                ContentMarginBottomOverride = 4,
            };
            var dropdownButtonDisabled = new StyleBoxFlat
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonDisabled : optionsDropdownDisabled,
                BorderColor = useOldLobbyPalette ? OldLobbyButtonBorderDisabled : UiButtonBorder,
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 6,
                ContentMarginTopOverride = 4,
                ContentMarginRightOverride = 6,
                ContentMarginBottomOverride = 4,
            };
            var dropdownOptionsBackground = new StyleBoxFlat
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyPanelSoft : optionsDropdownNormal,
                BorderThickness = new Thickness(1),
                BorderColor = useOldLobbyPalette ? OldLobbyButtonBorder : UiButtonBorder,
                ContentMarginLeftOverride = 0,
                ContentMarginTopOverride = 0,
                ContentMarginRightOverride = 0,
                ContentMarginBottomOverride = 0,
            };
            var optionsCategoryListBackground = useOldLobbyPalette
                ? OldLobbyPanel.WithAlpha(0.92f)
                : ThemeValue(
                    Color.FromHex("#17191D").WithAlpha(0.92f),
                    Color.FromHex("#17191D").WithAlpha(0.92f),
                    PanelDark.WithAlpha(0.75f));
            var optionsCategoryListBorder = useOldLobbyPalette
                ? OldLobbyButtonBorderHover
                : ThemeValue(
                    Color.FromHex("#686D76").WithAlpha(0.95f),
                    Color.FromHex("#686D76").WithAlpha(0.95f),
                    UiButtonBorder.WithAlpha(0.95f));
            var optionsCategoryButtonBorder = useOldLobbyPalette
                ? OldLobbyButtonBorder
                : ThemeValue(
                    Color.FromHex("#50555E"),
                    Color.FromHex("#50555E"),
                    UiButtonBorder.WithAlpha(0.95f));
            var optionsCategoryButtonNormal = useOldLobbyPalette
                ? OldLobbyButton.WithAlpha(0.96f)
                : ThemeValue(
                    ButtonColorDefault.WithAlpha(0.95f),
                    ButtonColorDefault.WithAlpha(0.95f),
                    Color.FromHex("#033D09"));
            var optionsCategoryButtonHover = useOldLobbyPalette
                ? OldLobbyButtonHover.WithAlpha(0.94f)
                : ThemeValue(
                    ButtonColorHovered.WithAlpha(0.92f),
                    ButtonColorHovered.WithAlpha(0.92f),
                    Color.FromHex("#04480B"));
            var optionsCategoryButtonPressed = useOldLobbyPalette
                ? OldLobbyButtonPressed.WithAlpha(0.92f)
                : ThemeValue(
                    ButtonColorPressed.WithAlpha(0.90f),
                    ButtonColorPressed.WithAlpha(0.90f),
                    Color.FromHex("#03340A"));
            var optionsCategoryButtonText = useOldLobbyPalette
                ? OldLobbyGold
                : ThemeValue(
                    Color.FromHex("#EAF3FC"),
                    Color.FromHex("#D5DDE7"),
                    Color.FromHex("#9DFFB2"));
            var genericButtonNormalFlat = new StyleBoxFlat
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButton : ButtonColorDefault,
                BorderColor = useOldLobbyPalette ? OldLobbyButtonBorder : UiButtonBorder,
                BorderThickness = new Thickness(1)
            };
            genericButtonNormalFlat.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
            genericButtonNormalFlat.SetContentMarginOverride(StyleBox.Margin.Horizontal, 4);

            var genericButtonHoverFlat = new StyleBoxFlat(genericButtonNormalFlat)
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonHover : ButtonColorHovered,
                BorderColor = useOldLobbyPalette ? OldLobbyButtonBorderHover : UiButtonBorder,
            };

            var genericButtonPressedFlat = new StyleBoxFlat(genericButtonNormalFlat)
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonPressed : ButtonColorPressed,
                BorderColor = useOldLobbyPalette ? OldLobbyButtonBorderPressed : UiButtonBorder,
            };

            var genericButtonDisabledFlat = new StyleBoxFlat(genericButtonNormalFlat)
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonDisabled : ButtonColorDisabled,
                BorderColor = useOldLobbyPalette ? OldLobbyButtonBorderDisabled : UiButtonBorder,
            };

            var cautionButtonNormalFlat = new StyleBoxFlat(genericButtonNormalFlat)
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonRed : ButtonColorCautionDefault,
            };

            var cautionButtonHoverFlat = new StyleBoxFlat(genericButtonNormalFlat)
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonRedHover : ButtonColorCautionHovered,
            };

            var cautionButtonPressedFlat = new StyleBoxFlat(genericButtonNormalFlat)
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonRed : ButtonColorCautionPressed,
            };

            var cautionButtonDisabledFlat = new StyleBoxFlat(genericButtonNormalFlat)
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonDisabled : ButtonColorCautionDisabled,
            };
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
            var exo2Bold13 = resCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
            var bedstead12 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 12);
            var bedstead14 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 14);
            var bedstead15 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 15);
            var bedstead16 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 16);
            var bedstead20 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 20);
            var notoSansMono = resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", size: 12);
            var robotoMonoBold11 = resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", size: 11);
            var robotoMonoBold12 = resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", size: 12);
            var robotoMonoBold14 = resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", size: 14);
            var paperDocumentFont12 = resCache.GetFont(new[]
            {
                "/Fonts/NotoSans/NotoSans-Regular.ttf",
                "/Fonts/NotoSans/NotoSansSymbols-Regular.ttf",
                "/Fonts/NotoSans/NotoSansSymbols2-Regular.ttf",
                "/Fonts/NotoEmoji.ttf"
            }, 12);

            var windowHeader = new StyleBoxFlat
            {
                BackgroundColor = ThemeValue(
                    Color.FromHex("#17304D"),
                    Color.FromHex("#262D37"),
                    Color.FromHex("#0E2A16")).WithAlpha(0.16f),
                ContentMarginBottomOverride = 0
            };
            var windowHeaderAlert = new StyleBoxFlat
            {
                BackgroundColor = ThemeValue(
                    Color.FromHex("#17304D"),
                    Color.FromHex("#262D37"),
                    Color.FromHex("#0E2A16")).WithAlpha(0.16f),
                ContentMarginBottomOverride = 0
            };
            var uiWindowBackgroundTint = ThemeValue(
                Color.FromHex("#0F2034"),
                Color.FromHex("#171D25"),
                Color.FromHex("#051A0B"));
            var windowBackground = new StyleBoxFlat
            {
                BackgroundColor = uiWindowBackgroundTint,
            };

            var optionsWindowBackground = new StyleBoxTexture
            {
                Texture = resCache.GetTexture("/Textures/_CCM14/Lobby/rightside_chat_bg.png"),
                Modulate = ThemeValue(
                    Color.FromHex("#0F2034").WithAlpha(0.96f),
                    Color.White.WithAlpha(1f),
                    Color.White.WithAlpha(1f)),
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
                BackgroundColor = ThemeValue(
                    Color.FromHex("#0A1530").WithAlpha(0.98f),
                    uiWindowBackgroundTint,
                    uiWindowBackgroundTint),
                BorderThickness = new Thickness(1),
                BorderColor = ThemeValue(
                    Color.FromHex("#132A56").WithAlpha(1f),
                    uiWindowBackgroundTint.WithAlpha(1f),
                    uiWindowBackgroundTint.WithAlpha(1f)),
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
                BackgroundColor = uiWindowBackgroundTint.WithAlpha(0.93f),
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

            var contextMenuButtonBase = ThemeValue(
                Color.FromHex("#2C4F93"),
                BlendTowards(ButtonColorContext, Color.Black, 0.12f),
                ButtonColorContext);
            var contextMenuButtonHover = ThemeValue(
                Color.FromHex("#3763B5"),
                BlendTowards(ButtonColorContextHover, Color.Black, 0.10f),
                ButtonColorContextHover);
            var contextMenuButtonPressed = ThemeValue(
                Color.FromHex("#223F78"),
                BlendTowards(ButtonColorContextPressed, Color.Black, 0.08f),
                ButtonColorContextPressed);
            var contextMenuButtonDisabled = ThemeValue(
                Color.FromHex("#26364E"),
                BlendTowards(ButtonColorContextDisabled, Color.Black, 0.06f),
                ButtonColorContextDisabled);
            var contextMenuExamineButtonBase = ThemeValue(
                Color.FromHex("#31579F"),
                BlendTowards(ExamineButtonColorContext, Color.Black, 0.12f),
                ExamineButtonColorContext).WithAlpha(0.62f);
            var contextMenuExamineButtonHover = ThemeValue(
                Color.FromHex("#3D6CC6"),
                BlendTowards(ExamineButtonColorContextHover, Color.Black, 0.10f),
                ExamineButtonColorContextHover).WithAlpha(0.78f);
            var contextMenuExamineButtonPressed = ThemeValue(
                Color.FromHex("#274785"),
                BlendTowards(ExamineButtonColorContextPressed, Color.Black, 0.08f),
                ExamineButtonColorContextPressed).WithAlpha(0.7f);
            var contextMenuExamineButtonDisabled = ThemeValue(
                Color.FromHex("#293B57"),
                BlendTowards(ExamineButtonColorContextDisabled, Color.Black, 0.06f),
                ExamineButtonColorContextDisabled).WithAlpha(0.45f);

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

            var chatChannelButton = new StyleBoxFlat
            {
                BackgroundColor = ButtonColorDefault,
                BorderColor = ButtonColorPressed,
                BorderThickness = new Thickness(1),
            };
            chatChannelButton.ContentMarginLeftOverride = 6;
            chatChannelButton.ContentMarginRightOverride = 6;
            chatChannelButton.ContentMarginTopOverride = 2;
            chatChannelButton.ContentMarginBottomOverride = 2;

            var chatChannelButtonHover = new StyleBoxFlat
            {
                BackgroundColor = ButtonColorHovered,
                BorderColor = ButtonColorPressed,
                BorderThickness = new Thickness(1),
            };
            chatChannelButtonHover.ContentMarginLeftOverride = 6;
            chatChannelButtonHover.ContentMarginRightOverride = 6;
            chatChannelButtonHover.ContentMarginTopOverride = 2;
            chatChannelButtonHover.ContentMarginBottomOverride = 2;

            var chatChannelButtonPressed = new StyleBoxFlat
            {
                BackgroundColor = ButtonColorPressed,
                BorderColor = ButtonColorPressed,
                BorderThickness = new Thickness(1),
            };
            chatChannelButtonPressed.ContentMarginLeftOverride = 6;
            chatChannelButtonPressed.ContentMarginRightOverride = 6;
            chatChannelButtonPressed.ContentMarginTopOverride = 2;
            chatChannelButtonPressed.ContentMarginBottomOverride = 2;

            var chatChannelButtonDisabled = new StyleBoxFlat
            {
                BackgroundColor = ButtonColorDisabled,
                BorderColor = ButtonColorPressed,
                BorderThickness = new Thickness(1),
            };
            chatChannelButtonDisabled.ContentMarginLeftOverride = 6;
            chatChannelButtonDisabled.ContentMarginRightOverride = 6;
            chatChannelButtonDisabled.ContentMarginTopOverride = 2;
            chatChannelButtonDisabled.ContentMarginBottomOverride = 2;

            var chatFilterButton = new StyleBoxFlat
            {
                BackgroundColor = Color.White,
                BorderColor = Color.White,
                BorderThickness = new Thickness(1),
            };
            chatFilterButton.ContentMarginLeftOverride = 3;
            chatFilterButton.ContentMarginRightOverride = 3;
            chatFilterButton.ContentMarginTopOverride = 3;
            chatFilterButton.ContentMarginBottomOverride = 3;

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
            var oldLobbyButtonFlat = new StyleBoxFlat
            {
                BackgroundColor = OldLobbyButton,
                BorderColor = OldLobbyButtonBorder,
                BorderThickness = new Thickness(1)
            };
            oldLobbyButtonFlat.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
            oldLobbyButtonFlat.SetContentMarginOverride(StyleBox.Margin.Horizontal, 6);
            var oldLobbyButtonHoverFlat = new StyleBoxFlat(oldLobbyButtonFlat)
            {
                BackgroundColor = OldLobbyButtonHover,
                BorderColor = OldLobbyButtonBorderHover,
            };
            var oldLobbyButtonPressedFlat = new StyleBoxFlat(oldLobbyButtonFlat)
            {
                BackgroundColor = OldLobbyButtonPressed,
                BorderColor = OldLobbyButtonBorderPressed,
            };
            var oldLobbyButtonDisabledFlat = new StyleBoxFlat(oldLobbyButtonFlat)
            {
                BackgroundColor = OldLobbyButtonDisabled,
                BorderColor = OldLobbyButtonBorderDisabled,
            };
            var oldLobbyButtonRedFlat = new StyleBoxFlat(oldLobbyButtonFlat)
            {
                BackgroundColor = OldLobbyButtonRed,
                BorderColor = OldLobbyButtonRedBorder,
            };
            var oldLobbyButtonRedHoverFlat = new StyleBoxFlat(oldLobbyButtonRedFlat)
            {
                BackgroundColor = OldLobbyButtonRedHover,
                BorderColor = OldLobbyButtonRedBorderHover,
            };
            var oldLobbyButtonRedPressedFlat = new StyleBoxFlat(oldLobbyButtonRedFlat)
            {
                BackgroundColor = Color.FromHex("#853A3A"),
                BorderColor = OldLobbyButtonRedBorder,
            };
            var oldLobbyButtonOpenRight = new StyleBoxFlat(oldLobbyButtonFlat);
            var oldLobbyButtonOpenLeft = new StyleBoxFlat(oldLobbyButtonFlat);
            var oldLobbyButtonOpenBoth = new StyleBoxFlat(oldLobbyButtonFlat);
            var oldLobbyButtonSquare = new StyleBoxFlat(oldLobbyButtonFlat);

            var textureInvertedTriangle = resCache.GetTexture("/Textures/Interface/Nano/inverted_triangle.svg.png");

            var lineEdit = new StyleBoxFlat
            {
                BackgroundColor = ThemeValue(
                    Color.FromHex("#0C1F44").WithAlpha(0.96f),
                    Color.FromHex("#181F27").WithAlpha(0.96f),
                    Color.FromHex("#071A0D").WithAlpha(0.96f)),
                BorderColor = ThemeValue(
                    Color.FromHex("#1F5CAB").WithAlpha(0.96f),
                    Color.FromHex("#686D76").WithAlpha(0.96f),
                    Color.FromHex("#2B7E45").WithAlpha(0.96f)),
                BorderThickness = new Thickness(1)
            };
            lineEdit.SetContentMarginOverride(StyleBox.Margin.Horizontal, 5);
            lineEdit.SetContentMarginOverride(StyleBox.Margin.Vertical, 3);

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
                BackgroundColor = ThemeValue(
                    Color.FromHex("#0A1324"),
                    Color.FromHex("#12181F"),
                    Color.FromHex("#0A120D")),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyButtonCrtHover = new StyleBoxFlat
            {
                BackgroundColor = ThemeValue(
                    Color.FromHex("#12203A"),
                    Color.FromHex("#1A232D"),
                    Color.FromHex("#122319")),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyButtonCrtPressed = new StyleBoxFlat
            {
                BackgroundColor = ThemeValue(
                    Color.FromHex("#16294A"),
                    Color.FromHex("#222D39"),
                    Color.FromHex("#15301F")),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyButtonClean = new StyleBoxFlat
            {
                BackgroundColor = ThemeValue(
                    Color.FromHex("#1A2740"),
                    Color.FromHex("#222A34"),
                    Color.FromHex("#14241A")),
                BorderColor = LobbyCleanAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyButtonCleanHover = new StyleBoxFlat
            {
                BackgroundColor = ThemeValue(
                    Color.FromHex("#243654"),
                    Color.FromHex("#2E3845"),
                    Color.FromHex("#1A3324")),
                BorderColor = LobbyCleanAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyButtonCleanPressed = new StyleBoxFlat
            {
                BackgroundColor = ThemeValue(
                    Color.FromHex("#2C4061"),
                    Color.FromHex("#394452"),
                    Color.FromHex("#1C3A28")),
                BorderColor = LobbyCleanAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyDiscordButton = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#5865F2").WithAlpha(0.92f),
                BorderColor = Color.FromHex("#AEB8FF"),
                BorderThickness = new Thickness(1)
            };

            var lobbyDiscordButtonHover = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#6C78FF").WithAlpha(0.96f),
                BorderColor = Color.FromHex("#D8DDFF"),
                BorderThickness = new Thickness(1)
            };

            var lobbyDiscordButtonPressed = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#3F4CC8").WithAlpha(0.98f),
                BorderColor = Color.FromHex("#AEB8FF"),
                BorderThickness = new Thickness(1)
            };

            var lobbyDiscordWarningPanel = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#101846").WithAlpha(0.86f),
                BorderColor = Color.FromHex("#6C78FF").WithAlpha(0.9f),
                BorderThickness = new Thickness(1)
            };
            lobbyDiscordWarningPanel.SetContentMarginOverride(StyleBox.Margin.All, 2);

            var lobbyMenuButtonCrt = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonBase,
                BorderColor = LobbyMenuButtonBase,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCrt.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyMenuButtonCrtHover = new StyleBoxFlat
            {
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonHover : LobbyCrtAccent.WithAlpha(0f),
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
                BackgroundColor = useOldLobbyPalette ? OldLobbyButtonHover : LobbyCrtAccent.WithAlpha(0f),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCleanHover.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyMenuButtonCleanPressed = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonPressed,
                BorderColor = LobbyMenuButtonPressed,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCleanPressed.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var lobbyMenuButtonCleanReadyPressed = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonReadyPressed,
                BorderColor = LobbyMenuButtonReadyPressed,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCleanReadyPressed.SetContentMarginOverride(StyleBox.Margin.All, 4);



            var lobbyMenuButtonCleanDisabled = new StyleBoxFlat
            {
                BackgroundColor = LobbyMenuButtonDisabledClean,
                BorderColor = LobbyMenuButtonDisabledClean,
                BorderThickness = new Thickness(1)
            };
            lobbyMenuButtonCleanDisabled.SetContentMarginOverride(StyleBox.Margin.All, 4);

            var mainMenuButtonNormal = new StyleBoxFlat
            {
                BackgroundColor = ButtonColorDefault,
                BorderColor = BlendTowards(ButtonColorDefault, Color.White, 0.22f),
                BorderThickness = new Thickness(1)
            };
            mainMenuButtonNormal.SetContentMarginOverride(StyleBox.Margin.Left, 8);
            mainMenuButtonNormal.SetContentMarginOverride(StyleBox.Margin.Top, 4);
            mainMenuButtonNormal.SetContentMarginOverride(StyleBox.Margin.Right, 8);
            mainMenuButtonNormal.SetContentMarginOverride(StyleBox.Margin.Bottom, 4);

            var mainMenuButtonHover = new StyleBoxFlat(mainMenuButtonNormal)
            {
                BackgroundColor = ButtonColorHovered,
                BorderColor = BlendTowards(ButtonColorHovered, Color.White, 0.24f),
            };

            var mainMenuButtonPressed = new StyleBoxFlat(mainMenuButtonNormal)
            {
                BackgroundColor = ButtonColorPressed,
                BorderColor = BlendTowards(ButtonColorPressed, Color.White, 0.18f),
            };

            var mainMenuButtonDisabled = new StyleBoxFlat(mainMenuButtonNormal)
            {
                BackgroundColor = ButtonColorDisabled,
                BorderColor = BlendTowards(ButtonColorDisabled, Color.White, 0.16f),
            };

            var lobbyChatPanelCrt = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0C1011").WithAlpha(0.88f),
                BorderColor = LobbyCrtAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyChatPanelClean = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#0D1813").WithAlpha(0.90f),
                BorderColor = LobbyCleanAccent,
                BorderThickness = new Thickness(1)
            };

            var lobbyChatInputCrt = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#030504"),
                BorderColor = Color.Transparent,
                BorderThickness = new Thickness(0)
            };

            var lobbyChatInputClean = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#04070B"),
                BorderColor = Color.Transparent,
                BorderThickness = new Thickness(0)
            };

            var actionSearchBox = new StyleBoxFlat
            {
                BackgroundColor = ThemeValue(
                    Color.FromHex("#12356A").WithAlpha(0.96f),
                    Color.FromHex("#222B35").WithAlpha(0.96f),
                    Color.FromHex("#0B2F15").WithAlpha(0.96f)),
                BorderColor = ThemeValue(
                    Color.FromHex("#1F5CAB").WithAlpha(0.98f),
                    Color.FromHex("#686D76").WithAlpha(0.98f),
                    Color.FromHex("#2B7E45").WithAlpha(0.98f)),
                BorderThickness = new Thickness(1)
            };
            actionSearchBox.SetContentMarginOverride(StyleBox.Margin.Horizontal, 5);
            actionSearchBox.SetContentMarginOverride(StyleBox.Margin.Vertical, 3);

            var tabContainerPanel = new StyleBoxFlat
            {
                BackgroundColor = PanelDark.WithAlpha(0.985f),
            };

            var tabContainerBoxActive = new StyleBoxFlat { BackgroundColor = PanelDark.WithAlpha(0.98f) };
            tabContainerBoxActive.SetContentMarginOverride(StyleBox.Margin.Horizontal, 8);
            tabContainerBoxActive.SetContentMarginOverride(StyleBox.Margin.Vertical, 3);
            var tabContainerBoxInactive = new StyleBoxFlat
            {
                BackgroundColor = ThemeValue(
                    Color.FromHex("#071532").WithAlpha(0.985f),
                    Color.FromHex("#181F28").WithAlpha(0.985f),
                    Color.FromHex("#031005").WithAlpha(0.985f))
            };
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
                Modulate = ThemeValue(
                    Color.FromHex("#0B1E3A"),
                    Color.FromHex("#181F28"),
                    Color.FromHex("#06130B")).WithAlpha(0.6f)
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
                BackgroundColor = ThemeValue(
                    Color.FromHex("#123A78").WithAlpha(0.9f),
                    Color.FromHex("#4D5D71").WithAlpha(0.9f),
                    Color.FromHex("#1A5A2B").WithAlpha(0.9f)),
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
            var sliderGrabNeutralColor = Color.FromHex("#8B939C");
            var sliderGrabNeutralBorderColor = Color.FromHex("#5D646C");
            var sliderBaseFillColor = ThemeValue(
                ButtonColorDefault,
                ButtonColorDefault,
                ButtonColorDefault);
            var sliderBaseBackColor = ThemeValue(
                BlendTowards(PanelDark, ButtonColorPressed, 0.38f),
                BlendTowards(PanelDark, ButtonColorPressed, 0.42f),
                BlendTowards(PanelDark, ButtonColorPressed, 0.36f));
            var sliderBaseOutlineColor = ThemeValue(
                UiButtonBorder.WithAlpha(0.95f),
                UiButtonBorder.WithAlpha(0.95f),
                UiButtonBorder.WithAlpha(0.95f));

            var sliderFillBox = new StyleBoxTexture
            {
                Texture = sliderFillTex,
                Modulate = sliderBaseFillColor,
            };

            var sliderBackBox = new StyleBoxTexture
            {
                Texture = sliderFillTex,
                Modulate = sliderBaseBackColor,
            };

            var sliderForeBox = new StyleBoxTexture
            {
                Texture = sliderOutlineTex,
                Modulate = sliderBaseOutlineColor,
            };

            var sliderGrabBox = new StyleBoxTexture
            {
                Texture = sliderGrabTex,
                Modulate = sliderGrabNeutralColor
            };

            sliderFillBox.SetPatchMargin(StyleBox.Margin.All, 12);
            sliderBackBox.SetPatchMargin(StyleBox.Margin.All, 12);
            sliderForeBox.SetPatchMargin(StyleBox.Margin.All, 12);
            sliderGrabBox.SetPatchMargin(StyleBox.Margin.All, 12);

            var sliderFillGreen = new StyleBoxTexture(sliderFillBox) { Modulate = Color.LimeGreen };
            var sliderFillRed = new StyleBoxTexture(sliderFillBox) { Modulate = Color.Red };
            var sliderFillBlue = new StyleBoxTexture(sliderFillBox) { Modulate = Color.Blue };
            var sliderFillWhite = new StyleBoxTexture(sliderFillBox) { Modulate = Color.FromHex("#D5FFE0") };

            var optionsSliderBackColor = ThemeValue(
                BlendTowards(PanelDark, ButtonColorPressed, 0.38f),
                BlendTowards(PanelDark, ButtonColorPressed, 0.42f),
                BlendTowards(PanelDark, ButtonColorPressed, 0.36f));
            var optionsSliderBorderColor = UiButtonBorder.WithAlpha(0.95f);
            var optionsSliderFillColor = ThemeValue(
                ButtonColorDefault,
                ButtonColorDefault,
                ButtonColorDefault);
            var optionsSliderGrabColor = sliderGrabNeutralColor;

            var optionsSliderBack = new StyleBoxFlat
            {
                BackgroundColor = optionsSliderBackColor,
                BorderThickness = new Thickness(1),
                BorderColor = optionsSliderBorderColor,
            };

            var optionsSliderFore = new StyleBoxFlat
            {
                BackgroundColor = Color.Transparent,
                BorderThickness = new Thickness(0),
                BorderColor = Color.Transparent,
            };

            var optionsSliderFill = new StyleBoxFlat
            {
                BackgroundColor = optionsSliderFillColor,
                BorderThickness = new Thickness(0),
            };

            var optionsSliderGrab = new StyleBoxFlat
            {
                BackgroundColor = optionsSliderGrabColor,
                BorderThickness = new Thickness(1),
                BorderColor = sliderGrabNeutralBorderColor,
            };
            var tacticalMapSliderBack = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#2A2D34").WithAlpha(0.96f),
                BorderThickness = new Thickness(1),
                BorderColor = Color.FromHex("#50555E").WithAlpha(0.95f),
            };
            tacticalMapSliderBack.SetContentMarginOverride(StyleBox.Margin.Vertical, 4);
            var tacticalMapSliderFore = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#181A1E").WithAlpha(0.98f),
                BorderThickness = new Thickness(1),
                BorderColor = Color.FromHex("#686D76").WithAlpha(0.90f),
            };
            tacticalMapSliderFore.SetContentMarginOverride(StyleBox.Margin.Vertical, 4);
            var tacticalMapSliderFill = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#686F80").WithAlpha(0.98f),
                BorderThickness = new Thickness(1),
                BorderColor = Color.FromHex("#858A92").WithAlpha(0.95f),
            };
            tacticalMapSliderFill.SetContentMarginOverride(StyleBox.Margin.Vertical, 4);
            var tacticalMapSliderGrab = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#DADDE1").WithAlpha(0.98f),
                BorderThickness = new Thickness(1),
                BorderColor = Color.FromHex("#969AA1").WithAlpha(0.98f),
            };
            tacticalMapSliderGrab.SetContentMarginOverride(StyleBox.Margin.Horizontal, 8);
            tacticalMapSliderGrab.SetContentMarginOverride(StyleBox.Margin.Vertical, 8);

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
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#0F2034").WithAlpha(0.96f),
                            PanelDark.WithAlpha(0.96f),
                            PanelDark.WithAlpha(0.96f)),
                        BorderColor = ThemeValue(
                            Color.FromHex("#25476C").WithAlpha(0.85f),
                            UiButtonBorder.WithAlpha(0.85f),
                            UiButtonBorder.WithAlpha(0.85f)),
                        BorderThickness = new Thickness(1),
                    }),

                Element<PanelContainer>().Class(StyleClassOptionsFooterPanel)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#0B1626").WithAlpha(0.92f),
                            Color.FromHex("#171D24").WithAlpha(0.88f),
                            PanelDark.WithAlpha(0.80f)),
                        BorderThickness = new Thickness(0, 1, 0, 0),
                        BorderColor = ThemeValue(
                            Color.FromHex("#25476C").WithAlpha(0.85f),
                            Color.FromHex("#555A63").WithAlpha(0.85f),
                            LobbyMenuButtonBase.WithAlpha(0.70f)),
                    }),
                // CCM rework ui - start
                Element<PanelContainer>().Class("CCMEscapeMenuBackground")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#061223").WithAlpha(0.96f),
                            Color.FromHex("#131A22").WithAlpha(0.96f),
                            Color.FromHex("#071B0D").WithAlpha(0.96f)),
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
                    .Prop(ContainerButton.StylePropertyStyleBox, useOldLobbyPalette ? oldLobbyButtonFlat : BaseButton),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Class(ButtonOpenRight)
                    .Prop(ContainerButton.StylePropertyStyleBox, useOldLobbyPalette ? oldLobbyButtonOpenRight : BaseButtonOpenRight),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Class(ButtonOpenLeft)
                    .Prop(ContainerButton.StylePropertyStyleBox, useOldLobbyPalette ? oldLobbyButtonOpenLeft : BaseButtonOpenLeft),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Class(ButtonOpenBoth)
                    .Prop(ContainerButton.StylePropertyStyleBox, useOldLobbyPalette ? oldLobbyButtonOpenBoth : BaseButtonOpenBoth),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Class(ButtonSquare)
                    .Prop(ContainerButton.StylePropertyStyleBox, useOldLobbyPalette ? oldLobbyButtonSquare : BaseButtonSquare),

                Element<OptionButton>()
                    .Prop(ContainerButton.StylePropertyStyleBox, useOldLobbyPalette ? oldLobbyButtonOpenBoth : optionButtonOpenBoth),

                Element<MultiselectOptionButton<object>>()
                    .Prop(ContainerButton.StylePropertyStyleBox, useOldLobbyPalette ? oldLobbyButtonOpenBoth : optionButtonOpenBoth),

                new StyleRule(new SelectorElement(typeof(Label), new[] { Button.StyleClassButton }, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                }),

                // Colors for the buttons.
                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, Color.White)
                    .Prop(ContainerButton.StylePropertyStyleBox, useOldLobbyPalette ? oldLobbyButtonFlat : genericButtonNormalFlat),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, Color.White)
                    .Prop(ContainerButton.StylePropertyStyleBox, genericButtonHoverFlat),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, Color.White)
                    .Prop(ContainerButton.StylePropertyStyleBox, genericButtonPressedFlat),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, Color.White)
                    .Prop(ContainerButton.StylePropertyStyleBox, genericButtonDisabledFlat),

                // Colors for the caution buttons.
                Element<ContainerButton>().Class(ContainerButton.StyleClassButton).Class(ButtonCaution)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, Color.White)
                    .Prop(ContainerButton.StylePropertyStyleBox, cautionButtonNormalFlat),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton).Class(ButtonCaution)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, Color.White)
                    .Prop(ContainerButton.StylePropertyStyleBox, cautionButtonHoverFlat),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton).Class(ButtonCaution)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, Color.White)
                    .Prop(ContainerButton.StylePropertyStyleBox, cautionButtonPressedFlat),

                Element<ContainerButton>().Class(ContainerButton.StyleClassButton).Class(ButtonCaution)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, Color.White)
                    .Prop(ContainerButton.StylePropertyStyleBox, cautionButtonDisabledFlat),

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
                    .Prop(Control.StylePropertyModulateSelf, contextMenuButtonBase),

                Element<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, contextMenuButtonHover),

                Element<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, contextMenuButtonPressed),

                Element<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, contextMenuButtonDisabled),

                // Context Menu Labels
                Element<RichTextLabel>().Class(InteractionVerb.DefaultTextStyleClass)
                    .Prop(Label.StylePropertyFont, notoSansBoldItalic12)
                    .Prop(Label.StylePropertyFontColor, contextMenuText),

                Element<RichTextLabel>().Class(ActivationVerb.DefaultTextStyleClass)
                    .Prop(Label.StylePropertyFont, notoSansBold12)
                    .Prop(Label.StylePropertyFontColor, contextMenuText),

                Element<RichTextLabel>().Class(AlternativeVerb.DefaultTextStyleClass)
                    .Prop(Label.StylePropertyFont, notoSansItalic12)
                    .Prop(Label.StylePropertyFontColor, contextMenuText),

                Element<RichTextLabel>().Class(Verb.DefaultTextStyleClass)
                    .Prop(Label.StylePropertyFont, notoSans12)
                    .Prop(Label.StylePropertyFontColor, contextMenuText),

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
                    .Prop(Control.StylePropertyModulateSelf, contextMenuExamineButtonBase),

                Element<ExamineButton>().Class(ExamineButton.StyleClassExamineButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, contextMenuExamineButtonHover),

                Element<ExamineButton>().Class(ExamineButton.StyleClassExamineButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, contextMenuExamineButtonPressed),

                Element<ExamineButton>().Class(ExamineButton.StyleClassExamineButton)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, contextMenuExamineButtonDisabled),

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
                        new StyleProperty(Label.StylePropertyFontColor, Color.White),
                    }),

                new StyleRule(new SelectorElement(typeof(Button), null, "mainMenu", null),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, mainMenuButtonNormal),
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorElement(typeof(Button), null, "mainMenu", new[] {ContainerButton.StylePseudoClassNormal}),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, mainMenuButtonNormal),
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorElement(typeof(Button), null, "mainMenu", new[] {ContainerButton.StylePseudoClassHover}),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, mainMenuButtonHover),
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorElement(typeof(Button), null, "mainMenu", new[] {ContainerButton.StylePseudoClassPressed}),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, mainMenuButtonPressed),
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorElement(typeof(Button), null, "mainMenu", new[] {ContainerButton.StylePseudoClassDisabled}),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, mainMenuButtonDisabled),
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
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
                        new StyleProperty("font-color", ThemeValue(
                            Color.FromHex("#86AEDD"),
                            Color.FromHex("#B1BBC8"),
                            Color.FromHex("#79B387"))),
                    }),

                Element<TextEdit>().Pseudo(TextEdit.StylePseudoClassPlaceholder)
                    .Prop("font-color", ThemeValue(
                        Color.FromHex("#86AEDD"),
                        Color.FromHex("#B1BBC8"),
                        Color.FromHex("#79B387"))),

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

                Element<Label>().Class(StyleClassLobbyTaskbarLabel).Class(StyleClassLobbyThemeCrt)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, bedstead15)
                    .Prop(Label.StylePropertyFontColor, Color.Black),

                Element<Label>().Class(StyleClassLobbyTaskbarLabel).Class(StyleClassLobbyThemeClean)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, bedstead15)
                    .Prop(Label.StylePropertyFontColor, Color.Black),

                Element<Label>().Class(StyleClassLobbyTaskbarLabel)
                    .Prop(Label.StylePropertyFontColor, Color.Black),

                Element<Label>().Class(StyleClassLobbyTaskbarMenuLabel).Class(StyleClassLobbyThemeCrt)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, bedstead15)
                    .Prop(Label.StylePropertyFontColor, Color.Black),

                Element<Label>().Class(StyleClassLobbyTaskbarMenuLabel).Class(StyleClassLobbyThemeClean)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, bedstead15)
                    .Prop(Label.StylePropertyFontColor, Color.Black),

                Element<Label>().Class(StyleClassLobbyTaskbarMenuLabel)
                    .Prop(Label.StylePropertyFontColor, Color.Black),

                Element<Label>().Class(StyleClassLobbyTaskbarLabelSmall).Class(StyleClassLobbyThemeCrt)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, bedstead15)
                    .Prop(Label.StylePropertyFontColor, Color.Black),

                Element<Label>().Class(StyleClassLobbyTaskbarLabelSmall).Class(StyleClassLobbyThemeClean)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                    .Prop(Label.StylePropertyFont, bedstead15)
                    .Prop(Label.StylePropertyFontColor, Color.Black),

                Element<Label>().Class(StyleClassLobbyTaskbarLabelSmall)
                    .Prop(Label.StylePropertyFontColor, Color.Black),

                Element<TextureRect>().Class(StyleClassLobbyTaskbarMenuIcon)
                    .Prop(Control.StylePropertyModulateSelf, LobbyMenuButtonBase.WithAlpha(0.9f)),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(typeof(ContainerButton), new[] {StyleClassLobbyMenuIconButton}, null, null),
                        new SelectorElement(typeof(BoxContainer), null, null, null)),
                    new SelectorElement(typeof(Label), new[] {StyleClassLobbyTaskbarLabel}, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(typeof(ContainerButton), new[] {StyleClassLobbyMenuIconButton}, null, null),
                        new SelectorElement(typeof(BoxContainer), null, null, null)),
                    new SelectorElement(typeof(Label), new[] {StyleClassLobbyTaskbarLabelSmall}, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(typeof(ContainerButton), new[] {StyleClassLobbyMenuIconButton}, null, new[] {ContainerButton.StylePseudoClassHover}),
                        new SelectorElement(typeof(BoxContainer), null, null, null)),
                    new SelectorElement(typeof(Label), new[] {StyleClassLobbyTaskbarLabel}, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, LobbyMenuButtonBase)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(typeof(ContainerButton), new[] {StyleClassLobbyMenuIconButton}, null, new[] {ContainerButton.StylePseudoClassHover}),
                        new SelectorElement(typeof(BoxContainer), null, null, null)),
                    new SelectorElement(typeof(Label), new[] {StyleClassLobbyTaskbarLabelSmall}, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, LobbyMenuButtonBase)
                    }),

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

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassEscapeMenuButton, StyleClassLobbyThemeCrt}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFont, bedstead15),
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyText : Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassEscapeMenuButton, StyleClassLobbyThemeClean}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFont, bedstead15),
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyText : Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassEscapeMenuButton, StyleClassLobbyThemeCrt}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyGold : LobbyCrtAccent)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassEscapeMenuButton, StyleClassLobbyThemeClean}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyGold : LobbyCrtAccent)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassEscapeMenuButton, StyleClassLobbyThemeCrt}, null, new[] {ContainerButton.StylePseudoClassPressed}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyText : Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassEscapeMenuButton, StyleClassLobbyThemeClean}, null, new[] {ContainerButton.StylePseudoClassPressed}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyText : Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassEscapeMenuButton, StyleClassLobbyThemeCrt}, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyMuted : Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyMenuButton, StyleClassEscapeMenuButton, StyleClassLobbyThemeClean}, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyMuted : Color.Black)
                    }),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCrt),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCrtHover),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCrtPressed),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCrtDisabled),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeClean)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonClean),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCleanHover),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCleanPressed),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Button.StylePropertyStyleBox, lobbyMenuButtonCleanDisabled),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassEscapeMenuButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Button.StylePropertyModulateSelf, Color.White),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassEscapeMenuButton, StyleClassLobbyThemeCrt}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFont, bedstead15),
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyText : Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassEscapeMenuButton, StyleClassLobbyThemeClean}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFont, bedstead15),
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyText : Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassEscapeMenuButton, StyleClassLobbyThemeCrt}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyGold : LobbyCrtAccent)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassEscapeMenuButton, StyleClassLobbyThemeClean}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyGold : LobbyCrtAccent)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassEscapeMenuButton, StyleClassLobbyThemeCrt}, null, new[] {ContainerButton.StylePseudoClassPressed}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyText : Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassEscapeMenuButton, StyleClassLobbyThemeClean}, null, new[] {ContainerButton.StylePseudoClassPressed}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyText : Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassEscapeMenuButton, StyleClassLobbyThemeCrt}, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyMuted : Color.Black)
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassEscapeMenuButton, StyleClassLobbyThemeClean}, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyMuted : Color.Black)
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
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<ContainerButton>().Class(StyleClassLobbyMenuIconButton)
                    .Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

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

                Element<Button>().Class(StyleClassLobbyDiscordLinkButton).Class(StyleClassLobbyThemeCrt)
                    .Prop(Button.StylePropertyStyleBox, lobbyDiscordButton),

                Element<Button>().Class(StyleClassLobbyDiscordLinkButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyDiscordButtonHover),

                Element<Button>().Class(StyleClassLobbyDiscordLinkButton).Class(StyleClassLobbyThemeCrt)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyDiscordButtonPressed),

                Element<Button>().Class(StyleClassLobbyDiscordLinkButton).Class(StyleClassLobbyThemeClean)
                    .Prop(Button.StylePropertyStyleBox, lobbyDiscordButton),

                Element<Button>().Class(StyleClassLobbyDiscordLinkButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, lobbyDiscordButtonHover),

                Element<Button>().Class(StyleClassLobbyDiscordLinkButton).Class(StyleClassLobbyThemeClean)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, lobbyDiscordButtonPressed),

                Element<Button>().Class(StyleClassLobbyDiscordLinkButton)
                    .Prop(nameof(Control.Margin), new Thickness(4, 0, 4, 0)),

                Element<PanelContainer>().Class(StyleClassLobbyDiscordLinkWarningPanel).Class(StyleClassLobbyThemeCrt)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyDiscordWarningPanel),

                Element<PanelContainer>().Class(StyleClassLobbyDiscordLinkWarningPanel).Class(StyleClassLobbyThemeClean)
                    .Prop(PanelContainer.StylePropertyPanel, lobbyDiscordWarningPanel),

                Element<RichTextLabel>().Class(StyleClassLobbyDiscordLinkWarningText).Class(StyleClassLobbyThemeCrt)
                    .Prop(Label.StylePropertyFont, notoSansBold12)
                    .Prop("font-color", Color.FromHex("#E6EAFF")),

                Element<RichTextLabel>().Class(StyleClassLobbyDiscordLinkWarningText).Class(StyleClassLobbyThemeClean)
                    .Prop(Label.StylePropertyFont, notoSansBold12)
                    .Prop("font-color", Color.FromHex("#E6EAFF")),

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

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassLobbyDiscordLinkButton}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                        new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#FFFFFF")),
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
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

                new StyleRule(new SelectorElement(typeof(OutputPanel), new[] {"DebugConsoleOutput"}, null, null), new[]
                {
                    new StyleProperty(OutputPanel.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#0E1C2B").WithAlpha(0.9f),
                            Color.FromHex("#171D24").WithAlpha(0.9f),
                            Color.FromHex("#0A160E").WithAlpha(0.9f)),
                        BorderColor = ThemeValue(
                            Color.FromHex("#416A90").WithAlpha(0.75f),
                            Color.FromHex("#686D76").WithAlpha(0.75f),
                            Color.FromHex("#2B7E45").WithAlpha(0.75f)),
                        BorderThickness = new Thickness(1),
                        ContentMarginLeftOverride = 3,
                        ContentMarginRightOverride = 3,
                        ContentMarginBottomOverride = 3,
                        ContentMarginTopOverride = 3,
                    })
                }),

                new StyleRule(new SelectorElement(typeof(HistoryLineEdit), new[] {"DebugConsoleInput"}, null, null), new[]
                {
                    new StyleProperty(LineEdit.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#0B1724").WithAlpha(0.96f),
                            Color.FromHex("#151B22").WithAlpha(0.96f),
                            Color.FromHex("#09110B").WithAlpha(0.96f)),
                        BorderColor = ThemeValue(
                            Color.FromHex("#416A90").WithAlpha(0.82f),
                            Color.FromHex("#686D76").WithAlpha(0.82f),
                            Color.FromHex("#2B7E45").WithAlpha(0.82f)),
                        BorderThickness = new Thickness(1)
                    })
                }),

                // Big Button
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassButtonBig}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty("font", notoSans16)
                    }),

                Element<Button>().Class(StyleClassOldLobbyButton)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassOldLobbyButton).Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassOldLobbyButton).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonHoverFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassOldLobbyButton).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonPressedFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassOldLobbyButton).Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonDisabledFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassOldLobbyButton}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFont, notoSans16),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassOldLobbyButton}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassOldLobbyButton}, null, new[] {ContainerButton.StylePseudoClassPressed}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                Element<ContainerButton>().Class(StyleClassOldLobbyButton)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<ContainerButton>().Class(StyleClassOldLobbyButton).Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<ContainerButton>().Class(StyleClassOldLobbyButton).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonHoverFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<ContainerButton>().Class(StyleClassOldLobbyButton).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonPressedFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<ContainerButton>().Class(StyleClassOldLobbyButton).Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonDisabledFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(ContainerButton), new[] {StyleClassOldLobbyButton}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFont, notoSans16),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(ContainerButton), new[] {StyleClassOldLobbyButton}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(ContainerButton), new[] {StyleClassOldLobbyButton}, null, new[] {ContainerButton.StylePseudoClassPressed}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                Element<Button>().Class(StyleClassOldLobbyButtonRed)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonRedFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassOldLobbyButtonRed).Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonRedFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassOldLobbyButtonRed).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonRedHoverFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassOldLobbyButtonRed).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonRedPressedFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class(StyleClassOldLobbyButtonRed).Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonDisabledFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassOldLobbyButtonRed}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFont, notoSans16),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassOldLobbyButtonRed}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {StyleClassOldLobbyButtonRed}, null, new[] {ContainerButton.StylePseudoClassPressed}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                Element<ContainerButton>().Class(StyleClassOldLobbyButtonRed)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonRedFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<ContainerButton>().Class(StyleClassOldLobbyButtonRed).Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonRedFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<ContainerButton>().Class(StyleClassOldLobbyButtonRed).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonRedHoverFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<ContainerButton>().Class(StyleClassOldLobbyButtonRed).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonRedPressedFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<ContainerButton>().Class(StyleClassOldLobbyButtonRed).Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(ContainerButton.StylePropertyStyleBox, oldLobbyButtonDisabledFlat)
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(ContainerButton), new[] {StyleClassOldLobbyButtonRed}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFont, notoSans16),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(ContainerButton), new[] {StyleClassOldLobbyButtonRed}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(ContainerButton), new[] {StyleClassOldLobbyButtonRed}, null, new[] {ContainerButton.StylePseudoClassPressed}),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                        new StyleProperty(nameof(Control.Margin), new Thickness(0)),
                        new StyleProperty(Label.StylePropertyFontColor, OldLobbyText),
                    }),

                Element<PanelContainer>().Class(StyleClassOldLobbyAngleRect)
                    .Prop(PanelContainer.StylePropertyPanel, BaseAngleRect)
                    .Prop(Control.StylePropertyModulateSelf, OldLobbyPanel),

                Element<StripeBack>().Class(StyleClassOldLobbyStripeBack)
                    .Prop(StripeBack.StylePropertyBackground, new StyleBoxTexture
                    {
                        Texture = stripeBackTex,
                        Mode = StyleBoxTexture.StretchMode.Tile,
                        Modulate = Color.FromHex("#121217").WithAlpha(0.92f),
                    }),

                Element<PanelContainer>().Class(StyleClassOldLobbyGoldDivider)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = OldLobbyGold,
                        ContentMarginTopOverride = 2
                    }),

                Element<Label>().Class(StyleClassOldLobbyHeading)
                    .Prop(Label.StylePropertyFont, notoSansBold16)
                    .Prop(Label.StylePropertyFontColor, OldLobbyGold),

                Element<Label>().Class(StyleClassOldLobbyTitle)
                    .Prop(Label.StylePropertyFont, notoSansBold18)
                    .Prop(Label.StylePropertyFontColor, OldLobbyGold),

                Element<Label>().Class(StyleClassOldLobbyMutedText)
                    .Prop(Label.StylePropertyFontColor, OldLobbyMuted),

                Element<Label>().Class(StyleClassOldLobbyCenteredText)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center),

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

                new StyleRule(new SelectorElement(typeof(Slider), new []{StyleClassTacticalMapSlider}, null, null), new []
                {
                    new StyleProperty(Slider.StylePropertyBackground, tacticalMapSliderBack),
                    new StyleProperty(Slider.StylePropertyForeground, tacticalMapSliderFore),
                    new StyleProperty(Slider.StylePropertyGrabber, tacticalMapSliderGrab),
                    new StyleProperty(Slider.StylePropertyFill, tacticalMapSliderFill),
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
                    new StyleProperty(ContainerButton.StylePropertyStyleBox, dropdownButtonNormal),
                }),
                new StyleRule(new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassNormal}), new[]
                {
                    new StyleProperty(ContainerButton.StylePropertyStyleBox, dropdownButtonNormal),
                }),
                new StyleRule(new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassHover}), new[]
                {
                    new StyleProperty(ContainerButton.StylePropertyStyleBox, dropdownButtonHover),
                }),
                new StyleRule(new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassPressed}), new[]
                {
                    new StyleProperty(ContainerButton.StylePropertyStyleBox, dropdownButtonPressed),
                }),
                new StyleRule(new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassDisabled}), new[]
                {
                    new StyleProperty(ContainerButton.StylePropertyStyleBox, dropdownButtonDisabled),
                }),

                new StyleRule(new SelectorElement(typeof(TextureRect), new[] {OptionButton.StyleClassOptionTriangle}, null, null), new[]
                {
                    new StyleProperty(TextureRect.StylePropertyTexture, textureInvertedTriangle),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassNormal}),
                    new SelectorElement(typeof(TextureRect), new[] {OptionButton.StyleClassOptionTriangle}, null, null)), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf, dropdownButtonText),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(TextureRect), new[] {OptionButton.StyleClassOptionTriangle}, null, null)), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassPressed}),
                    new SelectorElement(typeof(TextureRect), new[] {OptionButton.StyleClassOptionTriangle}, null, null)), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                    new SelectorElement(typeof(TextureRect), new[] {OptionButton.StyleClassOptionTriangle}, null, null)), new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf, dropdownButtonTextDisabled),
                }),

                new StyleRule(new SelectorElement(typeof(Label), new[] { OptionButton.StyleClassOptionButton }, null, null), new[]
                {
                    new StyleProperty(Label.StylePropertyAlignMode, Label.AlignMode.Center),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassNormal}),
                    new SelectorElement(typeof(Label), new[] {OptionButton.StyleClassOptionButton}, null, null)), new[]
                {
                    new StyleProperty(Label.StylePropertyFontColor, dropdownButtonText),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), new[] {OptionButton.StyleClassOptionButton}, null, null)), new[]
                {
                    new StyleProperty(Label.StylePropertyFontColor, Color.White),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassPressed}),
                    new SelectorElement(typeof(Label), new[] {OptionButton.StyleClassOptionButton}, null, null)), new[]
                {
                    new StyleProperty(Label.StylePropertyFontColor, Color.White),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                    new SelectorElement(typeof(Label), new[] {OptionButton.StyleClassOptionButton}, null, null)), new[]
                {
                    new StyleProperty(Label.StylePropertyFontColor, dropdownButtonTextDisabled),
                }),

                new StyleRule(new SelectorElement(typeof(Button), new[] {"optionButtonPopupItem"}, null, null), new[]
                {
                    new StyleProperty(Button.StylePropertyStyleBox, dropdownButtonNormal),
                }),
                new StyleRule(new SelectorElement(typeof(Button), new[] {"optionButtonPopupItem"}, null, new[] {ContainerButton.StylePseudoClassNormal}), new[]
                {
                    new StyleProperty(Button.StylePropertyStyleBox, dropdownButtonNormal),
                }),
                new StyleRule(new SelectorElement(typeof(Button), new[] {"optionButtonPopupItem"}, null, new[] {ContainerButton.StylePseudoClassHover}), new[]
                {
                    new StyleProperty(Button.StylePropertyStyleBox, dropdownButtonHover),
                }),
                new StyleRule(new SelectorElement(typeof(Button), new[] {"optionButtonPopupItem"}, null, new[] {ContainerButton.StylePseudoClassPressed}), new[]
                {
                    new StyleProperty(Button.StylePropertyStyleBox, dropdownButtonPressed),
                }),
                new StyleRule(new SelectorElement(typeof(Button), new[] {"optionButtonPopupItem"}, null, new[] {ContainerButton.StylePseudoClassDisabled}), new[]
                {
                    new StyleProperty(Button.StylePropertyStyleBox, dropdownButtonDisabled),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {"optionButtonPopupItem"}, null, new[] {ContainerButton.StylePseudoClassNormal}),
                    new SelectorElement(typeof(Label), null, null, null)), new[]
                {
                    new StyleProperty(Label.StylePropertyFontColor, dropdownButtonText),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {"optionButtonPopupItem"}, null, new[] {ContainerButton.StylePseudoClassHover}),
                    new SelectorElement(typeof(Label), null, null, null)), new[]
                {
                    new StyleProperty(Label.StylePropertyFontColor, Color.White),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {"optionButtonPopupItem"}, null, new[] {ContainerButton.StylePseudoClassPressed}),
                    new SelectorElement(typeof(Label), null, null, null)), new[]
                {
                    new StyleProperty(Label.StylePropertyFontColor, Color.White),
                }),
                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {"optionButtonPopupItem"}, null, new[] {ContainerButton.StylePseudoClassDisabled}),
                    new SelectorElement(typeof(Label), null, null, null)), new[]
                {
                    new StyleProperty(Label.StylePropertyFontColor, dropdownButtonTextDisabled),
                }),

                Element<PanelContainer>().Class(OptionButton.StyleClassOptionsBackground)
                    .Prop(PanelContainer.StylePropertyPanel, dropdownOptionsBackground),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Button), null, null, new[] {ContainerButton.StylePseudoClassNormal})),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, optionsButtonBase),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Button), null, null, new[] {ContainerButton.StylePseudoClassHover})),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, optionsButtonHover),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Button), null, null, new[] {ContainerButton.StylePseudoClassPressed})),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, optionsButtonPressed),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Button), null, null, new[] {ContainerButton.StylePseudoClassDisabled})),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, optionsButtonDisabled),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Button), new[] {StyleClassOptionsFooterButton}, null, new[] {ContainerButton.StylePseudoClassNormal})),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, optionsButtonBase),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Button), new[] {StyleClassOptionsFooterButton}, null, new[] {ContainerButton.StylePseudoClassHover})),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, optionsButtonHover),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Button), new[] {StyleClassOptionsFooterButton}, null, new[] {ContainerButton.StylePseudoClassPressed})),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, optionsButtonPressed),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Button), new[] {StyleClassOptionsFooterButton}, null, new[] {ContainerButton.StylePseudoClassDisabled})),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, optionsButtonDisabled),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassNormal})),
                    new[]
                    {
                        new StyleProperty(ContainerButton.StylePropertyStyleBox, dropdownButtonNormal),
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassHover})),
                    new[]
                    {
                        new StyleProperty(ContainerButton.StylePropertyStyleBox, dropdownButtonHover),
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassPressed})),
                    new[]
                    {
                        new StyleProperty(ContainerButton.StylePropertyStyleBox, dropdownButtonPressed),
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassDisabled})),
                    new[]
                    {
                        new StyleProperty(ContainerButton.StylePropertyStyleBox, dropdownButtonDisabled),
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassNormal})),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                        new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassNormal})),
                    new SelectorElement(typeof(TextureRect), new[] {OptionButton.StyleClassOptionTriangle}, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, dropdownButtonText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                        new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassHover})),
                    new SelectorElement(typeof(TextureRect), new[] {OptionButton.StyleClassOptionTriangle}, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, useOldLobbyPalette ? OldLobbyText : Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                        new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassPressed})),
                    new SelectorElement(typeof(TextureRect), new[] {OptionButton.StyleClassOptionTriangle}, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, useOldLobbyPalette ? OldLobbyText : Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassDisabled})),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                        new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassDisabled})),
                    new SelectorElement(typeof(TextureRect), new[] {OptionButton.StyleClassOptionTriangle}, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, dropdownButtonTextDisabled),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                        new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassNormal})),
                    new SelectorElement(typeof(Label), new[] {OptionButton.StyleClassOptionButton}, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, dropdownButtonText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                        new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassHover})),
                    new SelectorElement(typeof(Label), new[] {OptionButton.StyleClassOptionButton}, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyText : Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                        new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassPressed})),
                    new SelectorElement(typeof(Label), new[] {OptionButton.StyleClassOptionButton}, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, useOldLobbyPalette ? OldLobbyText : Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                        new SelectorElement(typeof(OptionButton), null, null, new[] {ContainerButton.StylePseudoClassDisabled})),
                    new SelectorElement(typeof(Label), new[] {OptionButton.StyleClassOptionButton}, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, dropdownButtonTextDisabled),
                    }),

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

                Element<PanelContainer>().Class("LauncherConnectingFrame")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = Color.FromHex("#111317").WithAlpha(0.96f),
                        BorderThickness = new Thickness(1),
                        BorderColor = ThemeValue(
                            Color.FromHex("#4A789F").WithAlpha(0.95f),
                            Color.FromHex("#686D76").WithAlpha(0.95f),
                            Color.FromHex("#2B7E45").WithAlpha(0.95f)),
                    }),

                Element<Label>().Class("LauncherConnectingTitle")
                    .Prop(Label.StylePropertyFont, resCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 22))
                    .Prop(Label.StylePropertyFontColor, ThemeValue(
                        Color.FromHex("#9DC4E5"),
                        Color.FromHex("#D6DDE5"),
                        Color.FromHex("#AFFFBD"))),

                Element<Label>().Class("LauncherConnectingStateLabel")
                    .Prop(Label.StylePropertyFont, resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 13))
                    .Prop(Label.StylePropertyFontColor, ThemeValue(
                        Color.FromHex("#B5CEE5"),
                        Color.FromHex("#C9D1D9"),
                        Color.FromHex("#BCEFC7"))),

                Element<Label>().Class("LauncherConnectingReasonSmallLabel")
                    .Prop(Label.StylePropertyFont, resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 13))
                    .Prop(Label.StylePropertyFontColor, ThemeValue(
                        Color.FromHex("#D5E3F1"),
                        Color.FromHex("#E1E6EC"),
                        Color.FromHex("#D7F0D8"))),

                Element<RichTextLabel>().Class("LauncherConnectingReasonSmallLabel")
                    .Prop("font", resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 13)),

                Element<Button>().Class("LauncherConnectingButton")
                    .Prop(Control.StylePropertyModulateSelf, ThemeValue(
                        ButtonColorDefault,
                        ButtonColorDefault,
                        Color.FromHex("#146A2C"))),

                Element<Button>().Class("LauncherConnectingButton").Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ThemeValue(
                        ButtonColorDefault,
                        ButtonColorDefault,
                        Color.FromHex("#146A2C"))),

                Element<Button>().Class("LauncherConnectingButton").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ThemeValue(
                        ButtonColorHovered,
                        ButtonColorHovered,
                        Color.FromHex("#1A7D35"))),

                Element<Button>().Class("LauncherConnectingButton").Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, ThemeValue(
                        ButtonColorPressed,
                        ButtonColorPressed,
                        Color.FromHex("#0E4F22"))),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {"LauncherConnectingButton"}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, bedstead15),
                        new StyleProperty(Label.StylePropertyFontColor, ThemeValue(
                            Color.FromHex("#B3D6FA"),
                            Color.FromHex("#E2E8EF"),
                            Color.FromHex("#C7FFD3"))),
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
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#0B1626").WithAlpha(0.9f),
                            Color.FromHex("#171E26").WithAlpha(0.9f),
                            Color.FromHex("#06130B").WithAlpha(0.9f)),
                        BorderThickness = new Thickness(1),
                        BorderColor = ThemeValue(
                            Color.FromHex("#25476C").WithAlpha(0.9f),
                            Color.FromHex("#555A63").WithAlpha(0.9f),
                            Color.FromHex("#1E3A28").WithAlpha(0.9f)),
                    }),

                Element<PanelContainer>().Class("CMWindowBodyBackground")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#10233A").WithAlpha(0.85f),
                            Color.FromHex("#141A21").WithAlpha(0.85f),
                            Color.FromHex("#001304").WithAlpha(0.85f)),
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
                    .Prop("font-color", ThemeValue(
                        Color.FromHex("#EAF2FB"),
                        Color.FromHex("#E7ECF2"),
                        Color.FromHex("#DCEFE0"))),

                Element<PanelContainer>().Class("WindowHeadingBackground")
                    .Prop("panel", new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#17304D"),
                            Color.FromHex("#262E39"),
                            Color.FromHex("#0F2A17")).WithAlpha(0.96f),
                    }),

                Element<PanelContainer>().Class("WindowHeadingBackgroundLight")
                    .Prop("panel", new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#1D3D5E"),
                            Color.FromHex("#303947"),
                            Color.FromHex("#163820")).WithAlpha(0.84f),
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

                // CCM rework lobby - start
                Element<TextureButton>().Class("CharacterSetupCloseButtonTransparent")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Nano/cross.svg.png"))
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#E6E6E6"))
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxEmpty())
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxEmpty()),

                Element<TextureButton>().Class("CharacterSetupCloseButtonTransparent").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#FFFFFF"))
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxEmpty())
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxEmpty()),

                Element<TextureButton>().Class("CharacterSetupCloseButtonTransparent").Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#D0D0D0"))
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxEmpty())
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxEmpty()),
                // CCM rework lobby - end

                // CCM rework lobby - start
                Element<TextureButton>().Class("CharacterSetupVisibleCloseButton")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Nano/cross.svg.png"))
                    .Prop(Control.StylePropertyModulateSelf, ThemeValue(
                        Color.FromHex("#C9D8F2"),
                        Color.FromHex("#D7DEE7"),
                        Color.FromHex("#D0E6D2")))
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxEmpty())
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxEmpty()),

                Element<TextureButton>().Class("CharacterSetupVisibleCloseButton").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ThemeValue(
                        Color.FromHex("#E3EEFF"),
                        Color.FromHex("#F0F3F7"),
                        Color.FromHex("#E2F6E4")))
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxEmpty())
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxEmpty()),

                Element<TextureButton>().Class("CharacterSetupVisibleCloseButton").Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, ThemeValue(
                        Color.FromHex("#9DB8E6"),
                        Color.FromHex("#ABB6C3"),
                        Color.FromHex("#A9D5AE")))
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxEmpty())
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxEmpty()),
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
                    .Prop(Label.StylePropertyFont, paperDocumentFont12),

                Element<RichTextLabel>().Class("LabelSubText")
                    .Prop(Label.StylePropertyFont, notoSans10)
                    .Prop(Label.StylePropertyFontColor, Color.FromHex("#3A6B47")),

                Element<LineEdit>().Class("PaperLineEdit")
                    .Prop(LineEdit.StylePropertyStyleBox, new StyleBoxEmpty())
                    .Prop(Label.StylePropertyFont, paperDocumentFont12),
                Element<TextEdit>().Class("PaperLineEdit")
                    .Prop("font", paperDocumentFont12)
                    .Prop("font-color", Color.FromHex("#0B140E")),

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
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#17304A").WithAlpha(0.86f),
                            Color.FromHex("#1C1E22").WithAlpha(0.86f),
                            Color.FromHex("#0A2C18").WithAlpha(0.82f)),
                        BorderColor = Color.Transparent,
                        BorderThickness = new Thickness(0f),
                        ContentMarginLeftOverride = 16f,
                        ContentMarginRightOverride = 16f,
                        ContentMarginTopOverride = 3f,
                        ContentMarginBottomOverride = 3f
                    })
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class("CharacterSetupActionButton").Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#17304A").WithAlpha(0.86f),
                            Color.FromHex("#1C1E22").WithAlpha(0.86f),
                            Color.FromHex("#0A2C18").WithAlpha(0.82f)),
                        BorderColor = Color.Transparent,
                        BorderThickness = new Thickness(0f),
                        ContentMarginLeftOverride = 16f,
                        ContentMarginRightOverride = 16f,
                        ContentMarginTopOverride = 3f,
                        ContentMarginBottomOverride = 3f
                    })
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class("CharacterSetupActionButton").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#264D72").WithAlpha(0.89f),
                            Color.FromHex("#232B35").WithAlpha(0.89f),
                            Color.FromHex("#0A2C18").WithAlpha(0.86f)),
                        BorderColor = Color.Transparent,
                        BorderThickness = new Thickness(0f),
                        ContentMarginLeftOverride = 16f,
                        ContentMarginRightOverride = 16f,
                        ContentMarginTopOverride = 3f,
                        ContentMarginBottomOverride = 3f
                    })
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class("CharacterSetupActionButton").Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#1D3D5E").WithAlpha(0.92f),
                            Color.FromHex("#202730").WithAlpha(0.92f),
                            Color.FromHex("#0A2C18").WithAlpha(0.90f)),
                        BorderColor = Color.Transparent,
                        BorderThickness = new Thickness(0f),
                        ContentMarginLeftOverride = 16f,
                        ContentMarginRightOverride = 16f,
                        ContentMarginTopOverride = 3f,
                        ContentMarginBottomOverride = 3f
                    })
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                Element<Button>().Class("CharacterSetupActionButton").Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = ThemeValue(
                            Color.FromHex("#17304A").WithAlpha(0.56f),
                            Color.FromHex("#202329").WithAlpha(0.72f),
                            Color.FromHex("#0A2C18").WithAlpha(0.68f)),
                        BorderColor = Color.Transparent,
                        BorderThickness = new Thickness(0f),
                        ContentMarginLeftOverride = 16f,
                        ContentMarginRightOverride = 16f,
                        ContentMarginTopOverride = 3f,
                        ContentMarginBottomOverride = 3f
                    })
                    .Prop(Control.StylePropertyModulateSelf, Color.White),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {"CharacterSetupActionButton"}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                        new StyleProperty(Label.StylePropertyFontColor, ThemeValue(
                            Color.FromHex("#D5E5F4"),
                            Color.FromHex("#D2DAE4"),
                            Color.FromHex("#B7FFC8"))),
                    }),

                Element<Label>().Class("CharacterCarouselName")
                    .Prop(Label.StylePropertyFont, notoSansBold12),

                Element<Label>().Class("CharacterCarouselNameSelected")
                    .Prop(Label.StylePropertyFont, notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, ThemeValue(
                        Color.FromHex("#A8CAE8"),
                        Color.FromHex("#E2E8EF"),
                        Color.FromHex("#BFFFD0"))),

                Element<Label>().Class("CharacterEditorSectionTitle")
                    .Prop(Label.StylePropertyFont, notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, ThemeValue(
                        Color.FromHex("#93BFE3"),
                        Color.FromHex("#D0D8E1"),
                        Color.FromHex("#AFFFBD"))),

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
                        BackgroundColor = useOldLobbyPalette
                            ? OldLobbyPanel.WithAlpha(0.92f)
                            : ThemeValue(
                                Color.FromHex("#10233A").WithAlpha(0.85f),
                                Color.FromHex("#202329").WithAlpha(0.85f),
                                PanelDark.WithAlpha(0.75f)),
                        BorderThickness = new Thickness(2, 0, 0, 0),
                        BorderColor = useOldLobbyPalette
                            ? OldLobbyButtonBorderHover
                            : LobbyMenuButtonBase.WithAlpha(0.95f),
                    }),

                Element<PanelContainer>().Class("VerticalTabContentBackground")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat(
                        useOldLobbyPalette
                            ? OldLobbyPanelSoft.WithAlpha(0.94f)
                            : PanelDark.WithAlpha(0.9f))),

                Element<Button>().Class(StyleBase.StyleClassVerticalTabButton)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = useOldLobbyPalette
                            ? OldLobbyButton.WithAlpha(0.96f)
                            : ThemeValue(
                                BlendTowards(ButtonColorDefault, PanelDark, 0.34f).WithAlpha(0.9f),
                                BlendTowards(ButtonColorDefault, PanelDark, 0.34f).WithAlpha(0.9f),
                                PanelDark.WithAlpha(0.9f)),
                        BorderThickness = new Thickness(1),
                        BorderColor = useOldLobbyPalette
                            ? OldLobbyButtonBorder
                            : PanelDark.WithAlpha(1f),
                    }),

                Element<Button>().Class(StyleBase.StyleClassVerticalTabButton)
                    .Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = useOldLobbyPalette
                            ? OldLobbyButtonHover.WithAlpha(0.94f)
                            : ThemeValue(
                                ButtonColorHovered.WithAlpha(0.8f),
                                ButtonColorHovered.WithAlpha(0.8f),
                                LobbyMenuButtonBase.WithAlpha(0.7f)),
                        BorderThickness = new Thickness(1),
                        BorderColor = useOldLobbyPalette
                            ? OldLobbyButtonBorderHover
                            : ThemeValue(
                                BlendTowards(ButtonColorHovered, Color.White, 0.18f).WithAlpha(0.9f),
                                BlendTowards(ButtonColorHovered, Color.White, 0.18f).WithAlpha(0.9f),
                                LobbyMenuButtonBase.WithAlpha(0.8f)),
                    }),

                Element<Button>().Class(StyleBase.StyleClassVerticalTabButton)
                    .Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = useOldLobbyPalette
                            ? OldLobbyButtonPressed.WithAlpha(0.92f)
                            : LobbyMenuButtonPressed.WithAlpha(0.7f),
                        BorderThickness = new Thickness(1),
                        BorderColor = useOldLobbyPalette
                            ? OldLobbyButtonBorderPressed
                            : LobbyMenuButtonPressed.WithAlpha(0.9f),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(PanelContainer), new[] {"VerticalTabListBackground"}, null, null)),
                    new[]
                    {
                        new StyleProperty(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                        {
                            BackgroundColor = optionsCategoryListBackground,
                            BorderThickness = new Thickness(2, 0, 0, 0),
                            BorderColor = optionsCategoryListBorder,
                        }),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Button), new[] {StyleBase.StyleClassVerticalTabButton}, null, null)),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, new StyleBoxFlat
                        {
                            BackgroundColor = optionsCategoryButtonNormal,
                            BorderThickness = new Thickness(1),
                            BorderColor = optionsCategoryButtonBorder,
                        }),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Button), new[] {StyleBase.StyleClassVerticalTabButton}, null, new[] {ContainerButton.StylePseudoClassHover})),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, new StyleBoxFlat
                        {
                            BackgroundColor = optionsCategoryButtonHover,
                            BorderThickness = new Thickness(1),
                            BorderColor = optionsCategoryButtonBorder,
                        }),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                    new SelectorElement(typeof(Button), new[] {StyleBase.StyleClassVerticalTabButton}, null, new[] {ContainerButton.StylePseudoClassPressed})),
                    new[]
                    {
                        new StyleProperty(Button.StylePropertyStyleBox, new StyleBoxFlat
                        {
                            BackgroundColor = optionsCategoryButtonPressed,
                            BorderThickness = new Thickness(1),
                            BorderColor = optionsCategoryButtonBorder,
                        }),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorChild(
                        new SelectorElement(null, new[] {StyleBase.StyleClassOptionsMenuRoot}, null, null),
                        new SelectorElement(typeof(Button), new[] {StyleBase.StyleClassVerticalTabButton}, null, null)),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, optionsCategoryButtonText),
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
                        new StyleProperty(TabContainer.stylePropertyTabFontColor, useOldLobbyPalette
                            ? OldLobbyGold
                            : ThemeValue(
                                Color.FromHex("#D5E5F4"),
                                Color.FromHex("#D2DAE4"),
                                Color.FromHex("#B7FFC8"))),
                        new StyleProperty(TabContainer.StylePropertyTabFontColorInactive, useOldLobbyPalette
                            ? OldLobbyText
                            : ThemeValue(
                                Color.FromHex("#AFC5DA"),
                                Color.FromHex("#A4AFBC"),
                                Color.FromHex("#94D5A3"))),
                        new StyleProperty(TabContainer.StylePropertyTabStyleBox, new StyleBoxFlat
                        {
                            BackgroundColor = useOldLobbyPalette
                                ? OldLobbyButtonPressed.WithAlpha(0.96f)
                                : ThemeValue(
                                    Color.FromHex("#1D3D5E").WithAlpha(0.92f),
                                    Color.FromHex("#202730").WithAlpha(0.92f),
                                    Color.FromHex("#0A2C18").WithAlpha(0.9f)),
                            BorderColor = useOldLobbyPalette ? OldLobbyButtonBorderPressed : Color.Transparent,
                            BorderThickness = useOldLobbyPalette ? new Thickness(1f) : new Thickness(0f),
                            ContentMarginLeftOverride = 16f,
                            ContentMarginRightOverride = 16f,
                            ContentMarginTopOverride = 3f,
                            ContentMarginBottomOverride = 3f
                        }),
                        new StyleProperty(TabContainer.StylePropertyTabStyleBoxInactive, new StyleBoxFlat
                        {
                            BackgroundColor = useOldLobbyPalette
                                ? OldLobbyButton.WithAlpha(0.9f)
                                : ThemeValue(
                                    Color.FromHex("#17304A").WithAlpha(0.86f),
                                    Color.FromHex("#1C1E22").WithAlpha(0.86f),
                                    Color.FromHex("#0A2C18").WithAlpha(0.82f)),
                            BorderColor = useOldLobbyPalette ? OldLobbyButtonBorder : Color.Transparent,
                            BorderThickness = useOldLobbyPalette ? new Thickness(1f) : new Thickness(0f),
                            ContentMarginLeftOverride = 16f,
                            ContentMarginRightOverride = 16f,
                            ContentMarginTopOverride = 3f,
                            ContentMarginBottomOverride = 3f
                        }),
                        new StyleProperty(Content.Client._CCM.UserInterface.Controls.CenteredTabContainer.StylePropertyTabStyleBoxHover, new StyleBoxFlat
                        {
                            BackgroundColor = useOldLobbyPalette
                                ? OldLobbyButtonHover.WithAlpha(0.94f)
                                : ThemeValue(
                                    Color.FromHex("#264D72").WithAlpha(0.89f),
                                    Color.FromHex("#232B35").WithAlpha(0.89f),
                                    Color.FromHex("#0A2C18").WithAlpha(0.86f)),
                            BorderColor = useOldLobbyPalette ? OldLobbyButtonBorderHover : Color.Transparent,
                            BorderThickness = useOldLobbyPalette ? new Thickness(1f) : new Thickness(0f),
                            ContentMarginLeftOverride = 16f,
                            ContentMarginRightOverride = 16f,
                            ContentMarginTopOverride = 3f,
                            ContentMarginBottomOverride = 3f
                        }),
                        new StyleProperty(Content.Client._CCM.UserInterface.Controls.CenteredTabContainer.StylePropertyTabFontColorHover, useOldLobbyPalette
                            ? OldLobbyGold
                            : ThemeValue(
                                Color.FromHex("#EAF2FB"),
                                Color.FromHex("#E2E8EF"),
                                Color.FromHex("#A7EDB5"))),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassCMProfileFont}, null, null),
                    new SelectorElement(typeof(Button), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, useOldLobbyPalette ? Color.White : LobbyMenuButtonBase),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassCMProfileFont}, null, null),
                    new SelectorElement(typeof(Button), null, null, new[] { ContainerButton.StylePseudoClassHover })),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, useOldLobbyPalette ? Color.White : ButtonColorHovered),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassCMProfileFont}, null, null),
                    new SelectorElement(typeof(Button), null, null, new[] { ContainerButton.StylePseudoClassPressed })),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, useOldLobbyPalette ? Color.White : ButtonColorPressed),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassCMProfileFont}, null, null),
                    new SelectorElement(typeof(OptionButton), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassCMProfileFont}, null, null),
                    new SelectorElement(typeof(OptionButton), null, null, new[] { ContainerButton.StylePseudoClassHover })),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(BoxContainer), new[] {StyleClassCMProfileFont}, null, null),
                    new SelectorElement(typeof(OptionButton), null, null, new[] { ContainerButton.StylePseudoClassPressed })),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.White),
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

                Element<PanelContainer>().Class(StyleClassLoadoutNamePanel)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanelAlt,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                    }),

                Element<PanelContainer>().Class(StyleClassLoadoutSectionPanel)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanel,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                    }),

                Element<PanelContainer>().Class(StyleClassLoadoutSpriteFrame)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanelAlt,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                    }),

                Element<PanelContainer>().Class(StyleClassLoadoutSubgroupPanel)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanelAlt.WithAlpha(0.90f),
                        BorderThickness = new Thickness(1, 0, 0, 0),
                        BorderColor = themedBorderSoft.WithAlpha(0.72f),
                        ContentMarginLeftOverride = 10,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 0,
                        ContentMarginBottomOverride = 4,
                    }),

                Element<Label>().Class("LoadoutSectionTitle")
                    .Prop(Label.StylePropertyFont, notoSansBold12)
                    .Prop(Label.StylePropertyFontColor, themedText),

                Element<Button>().Class(StyleClassLoadoutItemButton)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = loadoutButtonBase,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 6,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 6,
                    }),

                Element<Button>().Class(StyleClassLoadoutItemButton).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = loadoutButtonHover,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorder,
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 6,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 6,
                    }),

                Element<Button>().Class(StyleClassLoadoutItemButton).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = loadoutButtonPressed,
                        BorderThickness = new Thickness(1),
                        BorderColor = loadoutButtonPressedBorder,
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 6,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 6,
                    }),

                Element<Button>().Class(StyleClassLoadoutItemButton).Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = BlendTowards(loadoutButtonBase, Color.Black, 0.24f),
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft.WithAlpha(0.50f),
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 6,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 6,
                    }),

                Element<Button>().Class(StyleClassLoadoutItemButtonAlt)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = loadoutButtonAlt,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 6,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 6,
                    }),

                Element<Button>().Class(StyleClassLoadoutItemButtonAlt).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = loadoutButtonHover,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorder,
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 6,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 6,
                    }),

                Element<Button>().Class(StyleClassLoadoutItemButtonAlt).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = loadoutButtonPressedAlt,
                        BorderThickness = new Thickness(1),
                        BorderColor = loadoutButtonPressedBorder,
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 6,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 6,
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassLoadoutItemButton }, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                        new StyleProperty(Label.StylePropertyFontColor, themedTextMuted),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassLoadoutItemButton }, null, new[] { ContainerButton.StylePseudoClassHover }),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, themedText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassLoadoutItemButtonAlt }, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                        new StyleProperty(Label.StylePropertyFontColor, themedTextMuted),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassLoadoutItemButtonAlt }, null, new[] { ContainerButton.StylePseudoClassHover }),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, themedText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassLoadoutItemButton }, null, new[] { ContainerButton.StylePseudoClassPressed }),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, loadoutButtonPressedText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassLoadoutItemButtonAlt }, null, new[] { ContainerButton.StylePseudoClassPressed }),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, loadoutButtonPressedText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassLoadoutItemButton }, null, new[] { ContainerButton.StylePseudoClassDisabled }),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, themedTextMuted.WithAlpha(0.55f)),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassLoadoutItemButtonAlt }, null, new[] { ContainerButton.StylePseudoClassDisabled }),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, themedTextMuted.WithAlpha(0.55f)),
                    }),

                Element<Button>().Class(StyleClassLoadoutToggleButton)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanelRaised,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                        ContentMarginLeftOverride = 4,
                        ContentMarginTopOverride = 2,
                        ContentMarginRightOverride = 4,
                        ContentMarginBottomOverride = 2,
                    }),

                Element<Button>().Class(StyleClassLoadoutToggleButton).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = loadoutButtonHover,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorder,
                        ContentMarginLeftOverride = 4,
                        ContentMarginTopOverride = 2,
                        ContentMarginRightOverride = 4,
                        ContentMarginBottomOverride = 2,
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassLoadoutToggleButton }, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                        new StyleProperty(Label.StylePropertyFontColor, themedText),
                    }),

                Element<PanelContainer>().Class(StyleClassGuidebookTreePanel)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanel,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                    }),

                Element<PanelContainer>().Class(StyleClassGuidebookContentPanel)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanel,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                    }),

                Element<PanelContainer>().Class(StyleClassGuidebookSearchPanel)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanelAlt,
                        BorderThickness = new Thickness(0, 0, 0, 1),
                        BorderColor = themedBorderSoft.WithAlpha(0.70f),
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 6,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 6,
                    }),

                new StyleRule(new SelectorElement(typeof(LineEdit), new[] { StyleClassGuidebookSearchBar }, null, null), new[]
                {
                    new StyleProperty(LineEdit.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanelRaised,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 4,
                    }),
                    new StyleProperty(LineEdit.StylePropertyCursorColor, themedText),
                    new StyleProperty(LineEdit.StylePropertySelectionColor, themedBorder.WithAlpha(0.30f)),
                }),

                Element<PanelContainer>().Class(StyleClassGuidebookPlaceholderPanel)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanelAlt,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorder,
                    }),

                Element<Label>().Class("GuidebookPlaceholderTitle")
                    .Prop(Label.StylePropertyFont, notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, themedText),

                Element<Label>().Class("GuidebookPlaceholderText")
                    .Prop(Label.StylePropertyFontColor, themedTextMuted),

                Element<PanelContainer>().Class(StyleClassGuidebookEmbedCard)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanelAlt,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                    }),

                Element<Label>().Class(StyleClassGuidebookEmbedHeader)
                    .Prop(Label.StylePropertyFont, notoSansBold12)
                    .Prop(Label.StylePropertyFontColor, themedText),

                Element<RichTextLabel>().Class(StyleClassGuidebookEmbedHeader)
                    .Prop("font", notoSansBold12),

                Element<CollapsibleHeading>().Class(StyleClassGuidebookEmbedSectionHeading)
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanelRaised,
                        BorderThickness = new Thickness(1, 1, 1, 0),
                        BorderColor = themedBorderSoft,
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 4,
                    }),

                Element<CollapsibleHeading>().Class(StyleClassGuidebookEmbedSectionHeading).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = voteButtonHover,
                        BorderThickness = new Thickness(1, 1, 1, 0),
                        BorderColor = themedBorder,
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 4,
                    }),

                Element<CollapsibleHeading>().Class(StyleClassGuidebookEmbedSectionHeading).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(ContainerButton.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = voteButtonPressed,
                        BorderThickness = new Thickness(1, 1, 1, 0),
                        BorderColor = themedBorder,
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 4,
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(CollapsibleHeading), new[] { StyleClassGuidebookEmbedSectionHeading }, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                        new StyleProperty(Label.StylePropertyFontColor, themedText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(CollapsibleHeading), new[] { StyleClassGuidebookEmbedSectionHeading }, null, null),
                    new SelectorElement(typeof(TextureRect), new[] { OptionButton.StyleClassOptionTriangle }, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, themedTextMuted),
                    }),

                Element<CollapsibleBody>().Class(StyleClassGuidebookEmbedSectionBody)
                    .Prop(Control.StylePropertyModulateSelf, Color.White)
                    .Prop(nameof(Control.Margin), new Thickness(0, 0, 0, 4)),

                Element<PanelContainer>().Class(StyleClassVotePanel)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanel,
                        BorderThickness = new Thickness(0),
                        BorderColor = themedBorder,
                    }),

                Element<PanelContainer>().Class(StyleClassVoteSectionPanel)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanelAlt,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 6,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 6,
                    }),

                Element<Button>().Class(StyleClassVoteActionButton)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = voteButtonBase,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 5,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 5,
                    }),

                Element<Button>().Class(StyleClassVoteActionButton).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = voteButtonHover,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorder,
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 5,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 5,
                    }),

                Element<Button>().Class(StyleClassVoteActionButton).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = voteButtonPressed,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorder,
                        ContentMarginLeftOverride = 8,
                        ContentMarginTopOverride = 5,
                        ContentMarginRightOverride = 8,
                        ContentMarginBottomOverride = 5,
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassVoteActionButton }, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                        new StyleProperty(Label.StylePropertyFontColor, themedText),
                    }),

                Element<Button>().Class(StyleClassVoteCreateButton)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = DropdownButtonColorContext,
                        BorderThickness = new Thickness(1),
                        BorderColor = UiButtonBorder,
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 4,
                    }),

                Element<Button>().Class(StyleClassVoteCreateButton).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = DropdownButtonColorContextHover,
                        BorderThickness = new Thickness(1),
                        BorderColor = UiButtonBorder,
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 4,
                    }),

                Element<Button>().Class(StyleClassVoteCreateButton).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = DropdownButtonColorContextPressed,
                        BorderThickness = new Thickness(1),
                        BorderColor = UiButtonBorder,
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 4,
                    }),

                Element<Button>().Class(StyleClassVoteCreateButton).Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = DropdownButtonColorContextDisabled,
                        BorderThickness = new Thickness(1),
                        BorderColor = UiButtonBorder,
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 4,
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassVoteCreateButton }, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, exo2Bold13),
                        new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#C5CED8")),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassVoteCreateButton }, null, new[] { ContainerButton.StylePseudoClassHover }),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, exo2Bold13),
                        new StyleProperty(Label.StylePropertyFontColor, Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassVoteCreateButton }, null, new[] { ContainerButton.StylePseudoClassPressed }),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, exo2Bold13),
                        new StyleProperty(Label.StylePropertyFontColor, Color.White),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleClassVoteCreateButton }, null, new[] { ContainerButton.StylePseudoClassDisabled }),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, exo2Bold13),
                        new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#C5CED8").WithAlpha(0.72f)),
                    }),

                Element<Button>().Class(StyleBase.StyleClassVoteButton)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = themedPanelRaised,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorderSoft,
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 4,
                    }),

                Element<Button>().Class(StyleBase.StyleClassVoteButton).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = voteButtonHover,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorder,
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 4,
                    }),

                Element<Button>().Class(StyleBase.StyleClassVoteButton).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Button.StylePropertyStyleBox, new StyleBoxFlat
                    {
                        BackgroundColor = voteButtonPressed,
                        BorderThickness = new Thickness(1),
                        BorderColor = themedBorder,
                        ContentMarginLeftOverride = 6,
                        ContentMarginTopOverride = 4,
                        ContentMarginRightOverride = 6,
                        ContentMarginBottomOverride = 4,
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleBase.StyleClassVoteButton }, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, notoSansBold12),
                        new StyleProperty(Label.StylePropertyFontColor, themedTextMuted),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleBase.StyleClassVoteButton }, null, new[] { ContainerButton.StylePseudoClassHover }),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, themedText),
                    }),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] { StyleBase.StyleClassVoteButton }, null, new[] { ContainerButton.StylePseudoClassPressed }),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFontColor, themedText),
                    }),

                Element<Label>().Class("VoteTitleText")
                    .Prop(Label.StylePropertyFont, notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, themedText),

                Element<RichTextLabel>().Class("VoteTitleText")
                    .Prop("font", notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, themedText),

                Element<Label>().Class("VoteCallerText")
                    .Prop(Label.StylePropertyFont, notoSansBold12)
                    .Prop(Label.StylePropertyFontColor, themedTextMuted),

                Element<Label>().Class("VoteMenuTitle")
                    .Prop(Label.StylePropertyFont, notoSansBold14)
                    .Prop(Label.StylePropertyFontColor, themedText),

                Element<PanelContainer>().Class("VoteMenuDivider")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = themedBorderSoft,
                        ContentMarginLeftOverride = 2,
                        ContentMarginBottomOverride = 2,
                    }),

                Element<TextureButton>().Class("VoteMenuCloseButton")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Nano/cross.svg.png"))
                    .Prop(Control.StylePropertyModulateSelf, themedTextMuted),

                Element<TextureButton>().Class("VoteMenuCloseButton").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, themedText),

                Element<TextureButton>().Class("VoteMenuCloseButton").Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, themedTextMuted.WithAlpha(0.75f)),

                new StyleRule(new SelectorElement(typeof(ProgressBar), new[] { StyleClassVoteProgressBar }, null, null), new[]
                {
                    new StyleProperty(ProgressBar.StylePropertyBackground, new StyleBoxFlat(voteProgressBackground)),
                    new StyleProperty(ProgressBar.StylePropertyForeground, new StyleBoxFlat(voteProgressForeground)),
                }),

                Element<Label>().Class(StyleClassVoteTimerText)
                    .Prop(Label.StylePropertyFont, notoSansBold12)
                    .Prop(Label.StylePropertyFontColor, themedTextMuted),
            }).ToList());
        }
    }
}

// # CCM priority rework


