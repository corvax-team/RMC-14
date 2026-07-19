using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._Forge.DiscordAuth;

/// <summary>
///     Client -> server. Asks the server to (re)send <see cref="MsgDiscordAuthRequired"/> with a
///     fresh link + QR code so the player can open the Discord linking UI on demand (e.g. from the
///     lobby button) instead of only at connect time.
/// </summary>
public sealed class MsgDiscordAuthRequest : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
    }
}
