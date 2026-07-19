using Content.Server._RMC14.Rules.DistressSignal;
using Content.Server.Administration.Logs;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.GhostColor;
using Content.Shared._RMC14.LinkAccount;
using Content.Shared._RMC14.Mentor.ImaginaryFriend;
using Content.Shared.Database;
using Content.Shared.Ghost;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._RMC14.LinkAccount;

public sealed class LinkAccountSystem : EntitySystem
{
    public override void Initialize()
    {
    }
}
