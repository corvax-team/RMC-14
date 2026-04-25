using Content.Shared._RMC14.LinkAccount;
using Content.Client.Message;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Network;
using static Robust.Client.UserInterface.Controls.BaseButton;
using static Robust.Client.UserInterface.Controls.LineEdit;
using static Robust.Client.UserInterface.Controls.TabContainer;

namespace Content.Client._RMC14.LinkAccount;

public sealed class LinkAccountUIController : UIController, IOnSystemChanged<LinkAccountSystem>
{
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly INetManager _net = default!;

    private PatronPerksWindow? _patronPerksWindow;

    private void OnLobbyMessageReceived(SharedRMCDisplayLobbyMessageEvent message)
    {
    }

    public void ToggleWindow()
    {
        // RMC discord oauth rework: use the dedicated OAuth flow UI instead of the legacy copy-code window.
        UIManager.GetUIController<DiscordOAuthUIController>().OpenLink();
    }

    public void TogglePatronPerksWindow()
    {
        if (_patronPerksWindow == null)
        {
            _patronPerksWindow = new PatronPerksWindow();
            _patronPerksWindow.OnClose += () => _patronPerksWindow = null;

            var tier = _linkAccount.Tier;
            SetTabTitle(_patronPerksWindow.LobbyMessageTab, Loc.GetString("rmc-ui-lobby-message"));
            SetTabVisible(_patronPerksWindow.LobbyMessageTab, tier is { LobbyMessage: true });
            _patronPerksWindow.LobbyMessage.OnTextEntered += ChangeLobbyMessage;
            _patronPerksWindow.LobbyMessage.OnFocusExit += ChangeLobbyMessage;

            if (_linkAccount.LobbyMessage?.Message is { } lobbyMessage)
                _patronPerksWindow.LobbyMessage.Text = lobbyMessage;

            SetTabTitle(_patronPerksWindow.ShoutoutTab, Loc.GetString("rmc-ui-shoutout"));
            SetTabVisible(_patronPerksWindow.ShoutoutTab, tier is { RoundEndShoutout: true });
            _patronPerksWindow.MarineShoutout.OnTextEntered += ChangeMarineShoutout;
            _patronPerksWindow.MarineShoutout.OnFocusExit += ChangeMarineShoutout;

            if (_linkAccount.RoundEndShoutout?.Marine is { } marineShoutout)
                _patronPerksWindow.MarineShoutout.Text = marineShoutout;

            _patronPerksWindow.XenoShoutout.OnTextEntered += ChangeXenoShoutout;
            _patronPerksWindow.XenoShoutout.OnFocusExit += ChangeXenoShoutout;

            if (_linkAccount.RoundEndShoutout?.Xeno is { } xenoShoutout)
                _patronPerksWindow.XenoShoutout.Text = xenoShoutout;

            SetTabTitle(_patronPerksWindow.GhostColorTab, Loc.GetString("rmc-ui-ghost-color"));
            SetTabVisible(_patronPerksWindow.GhostColorTab, tier is { GhostColor: true });
            _patronPerksWindow.GhostColorSliders.Color = _linkAccount.GhostColor ?? Color.White;
            _patronPerksWindow.GhostColorSliders.OnColorChanged += OnGhostColorChanged;
            _patronPerksWindow.GhostColorClearButton.OnPressed += OnGhostColorClear;
            _patronPerksWindow.GhostColorSaveButton.OnPressed += OnGhostColorSave;

            SetTabTitle(_patronPerksWindow.NamedItemsReferenceTab, Loc.GetString("rmc-ui-named-items"));
            SetTabVisible(_patronPerksWindow.NamedItemsReferenceTab, tier is { NamedItems: true });

            SetTabTitle(_patronPerksWindow.FigurineReferenceTab, Loc.GetString("rmc-ui-figurine"));
            SetTabVisible(_patronPerksWindow.FigurineReferenceTab, tier is { Figurines: true });

            UpdateExamples();

            for (var i = 0; i < _patronPerksWindow.Tabs.ChildCount; i++)
            {
                var child = _patronPerksWindow.Tabs.GetChild(i);
                if (!child.GetValue(TabVisibleProperty))
                    continue;

                _patronPerksWindow.Tabs.CurrentTab = i;
                break;
            }

            _patronPerksWindow.OpenCentered();
            return;
        }

        _patronPerksWindow.Close();
        _patronPerksWindow = null;
    }

