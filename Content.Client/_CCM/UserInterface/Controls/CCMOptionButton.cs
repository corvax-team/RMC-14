// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Numerics;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Graphics;
using Robust.Shared.IoC;
using Robust.Shared.Input;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Client._CCM.UserInterface.Controls;

public sealed class CCMOptionButton : OptionButton
{
    private readonly Font _itemFont;
    private readonly Dictionary<int, Button> _itemButtons = new();
    private readonly Dictionary<int, Color> _itemColors = new();
    private StyleNano.UiColorTheme _appliedTheme;
    private bool _appliedNeutralPalette;
    private Label? _selectedLabel;
    private TextureRect? _triangleRect;
    private float _widestItemWidth;
    private Popup? _dropdownPopup;
    private bool _popupOpen;
    private Vector2 _popupAnchorPosition;
    private static readonly Color NeutralBackground = Color.FromHex("#464966");
    private static readonly Color NeutralHoverBackground = Color.FromHex("#575B7F");
    private static readonly Color NeutralPressedBackground = Color.FromHex("#3E6C45");
    private static readonly Color NeutralBorder = Color.FromHex("#A88B5E");

    public Font? TextFontOverride { get; set; }
    public float ItemMinHeight { get; set; } = 32f;
    public float ControlMinHeight { get; set; } = 34f;
    public Thickness ContentPadding { get; set; } = new(6, 4, 6, 4);
    public bool UseNeutralPalette { get; set; }

