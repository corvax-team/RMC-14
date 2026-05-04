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

    private static readonly Color ColorNormal = Color.FromHex("#B8BECD");
    private static readonly Color ColorRedNormal = Color.FromHex("#FEFEFE");
    private static readonly Color ColorHovered = Color.FromHex("#D2D7E5");
    private static readonly Color ColorRedHovered = Color.FromHex("#FFFFFF");
    private static readonly Color ColorPressed = Color.FromHex("#C1C7D8");
    private static readonly Color ColorContentNormal = Color.FromHex("#5F6777");
    private static readonly Color ColorContentDisabled = Color.FromHex("#7A8395");

    private const float VertPad = 5f;
    private Color NormalColor => HasStyleClass(StyleClassRedTopButton) ? ColorRedNormal : ColorNormal;
    private Color HoveredColor => HasStyleClass(StyleClassRedTopButton) ? ColorRedHovered : ColorHovered;

    private BoundKeyFunction _function;
    private readonly BoxContainer _root;
    private readonly TextureRect? _buttonIcon;
    private readonly Label? _buttonLabel;
    private readonly StyleBoxFlat _styleNormal;
    private readonly StyleBoxFlat _styleHover;
    private readonly StyleBoxFlat _stylePressed;
    private readonly StyleBoxFlat _styleDisabled;

    public string AppendStyleClass { set => AddStyleClass(value); }
    public Texture? Icon { get => _buttonIcon!.Texture; set => _buttonIcon!.Texture = value; }

    public BoundKeyFunction BoundKey
    {
        get => _function;
        set
        {
            _function = value;
            _buttonLabel!.Text = BoundKeyHelper.ShortKeyName(value);
        }
    }

    public BoxContainer ButtonRoot => _root;

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
        _buttonLabel!.Text = BoundKeyHelper.ShortKeyName(_function);
    }

    private void OnKeyBindingChanged()
    {
        _buttonLabel!.Text = BoundKeyHelper.ShortKeyName(_function);
    }

    protected override void StylePropertiesChanged()
    {
        // colors of children depend on style, so ensure we update when style is changed
        base.StylePropertiesChanged();
        UpdateChildColors();
    }

    private void UpdateChildColors()
    {
        UpdateThemePalette();

        if (_buttonIcon == null || _buttonLabel == null) return;
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
        var baseColor = Blend(StyleNano.ButtonColorDefault, Color.White, 0.16f).WithAlpha(0.96f);
        var hoverColor = Blend(StyleNano.ButtonColorHovered, Color.White, 0.12f).WithAlpha(0.98f);
        var pressedColor = Blend(StyleNano.ButtonColorPressed, Color.White, 0.08f).WithAlpha(0.96f);
        var disabledColor = Blend(StyleNano.ButtonColorDisabled, Color.White, 0.04f).WithAlpha(0.86f);
        var borderColor = Blend(StyleNano.UiButtonBorder, Color.White, 0.16f).WithAlpha(0.98f);
        var pressedBorderColor = Blend(StyleNano.UiButtonBorder, Color.White, 0.26f).WithAlpha(0.98f);
        var disabledBorderColor = Blend(StyleNano.UiButtonBorder, Color.White, 0.08f).WithAlpha(0.92f);

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
