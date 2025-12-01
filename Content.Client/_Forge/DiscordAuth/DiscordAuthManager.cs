using Content.Shared._Forge.DiscordAuth;
using Robust.Client.State;
using Robust.Shared.Network;

namespace Content.Client._Forge.DiscordAuth;

public sealed class DiscordAuthManager
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IStateManager _state = default!;

    public string AuthLink = default!;
    public string ErrorMessage = default!;
    public const string DiscordServerLink = "https://discord.gg/D3rSfbwZpx";

    public void Initialize()
    {
        _net.RegisterNetMessage<MsgDiscordAuthRequired>(OnDiscordAuthRequired);
    }

    public void OnDiscordAuthRequired(MsgDiscordAuthRequired args)
    {
        AuthLink = args.Link;
        ErrorMessage = args.ErrorMessage;

        // QR-код больше не используется
        _state.RequestStateChange<DiscordAuthState>();
    }

    public void OnAuthSkip()
    {
        _net.ClientSendMessage(new MsgDiscordAuthSkip());
    }
}
