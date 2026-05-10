using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Graphics;
using Robust.Shared.Input;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.Controls;

public sealed class MenuButton : ContainerButton
{
    [Dependency] private readonly IInputManager _inputManager = default!;
    public const string StyleClassLabelTopButton = "topButtonLabel";
    public const string StyleClassRedTopButton = "topButtonLabel";

    private static readonly Color ColorNormal = Color.FromHex("#a1a1a1");
    private static readonly Color ColorRedNormal = Color.FromHex("#FEFEFE");
    private static readonly Color ColorHovered = Color.FromHex("#a1a1a1");
    private static readonly Color ColorRedHovered = Color.FromHex("#FFFFFF");
    private static readonly Color ColorPressed = Color.FromHex("#a1a1a1");
    private static readonly Color ColorContentNormal = Color.FromHex("#a1a1a1");
    private static readonly Color ColorContentDisabled = Color.FromHex("#737780");

    private const float VertPad = 5f;
    private Color NormalColor => HasStyleClass(StyleClassRedTopButton) ? ColorRedNormal : ColorNormal;
    private Color HoveredColor => HasStyleClass(StyleClassRedTopButton) ? ColorRedHovered : ColorHovered;

    private BoundKeyFunction _function;
    private BoxContainer? _root;
    private TextureRect? _buttonIcon;
    private Label? _buttonLabel;
    private StyleBoxFlat? _styleNormal;
    private StyleBoxFlat? _styleHover;
    private StyleBoxFlat? _stylePressed;
    private StyleBoxFlat? _styleDisabled;

    public string AppendStyleClass { set => AddStyleClass(value); }
    public Texture? Icon
    {
        get => _buttonIcon?.Texture;
        set
        {
            if (_buttonIcon != null)
                _buttonIcon.Texture = value;
        }
    }

    public BoundKeyFunction BoundKey
    {
        get => _function;
        set
        {
            _function = value;
            if (_buttonLabel != null)
                _buttonLabel.Text = BoundKeyHelper.ShortKeyName(value);
        }
    }

    public BoxContainer? ButtonRoot => _root;

