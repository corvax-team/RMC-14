using System;
using System.Numerics;
using Content.Client.Options.UI;
using Content.Shared.Localizations;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Console;

namespace Content.Client.UserInterface.Systems.EscapeMenu;

[UsedImplicitly]
public sealed class OptionsUIController : UIController
{
    [Dependency] private readonly IConsoleHost _con = default!;
    [Dependency] private readonly ContentLocalizationManager _contentLoc = default!;
    private const float OptionsBaseWidth = 950f;
    private const float OptionsBaseHeight = 760f;
    private Control? _optionsHost;
    private bool _optionsHostHooked;

    public override void Initialize()
    {
        _con.RegisterCommand("options", Loc.GetString("cmd-options-desc"), Loc.GetString("cmd-options-help"), OptionsCommand);
        // CCM rework lobby - start
        _contentLoc.CultureChanged += OnCultureChanged;
        // CCM rework lobby - end
    }

    private void OptionsCommand(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length == 0)
        {
            ToggleWindow();
            return;
        }
        OpenWindow();

        if (!int.TryParse(args[0], out var tab))
        {
            shell.WriteError(Loc.GetString("cmd-parse-failure-integer", ("arg", args[0])));
            return;
        }

        _optionsWindow.SelectTabIndex(tab);
    }

    private OptionsMenu _optionsWindow = default!;

    private void EnsureWindow()
    {
        if (_optionsWindow is { Disposed: false })
            return;

        _optionsWindow = UIManager.CreateWindow<OptionsMenu>();
        HookOptionsHost();
    }

    // CCM rework lobby - start
    private void OnCultureChanged(string _)
    {
        if (_optionsWindow is not { Disposed: false })
            return;

        var wasOpen = _optionsWindow.IsOpen;
        var selectedTab = _optionsWindow.Tabs.CurrentTab;

        _optionsWindow.Dispose();
        _optionsWindow = default!;

        if (!wasOpen)
            return;

        OpenWindow();
        _optionsWindow.SelectTabIndex(selectedTab);
    }
    // CCM rework lobby - end

    public void OpenWindow()
    {
        EnsureWindow();

        _optionsWindow.UpdateTabs();
        UpdateOptionsWindowLayout();

        _optionsWindow.OpenCenteredAnimated();
        _optionsWindow.MoveToFront();
    }

    public void ToggleWindow()
    {
        EnsureWindow();

        if (_optionsWindow.IsOpen)
        {
            _optionsWindow.CloseAnimated();
        }
        else
        {
            OpenWindow();
        }
    }

    private void HookOptionsHost()
    {
        if (_optionsHostHooked)
            return;

        _optionsHost = UIManager.RootControl;
        if (_optionsHost == null)
            return;

        _optionsHost.OnResized += UpdateOptionsWindowLayout;
        _optionsHostHooked = true;
    }

    private void UpdateOptionsWindowLayout()
    {
        if (_optionsWindow is not { Disposed: false })
            return;

        var root = UIManager.RootControl;
        if (root == null)
            return;

        var rootSize = root.Size;
        if (rootSize.X <= 1f || rootSize.Y <= 1f)
            return;

        var desired = new Vector2(OptionsBaseWidth, OptionsBaseHeight);
        var scale = MathF.Min(1f, MathF.Min(rootSize.X / desired.X, rootSize.Y / desired.Y));
        _optionsWindow.SetSize = desired * scale;

        if (_optionsWindow.IsOpen)
            _optionsWindow.RecenterWindow(new Vector2(0.5f, 0.5f));
    }
}
