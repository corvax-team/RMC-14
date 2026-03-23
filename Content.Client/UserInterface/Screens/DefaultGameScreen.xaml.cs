using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Actions.Widgets;
using Content.Client.UserInterface.Systems.Alerts.Widgets;
using Content.Client.UserInterface.Systems.Chat.Widgets;
using Content.Client.UserInterface.Systems.Ghost.Widgets;
using Content.Client.UserInterface.Systems.Hotbar.Widgets;
using Content.Client.UserInterface.Systems.Inventory.Widgets;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.UserInterface.Screens;

public sealed partial class DefaultGameScreen : InGameScreen
{
    protected LayoutContainer ViewportContainer = default!;
    protected MainViewport MainViewport = default!;
    protected BoxContainer TopLeft = default!;
    protected GameTopMenuBar TopBar = default!;
    public BoxContainer VoteMenu = default!;
    protected ActionsBar Actions = default!;
    protected GhostGui Ghost = default!;
    protected InventoryGui Inventory = default!;
    protected HotbarGui Hotbar = default!;
    protected ResizableChatBox Chat = default!;
    protected AlertsUI Alerts = default!;

    public DefaultGameScreen()
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

        SetAnchorPreset(MainViewport, LayoutPreset.Wide);
        SetAnchorPreset(ViewportContainer, LayoutPreset.Wide);
        SetAnchorAndMarginPreset(TopLeft, LayoutPreset.TopLeft, margin: 10);
        SetAnchorAndMarginPreset(Ghost, LayoutPreset.BottomWide, margin: 80);
        SetAnchorAndMarginPreset(Inventory, LayoutPreset.BottomLeft, margin: 5);
        SetAnchorAndMarginPreset(Hotbar, LayoutPreset.BottomWide, margin: 5);
        SetAnchorAndMarginPreset(Chat, LayoutPreset.TopRight, margin: 10);
        SetAnchorAndMarginPreset(Alerts, LayoutPreset.TopRight, margin: 10);

        Chat.OnResized += ChatOnResized;
        Chat.OnChatResizeFinish += ChatOnResizeFinish;

        MainViewport.OnResized += ResizeActionContainer;
        Inventory.OnResized += ResizeActionContainer;
    }

    private void BindLoadedControls()
    {
        ViewportContainer = FindControl<LayoutContainer>("ViewportContainer");
        MainViewport = FindControl<MainViewport>("MainViewport");
        TopLeft = FindControl<BoxContainer>("TopLeft");
        TopBar = FindControl<GameTopMenuBar>("TopBar");
        VoteMenu = FindControl<BoxContainer>("VoteMenu");
        Actions = FindControl<ActionsBar>("Actions");
        Ghost = FindControl<GhostGui>("Ghost");
        Inventory = FindControl<InventoryGui>("Inventory");
        Hotbar = FindControl<HotbarGui>("Hotbar");
        Chat = FindControl<ResizableChatBox>("Chat");
        Alerts = FindControl<AlertsUI>("Alerts");
    }

    private void BuildUi()
    {
        var root = new LayoutContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        AddChild(root);

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
        root.AddChild(ViewportContainer);

        TopLeft = new BoxContainer
        {
            Name = "TopLeft",
            Orientation = BoxContainer.LayoutOrientation.Vertical,
        };

        var topRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
        };

        TopBar = new GameTopMenuBar
        {
            Name = "TopBar",
        };
        topRow.AddChild(TopBar);
        topRow.AddChild(new Control());

        VoteMenu = new BoxContainer
        {
            Name = "VoteMenu",
            Margin = new Thickness(0, 10, 0, 10),
            Orientation = BoxContainer.LayoutOrientation.Vertical,
        };

        Actions = new ActionsBar
        {
            Name = "Actions",
        };

        TopLeft.AddChild(topRow);
        TopLeft.AddChild(VoteMenu);
        TopLeft.AddChild(Actions);
        root.AddChild(TopLeft);

        Ghost = new GhostGui
        {
            Name = "Ghost",
        };
        root.AddChild(Ghost);

        Inventory = new InventoryGui
        {
            Name = "Inventory",
        };
        root.AddChild(Inventory);

        Hotbar = new HotbarGui
        {
            Name = "Hotbar",
        };
        root.AddChild(Hotbar);

        Chat = new ResizableChatBox
        {
            Name = "Chat",
        };
        root.AddChild(Chat);

        Alerts = new AlertsUI
        {
            Name = "Alerts",
        };
        root.AddChild(Alerts);
    }

    private void ResizeActionContainer()
    {
        float indent = Inventory.Size.Y + TopBar.Size.Y + 40;
        Actions.ActionsContainer.MaxGridHeight = MainViewport.Size.Y - indent;
    }

    private void ChatOnResizeFinish(Vector2 _)
    {
        var marginBottom = Chat.GetValue<float>(MarginBottomProperty);
        var marginLeft = Chat.GetValue<float>(MarginLeftProperty);
        OnChatResized?.Invoke(new Vector2(marginBottom, marginLeft));
    }

    private void ChatOnResized()
    {
        var marginBottom = Chat.GetValue<float>(MarginBottomProperty);
        SetMarginTop(Alerts, marginBottom);
    }

    public override ChatBox ChatBox => Chat;

    public override void SetChatSize(Vector2 size)
    {
        SetMarginBottom(Chat, size.X);
        SetMarginLeft(Chat, size.Y);
        SetMarginTop(Alerts, size.X);
    }
}
