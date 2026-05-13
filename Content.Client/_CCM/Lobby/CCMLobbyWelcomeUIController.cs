// CM14 rework: non-RMC edit marker.
using System;
using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Shared.CCVar;
using Content.Shared._RMC14.CCVar;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Content.Shared.Localizations;

namespace Content.Client._CCM.Lobby;

[UsedImplicitly]
public sealed class CCMLobbyWelcomeUIController : UIController, IOnStateEntered<LobbyState>, IOnStateExited<LobbyState>, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    private const int CurrentWelcomeVersion = 1;

    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IConsoleHost _console = default!;
    [Dependency] private readonly ContentLocalizationManager _loc = default!;

    private CCMLobbyWelcomeWindow? _window;

    public override void Initialize()
    {
        _console.RegisterCommand("welcome",
            Loc.GetString("cmd-welcome-desc"),
            Loc.GetString("cmd-welcome-help"),
            WelcomeCommand);

        _loc.CultureChanged += OnCultureChanged;
    }

    public void OnStateEntered(LobbyState state)
    {
        EnsureDefaultLobbyPresentation();
        EnsureWindow();

        var seenVersion = _config.GetCVar(RMCCVars.CCMLobbyWelcomeSeenVersion);
        if (seenVersion >= CurrentWelcomeVersion)
            return;

        _config.SetCVar(RMCCVars.CCMLobbyWelcomeSeenVersion, CurrentWelcomeVersion);
        _config.SetCVar(RMCCVars.CCMLobbyWelcomeSeenCount, 1);
        _config.SaveToFile();
        OpenWindow();
    }

    public void OnStateExited(LobbyState state)
    {
        DisposeWindow();
    }

    public void OnStateEntered(GameplayState state)
    {
    }

    public void OnStateExited(GameplayState state)
    {
        DisposeWindow();
    }

    public void OpenWindow()
    {
        EnsureWindow();
        if (_window == null)
            return;

        if (_window.IsOpen)
        {
            _window.MoveToFront();
            return;
        }

        _window.OpenCenteredAnimated();
        _window.MoveToFront();
    }

    private void WelcomeCommand(IConsoleShell shell, string argStr, string[] args)
    {
        OpenWindow();
    }

    private void EnsureWindow()
    {
        if (_window is { Disposed: false })
            return;

        _window = UIManager.CreateWindow<CCMLobbyWelcomeWindow>();
        _window.OnFinished += OnFinished;
    }

    private void OnFinished()
    {
        _config.SaveToFile();
    }

    private void EnsureDefaultLobbyPresentation()
    {
        if (!_config.GetCVar(RMCCVars.RMCLobbyPresentationDefaultsMigrated))
        {
            _config.SetCVar(RMCCVars.RMCLobbyUiStyle, "new");
            _config.SetCVar(RMCCVars.RMCUIColorTheme, "gray");
            _config.SetCVar(RMCCVars.RMCChatTranslateEnabled, false);
            _config.SetCVar(RMCCVars.RMCLobbyPresentationDefaultsMigrated, true);
            _config.SaveToFile();
            return;
        }

        var lobbyStyle = _config.GetCVar(RMCCVars.RMCLobbyUiStyle);
        if (string.IsNullOrWhiteSpace(lobbyStyle) ||
            (!lobbyStyle.Equals("new", StringComparison.OrdinalIgnoreCase) &&
             !lobbyStyle.Equals("old", StringComparison.OrdinalIgnoreCase)))
        {
            _config.SetCVar(RMCCVars.RMCLobbyUiStyle, "new");
        }

        var theme = _config.GetCVar(RMCCVars.RMCUIColorTheme);
        if (string.IsNullOrWhiteSpace(theme) ||
            (!theme.Equals("gray", StringComparison.OrdinalIgnoreCase) &&
             !theme.Equals("green", StringComparison.OrdinalIgnoreCase) &&
             !theme.Equals("blue", StringComparison.OrdinalIgnoreCase)))
        {
            _config.SetCVar(RMCCVars.RMCUIColorTheme, "gray");
        }
    }

    private void OnCultureChanged(string _)
    {
        if (_window is not { Disposed: false })
            return;

        _window.RefreshLocalization();
        _window.MoveToFront();
    }

    private void DisposeWindow()
    {
        if (_window == null)
            return;

        _window.OnFinished -= OnFinished;
        _window.Dispose();
        _window = null;
    }
}
