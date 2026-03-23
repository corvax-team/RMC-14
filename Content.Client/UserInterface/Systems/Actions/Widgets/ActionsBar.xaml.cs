using System.Numerics;
using Content.Client.UserInterface.Systems.Actions.Controls;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Systems.Actions.Widgets;

public sealed partial class ActionsBar : UIWidget
{
    public ActionButtonContainer ActionsContainer { get; }

    public ActionsBar()
    {
        VerticalExpand = false;
        Orientation = LayoutOrientation.Horizontal;
        HorizontalExpand = false;

        var root = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
        };

        ActionsContainer = new ActionButtonContainer
        {
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            MaxSize = new Vector2(64, float.MaxValue),
            Rows = 1,
        };

        root.AddChild(ActionsContainer);
        AddChild(root);
    }
}
