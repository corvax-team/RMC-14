using System.Collections.Generic;
using System.Numerics;
using Content.Client.Lobby.UI;
using Content.Client.Lobby;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.Systems.ScanlineOverlay;

public sealed class ScanlineOverlayUIController : UIController, IOnStateEntered<LobbyState>, IOnStateExited<LobbyState>
{
    private readonly Dictionary<Control, (ScanlineOverlayControl Overlay, Action<Control> ChildAddedHandler)> _overlays = new();

    public override void Initialize()
    {
        base.Initialize();

        UIManager.WindowRoot.OnChildAdded += OnWindowRootChildAdded;
        UIManager.WindowRoot.OnChildRemoved += OnWindowRootChildRemoved;

        foreach (var child in UIManager.WindowRoot.Children)
        {
            TryAttachWindow(child);
        }
    }

    public void OnStateEntered(LobbyState state)
    {
        if (UIManager.ActiveScreen is LobbyGui lobbyGui)
            AttachOverlay(lobbyGui);
    }

    public void OnStateExited(LobbyState state)
    {
        if (UIManager.ActiveScreen is LobbyGui lobbyGui)
            DetachOverlay(lobbyGui);
    }

    private void OnWindowRootChildAdded(Control control)
    {
        TryAttachWindow(control);
    }

    private void OnWindowRootChildRemoved(Control control)
    {
        DetachOverlay(control);
    }

    private void TryAttachWindow(Control control)
    {
        if (control is DefaultWindow window)
            AttachOverlay(window);
    }

    private void AttachOverlay(Control host)
    {
        if (_overlays.ContainsKey(host))
            return;

        var overlay = new ScanlineOverlayControl();
        host.AddChild(overlay);
        overlay.SetPositionLast();
        void ChildAddedHandler(Control child)
        {
            if (child == overlay)
                return;

            UIManager.DeferAction(overlay.SetPositionLast);
        }

        host.OnChildAdded += ChildAddedHandler;
        _overlays[host] = (overlay, ChildAddedHandler);
    }

    private void DetachOverlay(Control host)
    {
        if (!_overlays.Remove(host, out var entry))
            return;

        host.OnChildAdded -= entry.ChildAddedHandler;
        host.RemoveChild(entry.Overlay);
    }

    private sealed class ScanlineOverlayControl : Control
    {
        private const float LineSpacing = 10f;
        private const float LineThickness = 1f;
        private const float GlowThickness = 3f;
        private static readonly Color LineColor = Color.FromHex("#2EF241").WithAlpha(0.07f);
        private static readonly Color GlowColor = Color.FromHex("#2EF241").WithAlpha(0.028f);

        public ScanlineOverlayControl()
        {
            MouseFilter = MouseFilterMode.Ignore;
            HorizontalExpand = true;
            VerticalExpand = true;
            RectClipContent = true;
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            return Vector2.Zero;
        }

        protected override void Draw(DrawingHandleScreen handle)
        {
            base.Draw(handle);

            var width = (float) PixelSize.X;
            var height = (float) PixelSize.Y;

            for (var y = 0f; y < height; y += LineSpacing)
            {
                var glowRect = UIBox2.FromDimensions(0f, y - (GlowThickness - LineThickness) / 2f, width, GlowThickness);
                var rect = UIBox2.FromDimensions(0f, y, width, LineThickness);
                handle.DrawRect(glowRect, GlowColor, filled: true);
                handle.DrawRect(rect, LineColor, filled: true);
            }
        }
    }
}
