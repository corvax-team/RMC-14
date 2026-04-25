// CM14 rework: non-RMC edit marker.
using System;
using System.Linq;
using Content.Server.Administration;
using Content.Server._CCM.Achievements;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;

namespace Content.Server._CCM.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class CCMAchievementGrantCommand : LocalizedCommands
{
    private const string LogCategory = "ccm.achievements";

    [Dependency] private readonly IPlayerLocator _locator = default!;
    [Dependency] private readonly IEntityManager _entities = default!;

    public override string Command => "ccmachgrant";
    public override string Description => "Grants a CCM achievement to a player.";
    public override string Help => "ccmachgrant <ckey|netuserid> <achievementId>";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        try
        {
            if (args.Length != 2)
            {
                shell.WriteError($"Usage: {Help}");
                return;
            }

            var player = await _locator.LookupIdByNameOrIdAsync(args[0]);
            if (player == null)
            {
                shell.WriteError("Target player was not found.");
                return;
            }

            var achievements = _entities.System<CCMAchievementSystem>();
            if (!achievements.TryNormalizeAchievementId(args[1], out var achievementId))
            {
                shell.WriteError("Unknown achievement id. Use ccmachlist to inspect valid ids.");
                return;
            }

            Logger.InfoS(LogCategory, $"Host requested achievement grant '{achievementId}' for '{player.Username}' ({player.UserId}).");

            var granted = await achievements.GrantAchievementAsync(player.UserId, achievementId);
            if (!granted)
            {
                shell.WriteError($"Achievement '{achievementId}' is already unlocked or the player data could not be loaded.");
                return;
            }

            shell.WriteLine($"Granted achievement '{achievementId}' to {player.Username}.");
        }
        catch (Exception e)
        {
            Logger.ErrorS(LogCategory, $"ccmachgrant failed: {e}");
            shell.WriteError($"ccmachgrant failed: {e.Message}");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var achievements = _entities.System<CCMAchievementSystem>();
        return args.Length switch
        {
            1 => CompletionResult.FromHint("ckey|netuserid"),
            2 => CompletionResult.FromHintOptions(achievements.GetAchievementIds().ToArray(), "achievementId"),
            _ => CompletionResult.Empty,
        };
    }
}

[AdminCommand(AdminFlags.Host)]
public sealed class CCMAchievementRevokeCommand : LocalizedCommands
{
    private const string LogCategory = "ccm.achievements";

    [Dependency] private readonly IPlayerLocator _locator = default!;
    [Dependency] private readonly IEntityManager _entities = default!;

    public override string Command => "ccmachrevoke";
    public override string Description => "Revokes a CCM achievement from a player.";
    public override string Help => "ccmachrevoke <ckey|netuserid> <achievementId>";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        try
        {
            if (args.Length != 2)
            {
                shell.WriteError($"Usage: {Help}");
                return;
            }

            var player = await _locator.LookupIdByNameOrIdAsync(args[0]);
            if (player == null)
            {
                shell.WriteError("Target player was not found.");
                return;
            }

            var achievements = _entities.System<CCMAchievementSystem>();
            if (!achievements.TryNormalizeAchievementId(args[1], out var achievementId))
            {
                shell.WriteError("Unknown achievement id. Use ccmachlist to inspect valid ids.");
                return;
            }

            Logger.InfoS(LogCategory, $"Host requested achievement revoke '{achievementId}' for '{player.Username}' ({player.UserId}).");

            var revoked = await achievements.RevokeAchievementAsync(player.UserId, achievementId);
            if (!revoked)
            {
                shell.WriteError($"Achievement '{achievementId}' is not unlocked or the player data could not be loaded.");
                return;
            }

            shell.WriteLine($"Revoked achievement '{achievementId}' from {player.Username}.");
        }
        catch (Exception e)
        {
            Logger.ErrorS(LogCategory, $"ccmachrevoke failed: {e}");
            shell.WriteError($"ccmachrevoke failed: {e.Message}");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var achievements = _entities.System<CCMAchievementSystem>();
        return args.Length switch
        {
            1 => CompletionResult.FromHint("ckey|netuserid"),
            2 => CompletionResult.FromHintOptions(achievements.GetAchievementIds().ToArray(), "achievementId"),
            _ => CompletionResult.Empty,
        };
    }
}

[AdminCommand(AdminFlags.Host)]
public sealed class CCMAchievementListCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entities = default!;

    public override string Command => "ccmachlist";
    public override string Description => "Lists CCM achievement ids.";
    public override string Help => "ccmachlist [filter]";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 1)
        {
            shell.WriteError($"Usage: {Help}");
            return;
        }

        var achievements = _entities.System<CCMAchievementSystem>().GetAchievementIds();
        var filter = args.Length == 1 ? args[0].Trim() : string.Empty;

        var filtered = string.IsNullOrWhiteSpace(filter)
            ? achievements.ToArray()
            : achievements.Where(id => id.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToArray();

        if (filtered.Length == 0)
        {
            shell.WriteLine("No achievement ids matched the filter.");
            return;
        }

        foreach (var achievementId in filtered)
        {
            shell.WriteLine(achievementId);
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var achievements = _entities.System<CCMAchievementSystem>();
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(achievements.GetAchievementIds().ToArray(), "filter"),
            _ => CompletionResult.Empty,
        };
    }
}
