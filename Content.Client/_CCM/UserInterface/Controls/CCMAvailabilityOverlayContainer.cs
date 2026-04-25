// CM14 rework: non-RMC edit marker.
using System.Numerics;
using Robust.Client.UserInterface;

namespace Content.Client._CCM.UserInterface.Controls;

/// <summary>
/// Minimal container that layers an overlay over content while measuring/arranging
/// based on the content's size (unlike LayoutContainer anchors).
/// </summary>
public sealed class CCMAvailabilityOverlayContainer : Robust.Client.UserInterface.Control
{
    public Robust.Client.UserInterface.Control Content { get; }
    public Robust.Client.UserInterface.Control Overlay { get; }

    public CCMAvailabilityOverlayContainer(Robust.Client.UserInterface.Control content, Robust.Client.UserInterface.Control overlay)
    {
        Content = content;
        Overlay = overlay;

        AddChild(Content);
        AddChild(Overlay);
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        Content.Measure(availableSize);
        Overlay.Measure(availableSize);
        return Content.DesiredSize;
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        var box = UIBox2.FromDimensions(0, 0, finalSize.X, finalSize.Y);
        Content.Arrange(box);
        Overlay.Arrange(box);
        return finalSize;
    }
}
