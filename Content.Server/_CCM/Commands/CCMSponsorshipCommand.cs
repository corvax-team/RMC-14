using System;
using System.Linq;
using System.Threading.Tasks;
using Content.Server.Administration;
using Content.Server._CCM.Sponsorship;
using Content.Shared._CCM.Sponsorship;
using Content.Shared.Administration;
using Robust.Shared.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.Log;
using Robust.Shared.Player;

namespace Content.Server._CCM.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class CCMSponsorshipCommand : LocalizedCommands
{
    [Dependency] private readonly ISharedPlayerManager _players = default!;
    [Dependency] private readonly CCMSponsorshipManager _sponsorship = default!;
    [Dependency] private readonly IEntityManager _entities = default!;

    public override string Command => "ccmsponsor";
    public override string Description => "Sets or clears a persisted sponsorship tier for an online player.";
    public override string Help => "ccmsponsor <ckey> <none|1|2|3|sponsor1|sponsor2|sponsor3> [days]";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        try
        {
            if (args.Length is < 2 or > 3)
            {
                shell.WriteError($"Usage: {Help}");
                return;
            }

            if (!_players.TryGetSessionByUsername(args[0], out var session))
            {
                shell.WriteError("Target player is not online.");
                return;
            }

            if (!TryParseTier(args[1], out var tier))
            {
                shell.WriteError("Tier must be one of: none, 1, 2, 3, sponsor1, sponsor2, sponsor3.");
                return;
            }

            var days = 30;
            if (args.Length == 3 && (!int.TryParse(args[2], out days) || days < 1))
            {
                shell.WriteError("Days must be a positive integer.");
                return;
            }

            var expiration = tier == CCMSponsorshipTier.None
                ? 0
                : DateTimeOffset.UtcNow.AddDays(Math.Max(1, days)).ToUnixTimeSeconds();

            _sponsorship.SetManualTierOverride(session.UserId, tier, expiration);
            var sponsorshipSystem = _entities.System<CCMSponsorshipSystem>();
            sponsorshipSystem.PushStatus(session);
            _ = Task.Run(async () =>
            {
                try
                {
                    await _sponsorship.PersistTier(session.UserId, tier, days);
                }
                catch (Exception e)
                {
                    Logger.ErrorS("ccm.sponsor", $"Failed to persist sponsorship for {session.Name}: {e}");
                }
            });

            if (tier == CCMSponsorshipTier.None)
            {
                shell.WriteLine($"Cleared sponsorship for {session.Name}. Runtime override applied immediately, database update queued.");
                return;
            }

            shell.WriteLine($"Set sponsorship for {session.Name} to {tier} for {days} day(s). Runtime override applied immediately, database update queued.");
        }
        catch (Exception e)
        {
            Logger.ErrorS("ccm.sponsor", $"ccmsponsor failed: {e}");
            shell.WriteError($"ccmsponsor failed: {e.Message}");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(
                _players.Sessions.Select(s => s.Name).OrderBy(n => n).ToArray(),
                "ckey"),
            2 => CompletionResult.FromOptions(["none", "1", "2", "3", "sponsor1", "sponsor2", "sponsor3"]),
            _ => CompletionResult.Empty,
        };
    }

    private static bool TryParseTier(string arg, out CCMSponsorshipTier tier)
    {
        switch (arg.Trim().ToLowerInvariant())
        {
            case "none":
            case "0":
            case "remove":
            case "clear":
                tier = CCMSponsorshipTier.None;
                return true;
            case "1":
            case "i":
            case "sponsor1":
            case "sponsori":
                tier = CCMSponsorshipTier.SponsorI;
                return true;
            case "2":
            case "ii":
            case "sponsor2":
            case "sponsorii":
                tier = CCMSponsorshipTier.SponsorII;
                return true;
            case "3":
            case "iii":
            case "sponsor3":
            case "sponsoriii":
                tier = CCMSponsorshipTier.SponsorIII;
                return true;
            default:
                tier = CCMSponsorshipTier.None;
                return false;
        }
    }
}
