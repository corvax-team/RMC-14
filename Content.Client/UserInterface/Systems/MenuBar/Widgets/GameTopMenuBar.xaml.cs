// CM14 rework: non-RMC edit marker.
using System.Numerics;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.Input;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.IoC;

namespace Content.Client.UserInterface.Systems.MenuBar.Widgets;

public sealed partial class GameTopMenuBar : UIWidget
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    private const float TopButtonSize = 68f;

    public MenuButton EscapeButton { get; }
    public MenuButton? GuidebookButton { get; }
    public MenuButton? CharacterButton { get; }
    public MenuButton EmotesButton { get; }
    public MenuButton CraftingButton { get; }
    public MenuButton ActionButton { get; }
    public MenuButton AdminButton { get; }
    public MenuButton SandboxButton { get; }
    public MenuButton AHelpButton { get; }

    public GameTopMenuBar()
    {
        IoCManager.InjectDependencies(this);

        Name = "MenuButtons";
        Orientation = LayoutOrientation.Horizontal;
        VerticalExpand = false;
        HorizontalAlignment = HAlignment.Stretch;
        VerticalAlignment = VAlignment.Top;
        SeparationOverride = 5;
        MinSize = new Vector2(0f, TopButtonSize);

        EscapeButton = CreateButton(
            "EscapeButton",
            "/Textures/Interface/hamburger.svg.192dpi.png",
            EngineKeyFunctions.EscapeMenu,
            "game-hud-open-escape-menu-button-tooltip",
            TopButtonSize,
            StyleBase.ButtonSquare);
        EmotesButton = CreateButton(
            "EmotesButton",
            "/Textures/Interface/emotes.svg.192dpi.png",
            ContentKeyFunctions.OpenEmotesMenu,
            "game-hud-open-emotes-menu-button-tooltip",
            TopButtonSize,
            StyleBase.ButtonSquare);
        CraftingButton = CreateButton(
            "CraftingButton",
            "/Textures/Interface/hammer.svg.192dpi.png",
            ContentKeyFunctions.OpenCraftingMenu,
            "game-hud-open-crafting-menu-button-tooltip",
            TopButtonSize,
            StyleBase.ButtonSquare);
        ActionButton = CreateButton(
            "ActionButton",
            "/Textures/Interface/fist.svg.192dpi.png",
            ContentKeyFunctions.OpenActionsMenu,
            "game-hud-open-actions-menu-button-tooltip",
            TopButtonSize,
            StyleBase.ButtonSquare);
        AdminButton = CreateButton(
            "AdminButton",
            "/Textures/Interface/gavel.svg.192dpi.png",
            ContentKeyFunctions.OpenAdminMenu,
            "game-hud-open-admin-menu-button-tooltip",
            TopButtonSize,
            StyleBase.ButtonSquare);
        SandboxButton = CreateButton(
            "SandboxButton",
            "/Textures/Interface/sandbox.svg.192dpi.png",
            ContentKeyFunctions.OpenSandboxWindow,
            "game-hud-open-sandbox-menu-button-tooltip",
            TopButtonSize,
            StyleBase.ButtonSquare);
        AHelpButton = CreateButton(
            "AHelpButton",
            "/Textures/Interface/info.svg.192dpi.png",
            ContentKeyFunctions.OpenAHelp,
            "ui-options-function-open-a-help",
            TopButtonSize,
            StyleBase.ButtonSquare);
    }

    private MenuButton CreateButton(
        string name,
        string texturePath,
        BoundKeyFunction boundKey,
        string tooltipLocKey,
        float minWidth,
        string styleClass)
    {
        var button = new MenuButton
        {
            Name = name,
            Icon = _resourceCache.GetTexture(texturePath),
            BoundKey = boundKey,
            ToolTip = Loc.GetString(tooltipLocKey),
            MinSize = new Vector2(minWidth, TopButtonSize),
            MaxSize = new Vector2(minWidth, TopButtonSize),
            SetSize = new Vector2(minWidth, TopButtonSize),
            HorizontalExpand = false,
            VerticalExpand = false,
            AppendStyleClass = styleClass,
        };

        AddChild(button);
        return button;
    }
}
