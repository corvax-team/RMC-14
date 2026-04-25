using System;
using Content.Server.Administration;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CCM.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class JobPriorityWeightOverrideCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public string Command => "ccmjobweight";
    public string Description => "Overrides first-order weight inputs for a player (not persisted).";
    public string Help =>
        "ccmjobweight set <username|netuserid> <slot> <jobId> <missedRounds> <assignedLastRound true|false> <sessionHours> [externalBonus]\n" +
        "ccmjobweight clear <username|netuserid>\n" +
        "ccmjobweight clearjob <username|netuserid> <slot> <jobId>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteLine(Help);
            return;
        }

        if (!TryGetSession(args[1], out var session) || session == null)
        {
            shell.WriteLine("Player not found or offline.");
            return;
        }

        var jobs = _entitySystemManager.GetEntitySystem<StationJobsSystem>();
        var sub = args[0].ToLowerInvariant();

        switch (sub)
        {
            case "set":
                HandleSet(shell, jobs, session, args);
                break;
            case "clear":
                jobs.ClearFirstOrderWeightOverrides(session.UserId);
                jobs.ClearSessionMinutesOverride(session.UserId);
                jobs.ClearExternalWeightOverride(session.UserId);
                shell.WriteLine("Overrides cleared.");
                break;
            case "clearjob":
                HandleClearJob(shell, jobs, session, args);
                break;
            default:
                shell.WriteLine(Help);
                break;
        }
    }

    private void HandleSet(IConsoleShell shell, StationJobsSystem jobs, ICommonSession session, string[] args)
    {
        if (args.Length is < 7 or > 8)
        {
            shell.WriteLine(Help);
            return;
        }

        if (!int.TryParse(args[2], out var slot))
        {
            shell.WriteLine("Slot must be a number.");
            return;
        }

        var jobId = args[3];
        if (!_prototypeManager.HasIndex<JobPrototype>(jobId))
        {
            shell.WriteLine($"Unknown job id '{jobId}'.");
            return;
        }

        if (!int.TryParse(args[4], out var missedRounds))
        {
            shell.WriteLine("Missed rounds must be a number.");
            return;
        }

        if (!bool.TryParse(args[5], out var assignedLastRound))
        {
            shell.WriteLine("assignedLastRound must be true or false.");
            return;
        }

        if (!float.TryParse(args[6], out var sessionHours))
        {
            shell.WriteLine("Session hours must be a number.");
            return;
        }

        jobs.SetFirstOrderWeightOverride(
            session.UserId,
            slot,
            new ProtoId<JobPrototype>(jobId),
            missedRounds,
            assignedLastRound);

        jobs.SetSessionMinutesOverride(session.UserId, sessionHours * 60f);

        if (args.Length == 8)
        {
            if (!float.TryParse(args[7], out var externalBonus))
            {
                shell.WriteLine("External bonus must be a number.");
                return;
            }

            jobs.SetExternalWeightOverride(session.UserId, externalBonus);
        }

        shell.WriteLine("Overrides set (not persisted).");
    }

    private void HandleClearJob(IConsoleShell shell, StationJobsSystem jobs, ICommonSession session, string[] args)
    {
        if (args.Length != 4)
        {
            shell.WriteLine(Help);
            return;
        }

        if (!int.TryParse(args[2], out var slot))
        {
            shell.WriteLine("Slot must be a number.");
            return;
        }

        var jobId = args[3];
        if (!_prototypeManager.HasIndex<JobPrototype>(jobId))
        {
            shell.WriteLine($"Unknown job id '{jobId}'.");
            return;
        }

        jobs.ClearFirstOrderWeightOverride(session.UserId, slot, new ProtoId<JobPrototype>(jobId));
        shell.WriteLine("Job override cleared.");
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
