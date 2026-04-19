// CM14 rework: non-RMC edit marker.
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Client.Resources;
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

    private static readonly Color ColorNormal = Color.Black;
    private static readonly Color ColorRedNormal = Color.Black;
    private static readonly Color ColorPressed = Color.Black;

    private const float VertPad = 5f;
    private Color NormalColor => HasStyleClass(StyleClassRedTopButton) ? ColorRedNormal : ColorNormal;

    private BoundKeyFunction _function;
    private readonly BoxContainer _root;
    private readonly TextureRect? _buttonIcon;
    private readonly Label? _buttonLabel;

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
        _buttonIcon = new TextureRect()
        {
            TextureScale = new Vector2(0.5f, 0.5f),
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
            SeparationOverride = 2,
            Children =
            {
                _buttonIcon,
                _buttonLabel
            }
        };
        AddChild(_root);
        ToggleMode = true;
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
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (_buttonIcon == null || _buttonLabel == null) return;

        ModulateSelfOverride = Color.White;

        if (Disabled)
        {
            StyleBoxOverride = BuildStyleBox(StyleNano.LobbyMenuButtonDisabledCrt, StyleNano.LobbyMenuButtonDisabledCrt);
            _buttonIcon.ModulateSelfOverride = Color.Black.WithAlpha(0.55f);
            _buttonLabel.ModulateSelfOverride = Color.Black.WithAlpha(0.55f);
            _buttonLabel.FontColorShadowOverride = null;
            return;
        }

        var accent = StyleNano.LobbyMenuButtonBase;
        var pressedAccent = StyleNano.LobbyMenuButtonPressed;
        var normalIconColor = TintIcon(accent, 0.28f);
        var pressedIconColor = TintIcon(pressedAccent, 0.34f);

        switch (DrawMode)
        {
            case DrawModeEnum.Normal:
                StyleBoxOverride = BuildStyleBox(accent, accent);
                _buttonIcon.ModulateSelfOverride = normalIconColor;
                _buttonLabel.ModulateSelfOverride = NormalColor;
                _buttonLabel.FontColorShadowOverride = null;
                break;

            case DrawModeEnum.Pressed:
                StyleBoxOverride = BuildStyleBox(pressedAccent, pressedAccent);
                _buttonIcon.ModulateSelfOverride = pressedIconColor;
                _buttonLabel.ModulateSelfOverride = ColorPressed;
                _buttonLabel.FontColorShadowOverride = null;
                break;

            case DrawModeEnum.Hover:
                StyleBoxOverride = BuildStyleBox(Color.Transparent, accent);
                _buttonIcon.ModulateSelfOverride = accent;
                _buttonLabel.ModulateSelfOverride = accent;
                _buttonLabel.FontColorShadowOverride = null;
                break;

            case DrawModeEnum.Disabled:
                break;
        }
    }

    private static Color TintIcon(Color accent, float strength)
    {
        return new Color(accent.R * strength, accent.G * strength, accent.B * strength, 1f);
    }

    private static StyleBoxFlat BuildStyleBox(Color background, Color border)
    {
        return new StyleBoxFlat
        {
            BackgroundColor = background,
            BorderColor = border,
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 3,
            ContentMarginTopOverride = 2,
            ContentMarginRightOverride = 3,
            ContentMarginBottomOverride = 2,
        };
    }

    protected override void DrawModeChanged()
    {
        base.DrawModeChanged();
        UpdateVisualState();
    }
}