    public MenuButton()
    {
        IoCManager.InjectDependencies(this);
        _styleNormal = new StyleBoxFlat
        {
            BorderThickness = new Thickness(1),
        };
        _styleHover = new StyleBoxFlat
        {
            BorderThickness = new Thickness(1),
        };
        _stylePressed = new StyleBoxFlat
        {
            BorderThickness = new Thickness(1),
        };
        _styleDisabled = new StyleBoxFlat
        {
            BorderThickness = new Thickness(1),
        };
        _buttonIcon = new TextureRect()
        {
            TextureScale = new Vector2(0.46f, 0.46f),
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            VerticalExpand = true,
            Margin = new Thickness(0, VertPad),
            ModulateSelfOverride = NormalColor,
            Stretch = TextureRect.StretchMode.KeepCentered
        };
        _buttonLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HAlignment.Center,
            ModulateSelfOverride = NormalColor,
            StyleClasses = {StyleClassLabelTopButton}
        };
        _root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Children =
            {
                _buttonIcon,
                _buttonLabel
            }
        };
        AddChild(_root);
        ToggleMode = true;
        UpdateThemePalette();
        StyleBoxOverride = _styleNormal;
    }

    protected override void EnteredTree()
    {
        _inputManager.OnKeyBindingAdded += OnKeyBindingChanged;
        _inputManager.OnKeyBindingRemoved += OnKeyBindingChanged;
        _inputManager.OnInputModeChanged += OnKeyBindingChanged;
    }

    protected override void ExitedTree()
    {
        _inputManager.OnKeyBindingAdded -= OnKeyBindingChanged;
        _inputManager.OnKeyBindingRemoved -= OnKeyBindingChanged;
        _inputManager.OnInputModeChanged -= OnKeyBindingChanged;
    }


    private void OnKeyBindingChanged(IKeyBinding obj)
    {
        if (_buttonLabel != null)
            _buttonLabel.Text = BoundKeyHelper.ShortKeyName(_function);
    }

    private void OnKeyBindingChanged()
    {
        if (_buttonLabel != null)
            _buttonLabel.Text = BoundKeyHelper.ShortKeyName(_function);
    }

    protected override void StylePropertiesChanged()
    {
        // colors of children depend on style, so ensure we update when style is changed
        base.StylePropertiesChanged();
        UpdateChildColors();
    }

    private void UpdateChildColors()
    {
        if (_styleNormal == null ||
            _styleHover == null ||
            _stylePressed == null ||
            _styleDisabled == null ||
            _buttonIcon == null ||
            _buttonLabel == null)
        {
            return;
        }

        UpdateThemePalette();
        switch (DrawMode)
        {
            case DrawModeEnum.Normal:
                StyleBoxOverride = _styleNormal;
                _buttonIcon.ModulateSelfOverride = ColorContentNormal;
                _buttonLabel.ModulateSelfOverride = ColorContentNormal;
                break;

            case DrawModeEnum.Pressed:
                StyleBoxOverride = _stylePressed;
                _buttonIcon.ModulateSelfOverride = NormalColor;
                _buttonLabel.ModulateSelfOverride = NormalColor;
                break;

            case DrawModeEnum.Hover:
                StyleBoxOverride = _styleHover;
                _buttonIcon.ModulateSelfOverride = HoveredColor;
                _buttonLabel.ModulateSelfOverride = HoveredColor;
                break;

            case DrawModeEnum.Disabled:
                StyleBoxOverride = _styleDisabled;
                _buttonIcon.ModulateSelfOverride = ColorContentDisabled.WithAlpha(0.82f);
                _buttonLabel.ModulateSelfOverride = ColorContentDisabled.WithAlpha(0.82f);
                break;
        }
    }


    protected override void DrawModeChanged()
    {
        base.DrawModeChanged();
        UpdateChildColors();
    }

    private void UpdateThemePalette()
    {
        if (_styleNormal == null ||
            _styleHover == null ||
            _stylePressed == null ||
            _styleDisabled == null)
        {
            return;
        }

        var isGreenTheme = StyleNano.CurrentTheme == StyleNano.UiColorTheme.Green;
        var baseBlend = isGreenTheme ? 0.24f : 0.16f;
        var hoverBlend = isGreenTheme ? 0.18f : 0.12f;
        var pressedBlend = isGreenTheme ? 0.12f : 0.08f;
        var disabledBlend = isGreenTheme ? 0.08f : 0.04f;
        var borderBlend = isGreenTheme ? 0.24f : 0.16f;
        var pressedBorderBlend = isGreenTheme ? 0.34f : 0.26f;
        var disabledBorderBlend = isGreenTheme ? 0.14f : 0.08f;

        var baseColor = Blend(StyleNano.ButtonColorDefault, Color.White, baseBlend).WithAlpha(0.96f);
        var hoverColor = Blend(StyleNano.ButtonColorHovered, Color.White, hoverBlend).WithAlpha(0.98f);
        var pressedColor = Blend(StyleNano.ButtonColorPressed, Color.White, pressedBlend).WithAlpha(0.96f);
        var disabledColor = Blend(StyleNano.ButtonColorDisabled, Color.White, disabledBlend).WithAlpha(0.86f);
        var borderColor = Blend(StyleNano.UiButtonBorder, Color.White, borderBlend).WithAlpha(0.98f);
        var pressedBorderColor = Blend(StyleNano.UiButtonBorder, Color.White, pressedBorderBlend).WithAlpha(0.98f);
        var disabledBorderColor = Blend(StyleNano.UiButtonBorder, Color.White, disabledBorderBlend).WithAlpha(0.92f);

        _styleNormal.BackgroundColor = baseColor;
        _styleNormal.BorderColor = borderColor;

        _styleHover.BackgroundColor = hoverColor;
        _styleHover.BorderColor = borderColor;

        _stylePressed.BackgroundColor = pressedColor;
        _stylePressed.BorderColor = pressedBorderColor;

        _styleDisabled.BackgroundColor = disabledColor;
        _styleDisabled.BorderColor = disabledBorderColor;
    }

    private static Color Blend(Color source, Color target, float factor)
    {
        factor = Math.Clamp(factor, 0f, 1f);
        return new Color(
            source.R + (target.R - source.R) * factor,
            source.G + (target.G - source.G) * factor,
            source.B + (target.B - source.B) * factor,
            source.A + (target.A - source.A) * factor);
    }
}
