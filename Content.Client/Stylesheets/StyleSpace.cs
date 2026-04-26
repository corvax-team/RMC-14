using System;
using System.Linq;
using Content.Client.Resources;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Robust.Client.UserInterface.StylesheetHelpers;

namespace Content.Client.Stylesheets
{
    public sealed class StyleSpace : StyleBase
    {
        public static readonly Color SpaceRed = Color.FromHex("#9b2236");

        public static readonly Color ButtonColorDefault = Color.FromHex("#464966");
        public static readonly Color ButtonColorHovered = Color.FromHex("#575b7f");
        public static readonly Color ButtonColorPressed = Color.FromHex("#3e6c45");
        public static readonly Color ButtonColorDisabled = Color.FromHex("#30313c");

        public static readonly Color ButtonColorCautionDefault = Color.FromHex("#ab3232");
        public static readonly Color ButtonColorCautionHovered = Color.FromHex("#cf2f2f");
        public static readonly Color ButtonColorCautionPressed = Color.FromHex("#3e6c45");
        public static readonly Color ButtonColorCautionDisabled = Color.FromHex("#602a2a");

        public override Stylesheet Stylesheet { get; }

        public StyleSpace(IResourceCache resCache, string theme = "blue") : base(resCache)
        {
            var isBlueTheme = theme.Equals("blue", StringComparison.OrdinalIgnoreCase);

            var launcherFrameBackground = (isBlueTheme
                ? Color.FromHex("#070E1B")
                : Color.FromHex("#0B150D")).WithAlpha(0.96f);
            var launcherFrameBorder = (isBlueTheme
                ? Color.FromHex("#1C5CA8")
                : Color.FromHex("#2B7E45")).WithAlpha(0.98f);
            var launcherDivider = Color.FromHex("#545A66");
            var launcherTitleColor = isBlueTheme
                ? Color.FromHex("#58A6FF")
                : Color.FromHex("#95E7B1");
            var launcherStateColor = isBlueTheme
                ? Color.FromHex("#C4DAF8")
                : Color.FromHex("#D1EBDD");
            var launcherButtonNormal = isBlueTheme
                ? Color.FromHex("#1B2F56")
                : Color.FromHex("#2B4433");
            var launcherButtonHover = isBlueTheme
                ? Color.FromHex("#23406F")
                : Color.FromHex("#365B43");
            var launcherButtonPressed = isBlueTheme
                ? Color.FromHex("#152544")
                : Color.FromHex("#1F3528");
            var launcherButtonText = isBlueTheme
                ? Color.FromHex("#D5E4F9")
                : Color.FromHex("#D8F0E2");

            var notoSans10 = resCache.GetFont
            (
                new []
                {
                    "/Fonts/Exo2/Exo2-Regular.ttf",
                    "/Fonts/Exo2/Exo2-Regular.ttf",
                    "/Fonts/Exo2/Exo2-Regular.ttf"
                },
                10
            );
            var notoSansBold16 = resCache.GetFont
            (
                new []
                {
                    "/Fonts/Exo2/Exo2-Regular.ttf",
                    "/Fonts/Exo2/Exo2-Regular.ttf",
                    "/Fonts/Exo2/Exo2-Regular.ttf"
                },
                16
            );
            var exo2Regular12 = resCache.GetFont("/Fonts/Exo2/Exo2-Regular.ttf", 12);
            var exo2Bold12 = resCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 12);
            var bedstead12 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 12);
            var bedstead13 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 13);
            var bedstead15 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 15);
            var bedstead20 = resCache.GetFont("/Fonts/Bedstead/bedstead.otf", 20);
            var exo2Bold14 = resCache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 14);

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

            var textureInvertedTriangle = resCache.GetTexture("/Textures/Interface/Nano/inverted_triangle.svg.png");

            var tabContainerPanel = new StyleBoxFlat
            {
                BackgroundColor = StyleNano.PanelDark.WithAlpha(0.95f),
            };

            var tabContainerBoxActive = new StyleBoxFlat {BackgroundColor = StyleNano.PanelDark.WithAlpha(0.98f)};
            tabContainerBoxActive.SetContentMarginOverride(StyleBox.Margin.Horizontal, 5);
            var tabContainerBoxInactive = new StyleBoxFlat {BackgroundColor = StyleNano.PanelDark.WithAlpha(0.9f)};
            tabContainerBoxInactive.SetContentMarginOverride(StyleBox.Margin.Horizontal, 5);

            var voteButtonBox = new StyleBoxTexture(BaseAngleRect);
            voteButtonBox.SetPadding(StyleBox.Margin.All, 1);
            voteButtonBox.SetContentMarginOverride(StyleBox.Margin.Vertical, 2);
            voteButtonBox.SetContentMarginOverride(StyleBox.Margin.Horizontal, 12);
            var voteButtonBase = StyleNano.ButtonColorDefault;

            Stylesheet = new Stylesheet(BaseRules.Concat(new StyleRule[]
            {
                Element<Label>().Class(StyleClassLabelHeading)
                    .Prop(Label.StylePropertyFont, notoSansBold16)
                    .Prop(Label.StylePropertyFontColor, SpaceRed),

                Element<Label>().Class(StyleClassLabelSubText)
                    .Prop(Label.StylePropertyFont, notoSans10)
                    .Prop(Label.StylePropertyFontColor, Color.DarkGray),

                Element<PanelContainer>().Class(ClassHighDivider)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = SpaceRed, ContentMarginBottomOverride = 2, ContentMarginLeftOverride = 2
                    }),

                Element<PanelContainer>().Class(ClassLowDivider)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = Color.FromHex("#444"),
                        ContentMarginLeftOverride = 2,
                        ContentMarginBottomOverride = 2
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


                Element<Label>().Class(ContainerButton.StyleClassButton)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center),

                Element<PanelContainer>().Class(ClassAngleRect)
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = StyleNano.PanelDark.WithAlpha(0.95f),
                        BorderThickness = new Thickness(1),
                        BorderColor = StyleNano.PanelDark.WithAlpha(1f),
                    }),

                Element<PanelContainer>().Class("LauncherConnectingFrame")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = launcherFrameBackground,
                        BorderThickness = new Thickness(1),
                        BorderColor = launcherFrameBorder,
                    }),

                Element<PanelContainer>().Class("LauncherConnectingDivider")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = launcherDivider,
                        ContentMarginLeftOverride = 2,
                        ContentMarginBottomOverride = 1,
                    }),

                Element<Label>().Class("LauncherConnectingTitle")
                    .Prop(Label.StylePropertyFont, bedstead15)
                    .Prop(Label.StylePropertyFontColor, launcherTitleColor),

                Element<Label>().Class("LauncherConnectingStateLabel")
                    .Prop(Label.StylePropertyFont, bedstead13)
                    .Prop(Label.StylePropertyFontColor, launcherStateColor),

                Element<Label>().Class("LauncherConnectingReasonLabel")
                    .Prop(Label.StylePropertyFont, bedstead13)
                    .Prop(Label.StylePropertyFontColor, launcherStateColor),

                Element<RichTextLabel>().Class("LauncherConnectingReasonLabel")
                    .Prop(Label.StylePropertyFont, bedstead13),

                Element<Label>().Class("LauncherConnectingReasonSmallLabel")
                    .Prop(Label.StylePropertyFont, bedstead12)
                    .Prop(Label.StylePropertyFontColor, launcherStateColor),

                Element<RichTextLabel>().Class("LauncherConnectingReasonSmallLabel")
                    .Prop(Label.StylePropertyFont, bedstead12),

                Element<Button>().Class("LauncherConnectingButton")
                    .Prop(Control.StylePropertyModulateSelf, launcherButtonNormal),

                Element<Button>().Class("LauncherConnectingButton").Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, launcherButtonNormal),

                Element<Button>().Class("LauncherConnectingButton").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, launcherButtonHover),

                Element<Button>().Class("LauncherConnectingButton").Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, launcherButtonPressed),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(Button), new[] {"LauncherConnectingButton"}, null, null),
                    new SelectorElement(typeof(Label), null, null, null)),
                    new[]
                    {
                        new StyleProperty(Label.StylePropertyFont, bedstead15),
                        new StyleProperty(Label.StylePropertyFontColor, launcherButtonText),
                    }),

                Element<PanelContainer>().Class("VerticalTabListBackground")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = StyleNano.PanelDark.WithAlpha(0.6f),
                        BorderThickness = new Thickness(2, 0, 0, 0),
                        BorderColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.6f),
                    }),

                Element<PanelContainer>().Class("VerticalTabContentBackground")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat(StyleNano.PanelDark.WithAlpha(0.95f))),

                Child()
                    .Parent(Element<Button>().Class(ContainerButton.StylePseudoClassDisabled))
                    .Child(Element<Label>())
                    .Prop("font-color", Color.FromHex("#E5E5E581")),

                Element<ProgressBar>()
                    .Prop(ProgressBar.StylePropertyBackground, progressBarBackground)
                    .Prop(ProgressBar.StylePropertyForeground, progressBarForeground),

                // OptionButton
                Element<OptionButton>()
                    .Prop(ContainerButton.StylePropertyStyleBox, BaseButton),

                Element<OptionButton>().Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorDefault),

                Element<OptionButton>().Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorHovered),

                Element<OptionButton>().Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorPressed),

                Element<OptionButton>().Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, ButtonColorDisabled),

                Element<Button>().Class(StyleClassVoteButton)
                    .Prop(Button.StylePropertyStyleBox, voteButtonBox),

                Element<Button>().Class(StyleClassVoteButton).Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, StyleNano.LobbyMenuButtonBase * new Color(0.48f, 0.48f, 0.48f, 1f)),

                Element<Button>().Class(StyleClassVoteButton).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, StyleNano.LobbyMenuButtonPressed * new Color(0.58f, 0.58f, 0.58f, 1f)),

                Element<Button>().Class(StyleClassVoteButton).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, StyleNano.LobbyMenuButtonPressed * new Color(0.44f, 0.44f, 0.44f, 1f)),

                Element<Button>().Class(StyleClassVoteButton).Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, StyleNano.LobbyMenuButtonDisabledCrt * new Color(0.60f, 0.60f, 0.60f, 1f)),

                Element<OptionButton>().Class(StyleClassVoteButton)
                    .Prop(ContainerButton.StylePropertyStyleBox, voteButtonBox),

                Element<OptionButton>().Class(StyleClassVoteButton).Pseudo(ContainerButton.StylePseudoClassNormal)
                    .Prop(Control.StylePropertyModulateSelf, StyleNano.LobbyMenuButtonBase * new Color(0.48f, 0.48f, 0.48f, 1f)),

                Element<OptionButton>().Class(StyleClassVoteButton).Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, StyleNano.LobbyMenuButtonPressed * new Color(0.58f, 0.58f, 0.58f, 1f)),

                Element<OptionButton>().Class(StyleClassVoteButton).Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, StyleNano.LobbyMenuButtonPressed * new Color(0.44f, 0.44f, 0.44f, 1f)),

                Element<OptionButton>().Class(StyleClassVoteButton).Pseudo(ContainerButton.StylePseudoClassDisabled)
                    .Prop(Control.StylePropertyModulateSelf, StyleNano.LobbyMenuButtonDisabledCrt * new Color(0.60f, 0.60f, 0.60f, 1f)),

                  new StyleRule(new SelectorChild(
                      new SelectorElement(typeof(Button), new[] {StyleClassVoteButton}, null, null),
                      new SelectorElement(typeof(Label), null, null, null)),
                      new[]
                      {
                          new StyleProperty(Label.StylePropertyFont, exo2Bold12),
                          new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#D9DDE3")),
                      }),
                  new StyleRule(new SelectorChild(
                      new SelectorElement(typeof(Button), new[] {StyleClassVoteButton}, null, new[] {ContainerButton.StylePseudoClassHover}),
                      new SelectorElement(typeof(Label), null, null, null)),
                      new[]
                      {
                          new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#D9DDE3")),
                      }),

                  new StyleRule(new SelectorChild(
                      new SelectorElement(typeof(OptionButton), new[] {StyleClassVoteButton}, null, null),
                      new SelectorElement(typeof(Label), null, null, null)),
                      new[]
                      {
                          new StyleProperty(Label.StylePropertyFont, exo2Bold12),
                          new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#D9DDE3")),
                      }),
                  new StyleRule(new SelectorChild(
                      new SelectorElement(typeof(OptionButton), new[] {StyleClassVoteButton}, null, new[] {ContainerButton.StylePseudoClassHover}),
                      new SelectorElement(typeof(Label), null, null, null)),
                      new[]
                      {
                          new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#D9DDE3")),
                      }),

                  new StyleRule(new SelectorChild(
                      new SelectorElement(typeof(OptionButton), new[] {StyleClassVoteButton}, null, null),
                      new SelectorElement(typeof(Label), new[] {OptionButton.StyleClassOptionButton}, null, null)),
                      new[]
                      {
                          new StyleProperty(Label.StylePropertyFont, exo2Bold12),
                          new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#D9DDE3")),
                      }),
                  new StyleRule(new SelectorChild(
                      new SelectorElement(typeof(OptionButton), new[] {StyleClassVoteButton}, null, new[] {ContainerButton.StylePseudoClassHover}),
                      new SelectorElement(typeof(Label), new[] {OptionButton.StyleClassOptionButton}, null, null)),
                      new[]
                      {
                          new StyleProperty(Label.StylePropertyFontColor, Color.FromHex("#D9DDE3")),
                      }),

                Element<TextureRect>().Class(OptionButton.StyleClassOptionTriangle)
                    .Prop(TextureRect.StylePropertyTexture, textureInvertedTriangle),

                new StyleRule(new SelectorChild(
                    new SelectorElement(typeof(OptionButton), new[] {StyleClassVoteButton}, null, null),
                    new SelectorElement(typeof(TextureRect), new[] {OptionButton.StyleClassOptionTriangle}, null, null)),
                    new[]
                    {
                        new StyleProperty(Control.StylePropertyModulateSelf, Color.FromHex("#D9DDE3")),
                    }),

                Element<Label>().Class(OptionButton.StyleClassOptionButton)
                    .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center),

                Element<Label>().Class("VoteTitleText")
                    .Prop(Label.StylePropertyFont, exo2Bold14)
                    .Prop(Label.StylePropertyFontColor, Color.FromHex("#D9DDE3")),

                Element<RichTextLabel>().Class("VoteTitleText")
                    .Prop("font", exo2Bold14)
                    .Prop(Label.StylePropertyFontColor, Color.FromHex("#D9DDE3")),

                Element<Label>().Class("VoteCallerText")
                    .Prop(Label.StylePropertyFont, exo2Bold14)
                    .Prop(Label.StylePropertyFontColor, Color.FromHex("#D9DDE3")),

                Element<Label>().Class("VoteMenuTitle")
                    .Prop(Label.StylePropertyFont, exo2Bold14)
                    .Prop(Label.StylePropertyFontColor, StyleNano.LobbyMenuButtonBase),

                Element<PanelContainer>().Class("VoteMenuDivider")
                    .Prop(PanelContainer.StylePropertyPanel, new StyleBoxFlat
                    {
                        BackgroundColor = StyleNano.LobbyMenuButtonBase.WithAlpha(0.95f),
                        ContentMarginLeftOverride = 2,
                        ContentMarginBottomOverride = 2,
                    }),

                Element<TextureButton>().Class("VoteMenuCloseButton")
                    .Prop(TextureButton.StylePropertyTexture, resCache.GetTexture("/Textures/Interface/Nano/cross.svg.png"))
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#A9AFB8")),

                Element<TextureButton>().Class("VoteMenuCloseButton").Pseudo(ContainerButton.StylePseudoClassHover)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#C2C7CE")),

                Element<TextureButton>().Class("VoteMenuCloseButton").Pseudo(ContainerButton.StylePseudoClassPressed)
                    .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#8F959E")),

                // TabContainer
                new StyleRule(new SelectorElement(typeof(TabContainer), null, null, null),
                    new[]
                    {
                        new StyleProperty(TabContainer.StylePropertyPanelStyleBox, tabContainerPanel),
                        new StyleProperty(TabContainer.StylePropertyTabStyleBox, tabContainerBoxActive),
                        new StyleProperty(TabContainer.StylePropertyTabStyleBoxInactive, tabContainerBoxInactive),
                    }),

            }).ToList());
        }
    }
}
// # CCM priority rework


