using System;
using System.Linq;
using System.Numerics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client.UserInterface.Systems.Ghost.Controls;

public sealed class UniformWidthContainer : Container
{
    public int? SeparationOverride { get; set; }

    private int ActualSeparation => SeparationOverride ?? 0;

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var visibleCount = Children.Count(child => child.Visible || child.ReservesSpace);
        if (visibleCount == 0)
            return Vector2.Zero;

        var separation = ActualSeparation * (visibleCount - 1);
        var availableWidth = float.IsFinite(availableSize.X)
            ? availableSize.X
            : 0;
        var childWidth = Math.Max(0, (availableWidth - separation) / visibleCount);
        var childSize = new Vector2(childWidth, availableSize.Y);
        var maxHeight = 0f;
        var desiredWidth = (float) separation;

        foreach (var child in Children)
        {
            if (!child.Visible && !child.ReservesSpace)
                continue;

            child.Measure(childSize);
            maxHeight = Math.Max(maxHeight, child.DesiredSize.Y);
            desiredWidth += child.DesiredSize.X;
        }

        return new Vector2(float.IsFinite(availableSize.X) ? availableSize.X : desiredWidth, maxHeight);
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        var visibleCount = Children.Count(child => child.Visible || child.ReservesSpace);
        if (visibleCount == 0)
            return finalSize;

        var separation = ActualSeparation * (visibleCount - 1);
        var childWidth = Math.Max(0, (finalSize.X - separation) / visibleCount);
        var offset = 0f;

        foreach (var child in Children)
        {
            if (!child.Visible && !child.ReservesSpace)
                continue;

            child.Arrange(UIBox2.FromDimensions(offset, 0, childWidth, finalSize.Y));
            offset += childWidth + ActualSeparation;
        }

        return finalSize;
    }
}
