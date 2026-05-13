using System.Collections.Generic;
using System.Numerics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._CMU14.Medical.UI;

internal static class CMUMedicalWindowSizing
{
    private static readonly Vector2 WindowMargin = new(64f, 80f);
    private static readonly Vector2 AbsoluteFloor = new(360f, 300f);
    private static readonly Vector2 VisibleWindowSlice = new(96f, 64f);
    private static readonly Vector2 OffscreenBump = new(48f, 48f);
    private static readonly Dictionary<string, Vector2> RememberedSizes = new();

    public static Vector2 GetInitialSize(string key, Vector2 preferredSize)
    {
        return RememberedSizes.TryGetValue(key, out var size) && IsValidSize(size)
            ? size
            : preferredSize;
    }

    public static void FitToScreen(BaseWindow window, Vector2 preferredSize, Vector2 minimumSize)
    {
        var rootSize = window.UserInterfaceManager.WindowRoot.Size;
        if (rootSize.X <= 0f || rootSize.Y <= 0f)
            return;

        var maxSize = Vector2.Min(rootSize, Vector2.Max(AbsoluteFloor, rootSize - WindowMargin));
        var minSize = Vector2.Min(minimumSize, maxSize);
        var current = ResolveSetSize(window.SetSize, preferredSize);
        var target = Vector2.Max(minSize, Vector2.Min(current, maxSize));

        if (window.MinSize != minSize)
            window.MinSize = minSize;
        if (window.MaxSize != maxSize)
            window.MaxSize = maxSize;
        if (window.SetSize != target)
            window.SetSize = target;

        if (window.Parent is not { } parent)
            return;

        var visibleSlice = Vector2.Min(target, Vector2.Min(parent.Size, VisibleWindowSlice));
        var minPosition = visibleSlice - target;
        var maxPosition = parent.Size - visibleSlice;

        if (window.Position.X + target.X < 0f)
            minPosition.X = MathF.Min(maxPosition.X, minPosition.X + OffscreenBump.X);
        else if (window.Position.X > parent.Size.X)
            maxPosition.X = MathF.Max(minPosition.X, maxPosition.X - OffscreenBump.X);

        if (window.Position.Y + target.Y < 0f)
            minPosition.Y = MathF.Min(maxPosition.Y, minPosition.Y + OffscreenBump.Y);
        else if (window.Position.Y > parent.Size.Y)
            maxPosition.Y = MathF.Max(minPosition.Y, maxPosition.Y - OffscreenBump.Y);

        var position = Vector2.Max(minPosition, Vector2.Min(window.Position, maxPosition));
        if (window.Position != position)
            LayoutContainer.SetPosition(window, position);
    }

    public static void RememberSize(string key, BaseWindow window)
    {
        var size = window.SetSize;
        if (!IsValidSize(size))
            size = window.Size;

        if (!IsValidSize(size))
            return;

        RememberedSizes[key] = size;
    }

    private static Vector2 ResolveSetSize(Vector2 current, Vector2 fallback)
    {
        return new Vector2(
            float.IsNaN(current.X) ? fallback.X : current.X,
            float.IsNaN(current.Y) ? fallback.Y : current.Y);
    }

    private static bool IsValidSize(Vector2 size)
    {
        return size.X > 0f &&
            size.Y > 0f &&
            !float.IsNaN(size.X) &&
            !float.IsNaN(size.Y) &&
            !float.IsInfinity(size.X) &&
            !float.IsInfinity(size.Y);
    }
}
