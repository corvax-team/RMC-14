using System.Linq;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Network;

namespace Content.Server.Connection;

[AdminCommand(AdminFlags.Host)]
public sealed class HiddenBanCommand : LocalizedCommands
{
    [Dependency] private readonly IPlayerLocator _locator = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IServerNetManager _net = default!;

    public override string Command => "hiddenban";
    public override string Description => "Adds or removes a hidden account ban that mimics a dead remote host.";
    public override string Help => "hiddenban <ckey|netuserid> <add|remove>";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError($"Usage: {Help}");
            return;
        }

        var target = await _locator.LookupIdByNameOrIdAsync(args[0]);
        if (target == null)
        {
            shell.WriteError("Target player was not found.");
            return;
        }

        var action = args[1].Trim().ToLowerInvariant();
        switch (action)
        {
            case "add":
            case "on":
            case "ban":
            {
                if (await _db.GetHiddenBanStatusAsync(target.UserId))
                {
                    shell.WriteLine($"Hidden ban already exists for {target.Username}.");
                    return;
                }

                await _db.AddHiddenBanAsync(target.UserId);

                if (_players.TryGetSessionById(target.UserId, out var session))
                    _net.DisconnectChannel(session.Channel, ConnectionManager.HiddenBanDisconnectReason);

                shell.WriteLine($"Hidden ban added for {target.Username}.");
                return;
            }

            case "remove":
            case "off":
            case "unban":
            case "clear":
            {
                if (!await _db.GetHiddenBanStatusAsync(target.UserId))
                {
                    shell.WriteLine($"Hidden ban does not exist for {target.Username}.");
                    return;
                }

                await _db.RemoveHiddenBanAsync(target.UserId);
                shell.WriteLine($"Hidden ban removed for {target.Username}.");
                return;
            }

            default:
                shell.WriteError("Action must be one of: add, remove.");
                return;
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(
                _players.Sessions.Select(s => s.Name).OrderBy(n => n).ToArray(),
                "ckey"),
            2 => CompletionResult.FromOptions(["add", "remove"]),
            _ => CompletionResult.Empty,
        };
    }
}
