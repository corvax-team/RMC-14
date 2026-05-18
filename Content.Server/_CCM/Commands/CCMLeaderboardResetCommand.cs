// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Linq;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared._CCM.Stats;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Log;

namespace Content.Server._CCM.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class CCMLeaderboardResetCommand : LocalizedCommands
{
    private const string LogCategory = "ccm.leaderboard";

    private static readonly Dictionary<string, CCMLeaderboardCategory> CategoryAliases = new()
    {
        ["overallvp"] = CCMLeaderboardCategory.OverallVictoryPoints,
        ["overallvictorypoints"] = CCMLeaderboardCategory.OverallVictoryPoints,
        ["overallkills"] = CCMLeaderboardCategory.OverallKills,
        ["marinevp"] = CCMLeaderboardCategory.MarineVictoryPoints,
        ["marinevictorypoints"] = CCMLeaderboardCategory.MarineVictoryPoints,
        ["marineimpact"] = CCMLeaderboardCategory.MarineImpact,
        ["marinekills"] = CCMLeaderboardCategory.MarineKills,
        ["xenovp"] = CCMLeaderboardCategory.XenoVictoryPoints,
        ["xenovictorypoints"] = CCMLeaderboardCategory.XenoVictoryPoints,
        ["xenoimpact"] = CCMLeaderboardCategory.XenoImpact,
        ["xenokills"] = CCMLeaderboardCategory.XenoKills,
    };

    private static readonly Dictionary<string, CCMLeaderboardTimeframe> TimeframeAliases = new()
    {
        ["alltime"] = CCMLeaderboardTimeframe.AllTime,
        ["month"] = CCMLeaderboardTimeframe.CurrentMonth,
        ["monthly"] = CCMLeaderboardTimeframe.CurrentMonth,
        ["currentmonth"] = CCMLeaderboardTimeframe.CurrentMonth,
    };

    [Dependency] private readonly IServerDbManager _db = default!;

    public override string Command => "ccmleaderboardreset";
    public override string Description => "Resets a CCM leaderboard page by setting a new zero point.";
    public override string Help => "ccmleaderboardreset <category|all> <alltime|month>";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        try
        {
            if (args.Length != 2)
            {
                shell.WriteError($"Usage: {Help}");
                return;
            }

            if (!TryParseTimeframe(args[1], out var timeframe))
            {
                shell.WriteError("Unknown timeframe. Use alltime or month.");
                return;
            }

            if (NormalizeToken(args[0]) == "all")
            {
                var totalPlayers = 0;
                foreach (var leaderboardCategory in Enum.GetValues<CCMLeaderboardCategory>())
                {
                    totalPlayers += await _db.ResetCCMLeaderboard(leaderboardCategory, timeframe);
                }

                Logger.InfoS(LogCategory, $"Host reset all CCM leaderboard categories for {timeframe}.");
                shell.WriteLine($"Reset all CCM leaderboard categories for {FormatTimeframe(timeframe)}. Snapshotted scores: {totalPlayers}.");
                return;
            }

            if (!TryParseCategory(args[0], out var category))
            {
                shell.WriteError("Unknown category. Use overall_vp, overall_kills, marine_vp, marine_impact, marine_kills, xeno_vp, xeno_impact or xeno_kills.");
                return;
            }

            var affected = await _db.ResetCCMLeaderboard(category, timeframe);
            Logger.InfoS(LogCategory, $"Host reset CCM leaderboard {category} for {timeframe}. Snapshotted players: {affected}.");
            shell.WriteLine($"Reset {FormatCategory(category)} for {FormatTimeframe(timeframe)}. Snapshotted players: {affected}.");
        }
        catch (Exception e)
        {
            Logger.ErrorS(LogCategory, $"ccmleaderboardreset failed: {e}");
            shell.WriteError($"ccmleaderboardreset failed: {e.Message}");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromOptions([
                "overall_vp",
                "overall_kills",
                "marine_vp",
                "marine_impact",
                "marine_kills",
                "xeno_vp",
                "xeno_impact",
                "xeno_kills",
                "all"
            ]),
            2 => CompletionResult.FromOptions(["alltime", "month"]),
            _ => CompletionResult.Empty,
        };
    }

    private static bool TryParseCategory(string value, out CCMLeaderboardCategory category)
    {
        return CategoryAliases.TryGetValue(NormalizeToken(value), out category);
    }

    private static bool TryParseTimeframe(string value, out CCMLeaderboardTimeframe timeframe)
    {
        return TimeframeAliases.TryGetValue(NormalizeToken(value), out timeframe);
    }

    private static string NormalizeToken(string value)
    {
        return new string(value
            .Trim()
            .ToLowerInvariant()
            .Where(ch => ch != '_' && ch != '-' && ch != ' ')
            .ToArray());
    }

    private static string FormatCategory(CCMLeaderboardCategory category)
    {
        return category switch
        {
            CCMLeaderboardCategory.OverallVictoryPoints => "overall victory points",
            CCMLeaderboardCategory.OverallKills => "overall kills",
            CCMLeaderboardCategory.MarineVictoryPoints => "marine victory points",
            CCMLeaderboardCategory.MarineImpact => "marine impact",
            CCMLeaderboardCategory.MarineKills => "marine kills",
            CCMLeaderboardCategory.XenoVictoryPoints => "xeno victory points",
            CCMLeaderboardCategory.XenoImpact => "xeno impact",
            CCMLeaderboardCategory.XenoKills => "xeno kills",
            _ => category.ToString(),
        };
    }

    private static string FormatTimeframe(CCMLeaderboardTimeframe timeframe)
    {
        return timeframe switch
        {
            CCMLeaderboardTimeframe.AllTime => "all-time leaderboard",
            CCMLeaderboardTimeframe.CurrentMonth => "current month leaderboard",
            _ => timeframe.ToString(),
        };
    }
}