    private void ChangeLobbyMessage(LineEditEventArgs args)
    {
        var text = args.Text;
        if (text.Length > SharedRMCLobbyMessage.CharacterLimit)
        {
            text = text[..SharedRMCLobbyMessage.CharacterLimit];
            _patronPerksWindow?.LobbyMessage.SetText(text, false);
        }

        _net.ClientSendMessage(new RMCChangeLobbyMessageMsg { Text = text });
    }

    private void ChangeMarineShoutout(LineEditEventArgs args)
    {
        var text = args.Text;
        if (text.Length > SharedRMCRoundEndShoutouts.CharacterLimit)
        {
            text = text[..SharedRMCRoundEndShoutouts.CharacterLimit];
            _patronPerksWindow?.LobbyMessage.SetText(text, false);
        }

        _net.ClientSendMessage(new RMCChangeMarineShoutoutMsg { Name = text });
        UpdateExamples();
    }

    private void ChangeXenoShoutout(LineEditEventArgs args)
    {
        var text = args.Text;
        if (text.Length > SharedRMCRoundEndShoutouts.CharacterLimit)
        {
            text = text[..SharedRMCRoundEndShoutouts.CharacterLimit];
            _patronPerksWindow?.LobbyMessage.SetText(text, false);
        }

        _net.ClientSendMessage(new RMCChangeXenoShoutoutMsg { Name = text });
        UpdateExamples();
    }

    private void OnGhostColorChanged(Color color)
    {
        if (_patronPerksWindow is not { IsOpen: true })
            return;

        _patronPerksWindow.GhostColorSaveButton.Disabled = false;
    }

    private void OnGhostColorClear(ButtonEventArgs args)
    {
        if (_patronPerksWindow is not { IsOpen: true })
            return;

        _patronPerksWindow.GhostColorSliders.Color = Color.White;
        _patronPerksWindow.GhostColorSaveButton.Disabled = true;
        _net.ClientSendMessage(new RMCClearGhostColorMsg());
    }

    private void OnGhostColorSave(ButtonEventArgs args)
    {
        if (_patronPerksWindow is not { IsOpen: true })
            return;

        _patronPerksWindow.GhostColorSaveButton.Disabled = true;
        _net.ClientSendMessage(new RMCChangeGhostColorMsg { Color = _patronPerksWindow.GhostColorSliders.Color });
    }

    private void UpdateExamples()
    {
        if (_patronPerksWindow == null)
            return;

        var marine = _patronPerksWindow.MarineShoutout.Text.Trim();
        _patronPerksWindow.MarineShoutoutExample.SetMarkupPermissive(string.IsNullOrWhiteSpace(marine)
            ? " "
            : $"{Loc.GetString("rmc-ui-shoutout-example")} {Loc.GetString("rmc-ui-shoutout-marine", ("name", marine))}");

        var xeno = _patronPerksWindow.XenoShoutout.Text.Trim();
        _patronPerksWindow.XenoShoutoutExample.SetMarkupPermissive(string.IsNullOrWhiteSpace(xeno)
            ? " "
            : $"{Loc.GetString("rmc-ui-shoutout-example")} {Loc.GetString("rmc-ui-shoutout-xeno", ("name", xeno))}");
    }

    public void OnSystemLoaded(LinkAccountSystem system)
    {
        system.LobbyMessageReceived += OnLobbyMessageReceived;
    }

    public void OnSystemUnloaded(LinkAccountSystem system)
    {
        system.LobbyMessageReceived -= OnLobbyMessageReceived;
    }
}