    public CCMOptionButton()
    {
        var cache = IoCManager.Resolve<IResourceCache>();
        _itemFont = cache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
        _appliedTheme = StyleNano.CurrentTheme;
        _appliedNeutralPalette = UseNeutralPalette;
        _selectedLabel = FindChild<Label>(this, label =>
            label.StyleClasses.Contains(OptionButton.StyleClassOptionButton));
        _triangleRect = FindChild<TextureRect>(this, triangle =>
            triangle.StyleClasses.Contains(OptionButton.StyleClassOptionTriangle));

        ApplyCollapsedStyle();

        OnPressed += _ => CapturePopup();
        OnMouseEntered += _ => ApplyCollapsedStyle(hovered: true);
        OnMouseExited += _ => ApplyCollapsedStyle();
        OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyCollapsedStyle(pressed: true);
        };
        OnKeyBindUp += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyCollapsedStyle();
        };
        OnItemSelected += _ => ApplyCollapsedStyle();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        RefreshPopupState();

        if (_appliedTheme == StyleNano.CurrentTheme && _appliedNeutralPalette == UseNeutralPalette)
        {
            ClosePopupIfAnchorMoved();
            return;
        }

        _appliedTheme = StyleNano.CurrentTheme;
        _appliedNeutralPalette = UseNeutralPalette;
        RefreshVisualStyle();
        ClosePopupIfAnchorMoved();
    }

    public override void ButtonOverride(Button button)
    {
        if (ItemCount > 0)
            _itemButtons[GetItemId(ItemCount - 1)] = button;

        button.MinSize = new Vector2(0, ItemMinHeight);
        button.Margin = new Thickness(-1, 0, 0, 0);
        button.Label.FontOverride = TextFontOverride ?? _itemFont;
        button.Label.FontColorOverride = Color.FromHex("#D7E1EB");
        button.Label.FontColorShadowOverride = Color.Black.WithAlpha(0.72f);
        button.Label.Align = Label.AlignMode.Center;

        ApplyButtonColor(button, GetItemColor(ItemCount > 0 ? GetItemId(ItemCount - 1) : -1));

        button.OnMouseEntered += _ => ApplyButtonColor(button, GetButtonColor(button), hovered: true);
        button.OnMouseExited += _ => ApplyButtonColor(button, GetButtonColor(button));
        button.OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyButtonColor(button, GetButtonColor(button), hovered: true, pressed: true);
        };
        button.OnKeyBindUp += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ApplyButtonColor(button, GetButtonColor(button));
        };

        button.Measure(Vector2Helpers.Infinity);
        _widestItemWidth = MathF.Max(_widestItemWidth, button.DesiredSize.X);
        MinSize = new Vector2(MathF.Max(MinSize.X, _widestItemWidth + 36f), MathF.Max(MinSize.Y, ControlMinHeight));
        ApplyCollapsedStyle();
    }

    public void RefreshVisualStyle()
    {
        foreach (var button in _itemButtons.Values)
        {
            button.MinSize = new Vector2(0, ItemMinHeight);
            button.Label.FontOverride = TextFontOverride ?? _itemFont;
        }

        ApplyCollapsedStyle();
    }

    public void SetItemTextColor(int id, Color color)
    {
        _itemColors[id] = color;

        if (_itemButtons.TryGetValue(id, out var button))
            ApplyButtonColor(button, color);

        ApplyCollapsedStyle();
    }

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        if (_popupOpen)
            CloseTrackedPopup();

        base.MouseWheel(args);
    }

    private void ApplyButtonColor(Button button, Color? itemColor, bool hovered = false, bool pressed = false)
    {
        var normalBackground = GetNormalBackground();
        var hoverBackground = GetHoverBackground();
        var pressedBackground = GetPressedBackground();
        var normalBorder = GetBorderColor();
        var hoverBorder = GetBorderColor();
        var pressedBorder = GetBorderColor();

        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = pressed
                ? pressedBackground
                : hovered
                    ? hoverBackground
                    : normalBackground,
            BorderColor = pressed
                ? pressedBorder
                : hovered
                    ? hoverBorder
                    : normalBorder,
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = ContentPadding.Left,
            ContentMarginTopOverride = ContentPadding.Top,
            ContentMarginRightOverride = ContentPadding.Right,
            ContentMarginBottomOverride = ContentPadding.Bottom,
        };

        button.Label.FontColorOverride = pressed
            ? Color.White
            : itemColor ?? (hovered
                ? Color.White
                : Color.FromHex("#C5CED8"));
    }

    private void ApplyCollapsedStyle(bool hovered = false, bool pressed = false)
    {
        _itemColors.TryGetValue(SelectedId, out var itemColor);
        var hasItemColor = _itemColors.ContainsKey(SelectedId);
        var neutralText = Color.FromHex("#EEF4FB");
        var accentText = Color.White;

        StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = pressed
                    ? GetPressedBackground()
                : hovered
                    ? GetHoverBackground()
                    : GetNormalBackground(),
            BorderColor = pressed
                ? GetBorderColor()
                : hovered
                ? GetBorderColor()
                    : GetBorderColor(),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = ContentPadding.Left,
            ContentMarginTopOverride = ContentPadding.Top,
            ContentMarginRightOverride = ContentPadding.Right,
            ContentMarginBottomOverride = ContentPadding.Bottom,
        };

        if (_selectedLabel != null)
        {
            _selectedLabel.FontOverride = TextFontOverride ?? _itemFont;
            _selectedLabel.FontColorOverride = pressed
                ? Color.White
                : hasItemColor
                    ? itemColor
                    : hovered
                        ? accentText
                        : Color.FromHex("#C5CED8");
            _selectedLabel.Align = Label.AlignMode.Center;
        }

        if (_triangleRect != null)
        {
            _triangleRect.ModulateSelfOverride = pressed
                ? Color.White
                : hasItemColor
                    ? itemColor
                    : hovered
                        ? accentText
                        : Color.FromHex("#C5CED8");
        }

        foreach (var (id, button) in _itemButtons)
        {
            _itemColors.TryGetValue(id, out var color);
            ApplyButtonColor(button, _itemColors.ContainsKey(id) ? color : null);
        }
    }

    private Color? GetButtonColor(Button button)
    {
        foreach (var (id, itemButton) in _itemButtons)
        {
            if (itemButton != button)
                continue;

            return GetItemColor(id);
        }

        return null;
    }

    private Color? GetItemColor(int id)
    {
        return _itemColors.TryGetValue(id, out var color) ? color : null;
    }

    private Color GetNormalBackground()
    {
        return UseNeutralPalette ? NeutralBackground : StyleNano.DropdownButtonColorContext;
    }

    private Color GetHoverBackground()
    {
        return UseNeutralPalette ? NeutralHoverBackground : StyleNano.DropdownButtonColorContextHover;
    }

    private Color GetPressedBackground()
    {
        return UseNeutralPalette ? NeutralPressedBackground : StyleNano.DropdownButtonColorContextPressed;
    }

    private Color GetBorderColor()
    {
        return UseNeutralPalette ? NeutralBorder : StyleNano.UiButtonBorder;
    }

    private void CapturePopup()
    {
        _dropdownPopup = FindPopup();
        _popupOpen = _dropdownPopup != null;
        _popupAnchorPosition = GlobalPosition;
    }

    private void RefreshPopupState()
    {
        var popup = FindPopup();
        if (popup == null)
        {
            _dropdownPopup = null;
            _popupOpen = false;
            return;
        }

        if (!_popupOpen || _dropdownPopup != popup)
        {
            _dropdownPopup = popup;
            _popupOpen = true;
            _popupAnchorPosition = GlobalPosition;
        }
    }

    private void ClosePopupIfAnchorMoved()
    {
        if (!_popupOpen || _dropdownPopup == null)
            return;

        if (Vector2.DistanceSquared(GlobalPosition, _popupAnchorPosition) < 0.25f)
            return;

        CloseTrackedPopup();
    }

    private void CloseTrackedPopup()
    {
        if (_dropdownPopup != null)
            _dropdownPopup.Close();

        _dropdownPopup = null;
        _popupOpen = false;
    }

    private Popup? FindPopup()
    {
        if (UserInterfaceManager.ModalRoot.ChildCount == 0)
            return null;

        var expectedOrigin = GlobalPosition;
        expectedOrigin.Y += Size.Y + 1;
        expectedOrigin.Y -= Margin.SumVertical;

        Popup? bestPopup = null;
        var bestDistance = float.MaxValue;

        foreach (var child in UserInterfaceManager.ModalRoot.Children)
        {
            if (child is not Popup popup ||
                !popup.Visible ||
                !popup.StyleClasses.Contains(OptionButton.StyleClassPopup))
            {
                continue;
            }

            var origin = PopupContainer.GetPopupOrigin(popup);
            var distance = Vector2.DistanceSquared(origin, expectedOrigin);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestPopup = popup;
        }

        return bestDistance <= 4f ? bestPopup : null;
    }

    private static T? FindChild<T>(Robust.Client.UserInterface.Control root, Predicate<T> predicate)
        where T : Robust.Client.UserInterface.Control
    {
        foreach (var child in root.Children)
        {
            if (child is T typed && predicate(typed))
                return typed;

            if (child is Robust.Client.UserInterface.Control control)
            {
                var nested = FindChild(control, predicate);
                if (nested != null)
                    return nested;
            }
        }

        return null;
    }
}
