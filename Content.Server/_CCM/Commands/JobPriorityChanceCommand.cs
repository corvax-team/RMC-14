using System;
using System.Linq;
using Content.Server.Administration;
using Content.Server.Preferences.Managers;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Preferences;
using Robust.Shared.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._CCM.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class JobPriorityChanceCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public string Command => "ccmjobchance";
    public string Description => "Shows first-order job chance formula for a player profile.";
    public string Help => "ccmjobchance <username|netuserid> [slot]";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is < 1 or > 2)
        {
            shell.WriteLine(Help);
            return;
        }

        if (!TryGetSession(args[0], out var session) || session == null)
        {
            shell.WriteLine("Player not found or offline.");
            return;
        }

        var prefs = _prefs.GetPreferencesOrNull(session.UserId);
        if (prefs == null)
        {
            shell.WriteLine("Player preferences are not loaded.");
            return;
        }

        var jobs = _entitySystemManager.GetEntitySystem<StationJobsSystem>();
        if (args.Length == 2)
        {
            if (!int.TryParse(args[1], out var slot))
            {
                shell.WriteLine("Slot must be a number.");
                return;
            }

            PrintSlotReport(shell, jobs, session.UserId, prefs, slot);
            return;
        }

        foreach (var (slot, character) in prefs.Characters.OrderBy(pair => pair.Key))
        {
            if (character is not HumanoidCharacterProfile humanoid)
                continue;

            PrintSlotReport(shell, jobs, session.UserId, prefs, slot, humanoid);
        }
    }

    private void PrintSlotReport(
        IConsoleShell shell,
        StationJobsSystem jobs,
        NetUserId userId,
        PlayerPreferences prefs,
        int slot,
        HumanoidCharacterProfile? humanoidOverride = null)
    {
        if (!prefs.Characters.TryGetValue(slot, out var profile) && humanoidOverride == null)
        {
            shell.WriteLine("Slot not found.");
            return;
        }

        var humanoid = humanoidOverride ?? profile as HumanoidCharacterProfile;
        if (humanoid == null)
        {
            shell.WriteLine($"slot={slot} not a humanoid profile.");
            return;
        }

        shell.WriteLine($"slot={slot} name={humanoid.Name}");

        if (!jobs.TryGetFirstOrderChanceReport(userId, humanoid, slot, out var report, out var warning))
        {
            shell.WriteLine(warning ?? "No first-order jobs found.");
            return;
        }

        if (warning != null)
            shell.WriteLine(warning);

        foreach (var entry in report)
        {
            var breakdown = entry.Breakdown;
            var formula =
                $"base {breakdown.BaseWeight:0.##} + " +
                $"missed({breakdown.MissedRounds}) {breakdown.MissedWeight:0.##} + " +
                $"recent {breakdown.RecentPenalty:0.##} + " +
                $"session({breakdown.SessionBonusSteps}) {breakdown.SessionBonus:0.##} + " +
                $"external {breakdown.ExternalBonus:0.##} = {breakdown.TotalWeight:0.##}";

            shell.WriteLine(
                $"job={entry.JobId} priority=first chance={entry.ChancePercent:0.##}% " +
                $"weight={entry.Weight:0.##}/{entry.TotalWeight:0.##} ({formula})");
        }
    }

    private bool TryGetSession(string input, out ICommonSession? session)
    {
        if (_player.TryGetSessionByUsername(input, out session))
            return true;

        if (Guid.TryParse(input, out var guid))
            return _player.TryGetSessionById(new NetUserId(guid), out session);

        session = null;
        return false;
    }
}

// # CCM priority rework
