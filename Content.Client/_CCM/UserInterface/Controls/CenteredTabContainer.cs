using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Localization;
using Robust.Shared.Maths;
using UIControl = Robust.Client.UserInterface.Control;

namespace Content.Client._CCM.UserInterface.Controls
{
    [Virtual]
    public sealed class CenteredTabContainer : Container
    {
        public static readonly AttachedProperty<bool> TabVisibleProperty = AttachedProperty<bool>.Create("TabVisible", typeof(CenteredTabContainer), true);
        public static readonly AttachedProperty<string?> TabTitleProperty = AttachedProperty<string?>.CreateNull("TabTitle", typeof(CenteredTabContainer));

        public const string StylePropertyTabStyleBox = TabContainer.StylePropertyTabStyleBox;
        public const string StylePropertyTabStyleBoxInactive = TabContainer.StylePropertyTabStyleBoxInactive;
        public const string StylePropertyTabStyleBoxHover = "tabStyleBoxHover";
        public const string stylePropertyTabFontColor = TabContainer.stylePropertyTabFontColor;
        public const string StylePropertyTabFontColorInactive = TabContainer.StylePropertyTabFontColorInactive;
        public const string StylePropertyTabFontColorHover = "tabFontColorHover";
        public const string StylePropertyPanelStyleBox = TabContainer.StylePropertyPanelStyleBox;

        private int _currentTab;
        private int? _hoveredTab;
        private bool _tabsVisible = true;
        private readonly List<float> _tabRight = new();
        private readonly List<float> _tabLeft = new();
        private float _headerOffsetStart;

        public int CurrentTab
        {
            get => _currentTab;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Current tab must be positive.");

                if (value >= ChildCount)
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        "Current tab must less than the amount of tabs.");

                if (_currentTab == value)
                    return;

                var old = _currentTab;
                _currentTab = value;

                GetChild(old).Visible = false;
                var newSelected = GetChild(value);
                newSelected.Visible = true;
                InvalidateMeasure();

