// CM14 rework: non-RMC edit marker.
using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Actions.Widgets;
using Content.Client.UserInterface.Systems.Alerts.Widgets;
using Content.Client.UserInterface.Systems.Chat.Widgets;
using Content.Client.UserInterface.Systems.Ghost.Widgets;
using Content.Client.UserInterface.Systems.Hotbar.Widgets;
using Content.Client.UserInterface.Systems.Inventory.Widgets;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.UserInterface.Screens;

public sealed partial class SeparatedChatGameScreen : InGameScreen
{
    protected SplitContainer ScreenContainer = default!;
    protected LayoutContainer ViewportContainer = default!;
    protected MainViewport MainViewport = default!;
    protected GhostGui Ghost = default!;
    protected InventoryGui Inventory = default!;
    protected HotbarGui Hotbar = default!;
    protected BoxContainer TopLeftContainer = default!;
    protected ActionsBar Actions = default!;
    public BoxContainer VoteMenu = default!;
    protected AlertsUI Alerts = default!;
    protected PanelContainer SeparatedChatPanel = default!;
    protected GameTopMenuBar TopBar = default!;
    protected ChatBox Chat = default!;

    public SeparatedChatGameScreen()
    {
        try
        {
            RobustXamlLoader.Load(this);
            BindLoadedControls();
        }
        catch (Exception)
        {
            BuildUi();
        }

        AutoscaleMaxResolution = new Vector2i(1080, 770);

        SetAnchorPreset(ScreenContainer, LayoutPreset.Wide);
        SetAnchorPreset(ViewportContainer, LayoutPreset.Wide);
        SetAnchorPreset(MainViewport, LayoutPreset.Wide);
        SetAnchorAndMarginPreset(Inventory, LayoutPreset.BottomLeft, margin: 5);
        SetAnchorAndMarginPreset(TopLeftContainer, LayoutPreset.TopLeft, margin: 10);
        SetAnchorAndMarginPreset(VoteMenu, LayoutPreset.CenterTop, margin: 10);
        SetPosition(VoteMenu, new Vector2(-180, 10));
        SetAnchorAndMarginPreset(Ghost, LayoutPreset.BottomWide, margin: 80);
        SetAnchorAndMarginPreset(Hotbar, LayoutPreset.BottomWide, margin: 5);
        SetAnchorAndMarginPreset(Alerts, LayoutPreset.CenterRight, margin: 10);

        // RMC14
        SetGrowVertical(Alerts, GrowDirection.Both);

        ScreenContainer.OnSplitResizeFinished += () =>
            OnChatResized?.Invoke(new Vector2(ScreenContainer.SplitFraction, 0));

        ViewportContainer.OnResized += ResizeActionContainer;
    }

    private void BindLoadedControls()
    {
        ScreenContainer = FindControl<SplitContainer>("ScreenContainer");
        ViewportContainer = FindControl<LayoutContainer>("ViewportContainer");
        MainViewport = FindControl<MainViewport>("MainViewport");
        Ghost = FindControl<GhostGui>("Ghost");
        Inventory = FindControl<InventoryGui>("Inventory");
        Hotbar = FindControl<HotbarGui>("Hotbar");
        TopLeftContainer = FindControl<BoxContainer>("TopLeftContainer");
        Actions = FindControl<ActionsBar>("Actions");
        VoteMenu = FindControl<BoxContainer>("VoteMenu");
        Alerts = FindControl<AlertsUI>("Alerts");
        SeparatedChatPanel = FindControl<PanelContainer>("SeparatedChatPanel");
        TopBar = FindControl<GameTopMenuBar>("TopBar");
        Chat = FindControl<ChatBox>("Chat");
    }

    private void BuildUi()
    {
        var root = new LayoutContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        AddChild(root);

        ScreenContainer = new SplitContainer
        {
            Name = "ScreenContainer",
            HorizontalExpand = true,
            VerticalExpand = true,
            SplitWidth = 0,
            StretchDirection = SplitContainer.SplitStretchDirection.TopLeft,
        };

        ViewportContainer = new LayoutContainer
        {
            Name = "ViewportContainer",
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        MainViewport = new MainViewport
        {
            Name = "MainViewport",
        };
        ViewportContainer.AddChild(MainViewport);

        Ghost = new GhostGui
        {
            Name = "Ghost",
        };
        ViewportContainer.AddChild(Ghost);

        Inventory = new InventoryGui
        {
            Name = "Inventory",
        };
        ViewportContainer.AddChild(Inventory);

        Hotbar = new HotbarGui
        {
            Name = "Hotbar",
        };
        ViewportContainer.AddChild(Hotbar);

        TopLeftContainer = new BoxContainer
        {
            Name = "TopLeftContainer",
            Orientation = BoxContainer.LayoutOrientation.Vertical,
        };
        Actions = new ActionsBar
        {
            Name = "Actions",
        };
        TopLeftContainer.AddChild(Actions);
        ViewportContainer.AddChild(TopLeftContainer);

        Alerts = new AlertsUI
        {
            Name = "Alerts",
        };
        ViewportContainer.AddChild(Alerts);

        SeparatedChatPanel = new PanelContainer
        {
            Name = "SeparatedChatPanel",
            MinWidth = 300,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#202020"),
            },
        };

        var chatPanelBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            SeparationOverride = 10,
            Margin = new Thickness(10),
        };

        TopBar = new GameTopMenuBar
        {
            Name = "TopBar",
            HorizontalExpand = true,
        };
        chatPanelBox.AddChild(TopBar);

        Chat = new ChatBox
        {
            Name = "Chat",
            HorizontalExpand = true,
            VerticalExpand = true,
            MinSize = new Vector2(0, 0),
        };
        chatPanelBox.AddChild(Chat);
        SeparatedChatPanel.AddChild(chatPanelBox);

        ScreenContainer.AddChild(ViewportContainer);
        ScreenContainer.AddChild(SeparatedChatPanel);
        root.AddChild(ScreenContainer);

        VoteMenu = new BoxContainer
        {
            Name = "VoteMenu",
            Margin = new Thickness(0, 10, 0, 0),
            Orientation = BoxContainer.LayoutOrientation.Vertical,
        };
        root.AddChild(VoteMenu);
    }

    private void ResizeActionContainer()
    {
        float indent = 20;
        Actions.ActionsContainer.MaxGridWidth = ViewportContainer.Size.X - indent;
    }

    public override ChatBox ChatBox => Chat;

    public override void SetChatSize(Vector2 size)
    {
        ScreenContainer.ResizeMode = SplitContainer.SplitResizeMode.RespectChildrenMinSize;
    }
}
