// CM14 rework: non-RMC edit marker.
using System;
using Content.Client.Gameplay;
using Content.Client._CCM.Achievements;
using Content.Client._CCM.Sponsorship;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Guidebook;
using Content.Client.UserInterface.Systems.Info;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Shared.CCVar;
using Content.Shared.Localizations;
using JetBrains.Annotations;
using Robust.Client.Console;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.UserInterface.Systems.EscapeMenu;

[UsedImplicitly]
public sealed class EscapeUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IClientConsoleHost _console = default!;
    [Dependency] private readonly IUriOpener _uri = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly InfoUIController _info = default!;
    [Dependency] private readonly OptionsUIController _options = default!;
    [Dependency] private readonly GuidebookUIController _guidebook = default!;
    [Dependency] private readonly CCMAchievementsUIController _achievements = default!;
    [Dependency] private readonly CCMSponsorshipUIController _sponsorship = default!;
    [Dependency] private readonly ContentLocalizationManager _contentLoc = default!;

    private Options.UI.EscapeMenu? _escapeWindow;

    private MenuButton? EscapeButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.EscapeButton;

    public override void Initialize()
    {
        _contentLoc.CultureChanged += OnCultureChanged;
    }

    public void UnloadButton()
    {
        if (EscapeButton == null)
            return;

        EscapeButton.Pressed = false;
        EscapeButton.OnPressed -= EscapeButtonOnOnPressed;
    }

    public void LoadButton()
    {
        if (EscapeButton == null)
            return;

        EscapeButton.OnPressed += EscapeButtonOnOnPressed;
    }

    private void ActivateButton() => EscapeButton!.SetClickPressed(true);
    private void DeactivateButton() => EscapeButton!.SetClickPressed(false);

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_escapeWindow == null);
        CreateEscapeWindow();

        CommandBinds.Builder
            .Bind(EngineKeyFunctions.EscapeMenu, InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<EscapeUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        DestroyEscapeWindow();
        CommandBinds.Unregister<EscapeUIController>();
    }

    private void EscapeButtonOnOnPressed(ButtonEventArgs obj)
    {
        ToggleWindow();
    }

    private void CloseEscapeWindow()
    {
        _escapeWindow?.Close();
    }

    public void ToggleWindow()
    {
        if (_escapeWindow == null)
            return;

        if (_escapeWindow.IsOpen)
        {
            CloseEscapeWindow();
            EscapeButton!.Pressed = false;
        }
        else
        {
            _escapeWindow.OpenCentered();
            EscapeButton!.Pressed = true;
        }
    }

    private void OnCultureChanged(string _)
    {
        if (_escapeWindow == null)
            return;

        var wasOpen = _escapeWindow.IsOpen;
        DestroyEscapeWindow();
        CreateEscapeWindow();

        if (!wasOpen || _escapeWindow == null)
            return;

        _escapeWindow.OpenCentered();
        EscapeButton?.SetClickPressed(true);
    }

    private void CreateEscapeWindow()
    {
        _escapeWindow = UIManager.CreateWindow<Options.UI.EscapeMenu>();
        _escapeWindow.OnClose += DeactivateButton;
        _escapeWindow.OnOpen += ActivateButton;

        _escapeWindow.RulesButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _info.OpenWindow();
        };

        _escapeWindow.DisconnectButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _console.ExecuteCommand("disconnect");
        };

        _escapeWindow.OptionsButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _options.OpenWindow();
        };

        _escapeWindow.QuitButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _console.ExecuteCommand("quit");
        };

        var wikiUrl = NormalizeAbsoluteHttpUri(_cfg.GetCVar(CCVars.InfoLinksWiki));

        _escapeWindow.WikiButton.OnPressed += _ =>
        {
            if (wikiUrl == null)
                return;

            _uri.OpenUri(wikiUrl);
        };

        _escapeWindow.GuidebookButton.OnPressed += _ =>
        {
            _guidebook.ToggleGuidebook();
        };

        _escapeWindow.AchievementsButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _achievements.OpenWindow();
        };

        _escapeWindow.SponsorshipButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _sponsorship.OpenWindow();
        };

        _escapeWindow.WikiButton.Visible =
            wikiUrl != null &&
            wikiUrl != "https://station14.ru/wiki/%D0%9F%D0%BE%D1%80%D1%82%D0%B0%D0%BB:Colonial_Marines";
    }

    private void DestroyEscapeWindow()
    {
        if (_escapeWindow == null)
            return;

        _escapeWindow.Dispose();
        _escapeWindow = null;
    }

    private static string? NormalizeAbsoluteHttpUri(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        raw = raw.Trim();

        if (raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return raw;

        if (raw.Contains("://", StringComparison.Ordinal))
            return null;

        var trimmed = raw.TrimStart('/');
        if (string.IsNullOrWhiteSpace(trimmed))
            return null;

        return $"https://{trimmed}";
    }
}
