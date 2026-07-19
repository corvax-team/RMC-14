// Forge port: the legacy "request OAuth link from server" flow is gone — Forge's
// DiscordAuthManager handles verification at connect time. This controller now
// just opens the Discord server invite so the lobby button still does something.
using Content.Client._Forge.DiscordAuth;
using Content.Shared._Forge.DiscordAuth;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Network;

namespace Content.Client._RMC14.LinkAccount;

public sealed class DiscordOAuthUIController : UIController
{
    [Dependency] private readonly IUriOpener _uri = default!;
    [Dependency] private readonly INetManager _net = default!;

    public void OpenLink()
    {
        _uri.OpenUri(DiscordAuthManager.DiscordServerLink);
    }

    public void OnDiscordAuthRequest()
    {
        _net.ClientSendMessage(new MsgDiscordAuthRequest());
    }
}