                OnTabChanged?.Invoke(value);
            }
        }

        public bool TabsVisible
        {
            get => _tabsVisible;
            set
            {
                _tabsVisible = value;
                InvalidateMeasure();
            }
        }

        public StyleBox? PanelStyleBoxOverride { get; set; }
        public Color? TabFontColorOverride { get; set; }
        public Color? TabFontColorInactiveOverride { get; set; }

        public event Action<int>? OnTabChanged;

        public CenteredTabContainer()
        {
            // CCM rework lobby - start
            MouseFilter = MouseFilterMode.Stop;
            // CCM rework lobby - end
        }

        public string GetActualTabTitle(int tab)
        {
            var control = GetChild(tab);
            var title = control.GetValue(TabTitleProperty);

            return title ?? control.Name ?? Loc.GetString("tab-container-not-tab-title-provided");
        }

        public static string? GetTabTitle(UIControl control)
        {
            return control.GetValue(TabTitleProperty);
        }

        public bool GetTabVisible(int tab)
        {
            var control = GetChild(tab);
            return GetTabVisible(control);
        }

        public static bool GetTabVisible(UIControl control)
        {
            return control.GetValue(TabVisibleProperty);
        }

        public void SetTabTitle(int tab, string title)
        {
            var control = GetChild(tab);
            SetTabTitle(control, title);
        }

        public static void SetTabTitle(UIControl control, string title)
        {
            control.SetValue(TabTitleProperty, title);
        }

        public void SetTabVisible(int tab, bool visible)
        {
            var control = GetChild(tab);
            SetTabVisible(control, visible);
        }

        public static void SetTabVisible(UIControl control, bool visible)
        {
            control.SetValue(TabVisibleProperty, visible);
        }

        protected override void ChildAdded(UIControl newChild)
        {
            base.ChildAdded(newChild);

            if (ChildCount == 1)
            {
                newChild.Visible = true;
            }
            else
            {
                newChild.Visible = false;
            }
        }

        protected override void Draw(DrawingHandleScreen handle)
        {
            base.Draw(handle);

            var headerSize = _getHeaderSize();
            var panel = _getPanel();
            var panelBox = new UIBox2(0, headerSize, PixelWidth, PixelHeight);

            panel?.Draw(handle, panelBox, UIScale);

            var font = _getFont();
            var boxActive = _getTabBoxActive();
            var boxInactive = _getTabBoxInactive();
            var boxHover = _getTabBoxHover();
            var fontColorActive = _getTabFontColorActive();
            var fontColorInactive = _getTabFontColorInactive();
            var fontColorHover = _getTabFontColorHover();

            var totalWidth = GetTotalHeaderWidth(font, boxActive, boxInactive);
            _headerOffsetStart = MathF.Max(0f, (PixelWidth - totalWidth) / 2f);

            _tabRight.Clear();
            _tabLeft.Clear();

            var headerOffset = _headerOffsetStart;
            for (var i = 0; i < ChildCount; i++)
            {
                if (!GetTabVisible(i))
                {
                    _tabLeft.Add(headerOffset);
                    _tabRight.Add(headerOffset);
                    continue;
                }

                var title = GetActualTabTitle(i);
                var titleLength = 0;
                foreach (var rune in title.EnumerateRunes())
                {
                    if (!font.TryGetCharMetrics(rune, UIScale, out var metrics))
                        continue;

                    titleLength += metrics.Advance;
                }

                var active = _currentTab == i;
                var hovered = _hoveredTab == i && !active;
                var box = active ? boxActive : hovered ? boxHover ?? boxInactive : boxInactive;

                UIBox2 contentBox;
                var topLeft = new Vector2(headerOffset, 0);
                var size = new Vector2(titleLength, font.GetHeight(UIScale));
                float boxAdvance;

                if (box != null)
                {
                    var drawBox = box.GetEnvelopBox(topLeft, size, UIScale);
                    boxAdvance = drawBox.Width;
                    box.Draw(handle, drawBox, UIScale);
                    contentBox = box.GetContentBox(drawBox, UIScale);
                }
                else
                {
                    boxAdvance = size.X;
                    contentBox = UIBox2.FromDimensions(topLeft, size);
                }

                var baseLine = new Vector2(0, font.GetAscent(UIScale)) + contentBox.TopLeft;
                foreach (var rune in title.EnumerateRunes())
                {
                    if (!font.TryGetCharMetrics(rune, UIScale, out var metrics))
                        continue;

                    font.DrawChar(handle, rune, baseLine, UIScale, active
                        ? fontColorActive
                        : hovered
                            ? fontColorHover
                            : fontColorInactive);
                    baseLine += new Vector2(metrics.Advance, 0);
                }

                _tabLeft.Add(headerOffset);
                headerOffset += boxAdvance;
                _tabRight.Add(headerOffset);
            }
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            var headerSize = Vector2.Zero;

            if (TabsVisible)
                headerSize = new Vector2(0, _getHeaderSize() / UIScale);

            var panel = _getPanel();
            var panelSize = panel?.MinimumSize ?? Vector2.Zero;

            var contentsSize = availableSize - headerSize - panelSize;

            var total = Vector2.Zero;
            foreach (var child in Children)
            {
                if (child.Visible)
                {
                    child.Measure(contentsSize);
                    total = Vector2.Max(child.DesiredSize, total);
                }
            }

            return total + headerSize + panelSize;
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            if (ChildCount == 0 || _currentTab >= ChildCount)
                return finalSize;

            var headerSize = _getHeaderSize();
            var panel = _getPanel();
            var contentBox = new UIBox2i(0, headerSize, (int) (finalSize.X * UIScale), (int) (finalSize.Y * UIScale));
            if (panel != null)
                contentBox = (UIBox2i) panel.GetContentBox(contentBox, UIScale);

            var control = GetChild(_currentTab);
            control.Visible = true;
            control.ArrangePixel(contentBox);
            return finalSize;
        }

        protected override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            base.KeyBindDown(args);

            if (!TabsVisible || args.Function != EngineKeyFunctions.UIClick)
                return;

            if (args.RelativePixelPosition.Y < 0 || args.RelativePixelPosition.Y > _getHeaderSize())
                return;

            args.Handle();

            if (GetTabAtPixelPosition(args.RelativePixelPosition) is { } tab)
            {
                CurrentTab = tab;
                return;
            }
        }

        protected override void MouseMove(GUIMouseMoveEventArgs args)
        {
            base.MouseMove(args);

            var hovered = GetTabAtPixelPosition(args.RelativePixelPosition);
            if (_hoveredTab == hovered)
                return;

            _hoveredTab = hovered;
            if (hovered != null)
                UserInterfaceManager.HoverSound();
        }

        protected override void MouseExited()
        {
            base.MouseExited();
            _hoveredTab = null;
        }

        private int? GetTabAtPixelPosition(Vector2 position)
        {
            if (!TabsVisible || position.Y < 0 || position.Y > _getHeaderSize())
                return null;

            if (_tabLeft.Count != ChildCount || _tabRight.Count != ChildCount)
                return null;

            for (var i = 0; i < ChildCount; i++)
            {
                if (!GetTabVisible(i))
                    continue;

                if (position.X >= _tabLeft[i] && position.X <= _tabRight[i])
                    return i;
            }

            return null;
        }

        private float GetTotalHeaderWidth(Font font, StyleBox? boxActive, StyleBox? boxInactive)
        {
            var total = 0f;
            for (var i = 0; i < ChildCount; i++)
            {
                if (!GetTabVisible(i))
                    continue;

                var title = GetActualTabTitle(i);
                var titleLength = 0;
                foreach (var rune in title.EnumerateRunes())
                {
                    if (!font.TryGetCharMetrics(rune, UIScale, out var metrics))
                        continue;

                    titleLength += metrics.Advance;
                }

                var box = _currentTab == i ? boxActive : boxInactive;
                var size = new Vector2(titleLength, font.GetHeight(UIScale));
                if (box != null)
                {
                    var drawBox = box.GetEnvelopBox(Vector2.Zero, size, UIScale);
                    total += drawBox.Width;
                }
                else
                {
                    total += size.X;
                }
            }

            return total;
        }

        private int _getHeaderSize()
        {
            var headerSize = 0;

            if (TabsVisible)
            {
                var active = _getTabBoxActive();
                var inactive = _getTabBoxInactive();
                var font = _getFont();

                var activeSize = (active?.MinimumSize ?? Vector2.Zero) * UIScale;
                var inactiveSize = (inactive?.MinimumSize ?? Vector2.Zero) * UIScale;

                headerSize = (int) MathF.Max(activeSize.Y, inactiveSize.Y);
                headerSize += font.GetHeight(UIScale);
            }

            return headerSize;
        }

        private StyleBox? _getTabBoxActive()
        {
            TryGetStyleProperty<StyleBox>(StylePropertyTabStyleBox, out var box);
            return box;
        }

        private StyleBox? _getTabBoxInactive()
        {
            TryGetStyleProperty<StyleBox>(StylePropertyTabStyleBoxInactive, out var box);
            return box;
        }

        private StyleBox? _getTabBoxHover()
        {
            TryGetStyleProperty<StyleBox>(StylePropertyTabStyleBoxHover, out var box);
            return box;
        }

        private Color _getTabFontColorActive()
        {
            if (TabFontColorOverride != null)
                return TabFontColorOverride.Value;

            if (TryGetStyleProperty(stylePropertyTabFontColor, out Color color))
                return color;

            return Color.White;
        }

        private Color _getTabFontColorInactive()
        {
            if (TabFontColorInactiveOverride != null)
                return TabFontColorInactiveOverride.Value;

            if (TryGetStyleProperty(StylePropertyTabFontColorInactive, out Color color))
                return color;

            return Color.Gray;
        }

        private Color _getTabFontColorHover()
        {
            if (TryGetStyleProperty(StylePropertyTabFontColorHover, out Color color))
                return color;

            return _getTabFontColorActive();
        }

        private StyleBox? _getPanel()
        {
            if (PanelStyleBoxOverride != null)
                return PanelStyleBoxOverride;

            TryGetStyleProperty<StyleBox>(StylePropertyPanelStyleBox, out var box);
            return box;
        }

        private Font _getFont()
        {
            if (TryGetStyleProperty<Font>("font", out var font))
                return font;

            return UserInterfaceManager.ThemeDefaults.DefaultFont;
        }
    }
}
