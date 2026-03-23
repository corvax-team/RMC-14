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

namespace Content.Client._CCM.UserInterface.Controls;

public sealed class CCMOptionButton : OptionButton
{
    private readonly Font _itemFont;
    private readonly Dictionary<int, Button> _itemButtons = new();
    private readonly Dictionary<int, Color> _itemColors = new();
    private Label? _selectedLabel;
    private TextureRect? _triangleRect;
    private float _widestItemWidth;

    public CCMOptionButton()
    {
        var cache = IoCManager.Resolve<IResourceCache>();
        _itemFont = cache.GetFont("/Fonts/Exo2/Exo2-Bold.ttf", 13);
        _selectedLabel = FindChild<Label>(this, label =>
            label.StyleClasses.Contains(OptionButton.StyleClassOptionButton));
        _triangleRect = FindChild<TextureRect>(this, triangle =>
            triangle.StyleClasses.Contains(OptionButton.StyleClassOptionTriangle));

        ApplyCollapsedStyle();

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

    public override void ButtonOverride(Button button)
    {
        if (ItemCount > 0)
            _itemButtons[GetItemId(ItemCount - 1)] = button;

        button.MinSize = new Vector2(0, 32);
        button.Margin = new Thickness(0);
        button.Label.FontOverride = _itemFont;
        button.Label.FontColorOverride = Color.FromHex("#D7E1EB");
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
        MinSize = new Vector2(MathF.Max(MinSize.X, _widestItemWidth + 36f), MathF.Max(MinSize.Y, 34f));
        ApplyCollapsedStyle();
    }

    public void SetItemTextColor(int id, Color color)
    {
        _itemColors[id] = color;

        if (_itemButtons.TryGetValue(id, out var button))
            ApplyButtonColor(button, color);

        ApplyCollapsedStyle();
    }

    private static void ApplyButtonColor(Button button, Color? itemColor, bool hovered = false, bool pressed = false)
    {
        var selected = button.Pressed;
        var normalBackground = BlendTowards(StyleNano.ButtonColorContext, Color.White, 0.10f).WithAlpha(0.98f);
        var hoverBackground = BlendTowards(StyleNano.ButtonColorContextHover, Color.White, 0.16f).WithAlpha(0.99f);
        var selectedBackground = BlendTowards(StyleNano.LobbyMenuButtonBase, Color.Black, 0.24f).WithAlpha(0.995f);
        var pressedBackground = BlendTowards(StyleNano.LobbyMenuButtonBase, Color.Black, 0.12f).WithAlpha(0.995f);
        var selectedBorder = BlendTowards(StyleNano.LobbyMenuButtonBase, Color.White, 0.06f);

        button.StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = selected
                ? selectedBackground
                : pressed
                    ? pressedBackground
                    : hovered
                        ? hoverBackground
                        : normalBackground,
            BorderColor = selected || pressed
                ? selectedBorder
                : hovered
                    ? selectedBorder.WithAlpha(0.86f)
                    : selectedBorder.WithAlpha(0.56f),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 10,
            ContentMarginTopOverride = 4,
            ContentMarginRightOverride = 10,
            ContentMarginBottomOverride = 4,
        };

        button.Label.FontColorOverride = selected || pressed
            ? Color.White
            : itemColor ?? (hovered
                ? BlendTowards(StyleNano.LobbyMenuButtonBase, Color.White, 0.22f)
                : Color.FromHex("#EEF4FB"));
    }

    private void ApplyCollapsedStyle(bool hovered = false, bool pressed = false)
    {
        _itemColors.TryGetValue(SelectedId, out var itemColor);
        var hasItemColor = _itemColors.ContainsKey(SelectedId);
        var accentText = BlendTowards(StyleNano.LobbyMenuButtonBase, Color.White, 0.18f);
        var neutralText = Color.FromHex("#EEF4FB");

        if (_selectedLabel != null)
        {
            _selectedLabel.FontOverride = _itemFont;
            _selectedLabel.FontColorOverride = pressed
                ? Color.Black
                : hasItemColor
                    ? itemColor
                    : hovered
                        ? accentText
                        : neutralText;
            _selectedLabel.Align = Label.AlignMode.Center;
        }

        if (_triangleRect != null)
        {
            _triangleRect.ModulateSelfOverride = pressed
                ? Color.Black
                : hasItemColor
                    ? itemColor
                    : hovered
                        ? accentText
                        : neutralText;
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

    private static Color BlendTowards(Color source, Color target, float factor)
    {
        factor = Math.Clamp(factor, 0f, 1f);
        return new Color(
            source.R + (target.R - source.R) * factor,
            source.G + (target.G - source.G) * factor,
            source.B + (target.B - source.B) * factor,
            source.A + (target.A - source.A) * factor);
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
