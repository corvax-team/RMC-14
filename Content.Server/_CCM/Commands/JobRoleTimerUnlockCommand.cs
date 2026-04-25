using System;
using System.Linq;
using Content.Server.Administration;
using Content.Server._RMC14.PlayTimeTracking;
using Content.Shared.Administration;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CCM.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class JobRoleTimerUnlockCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly RMCPlayTimeManager _playTime = default!;

    public string Command => "ccmjobtimerunlock";
    public string Description => "Disables all job timer restrictions for a player (adds exclusions for all jobs).";
    public string Help => "ccmjobtimerunlock <username|netuserid>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine(Help);
            return;
        }

        if (!TryGetSession(args[0], out var session) || session == null)
        {
            shell.WriteLine("Player not found or offline.");
            return;
        }

        var jobs = _prototypeManager.EnumeratePrototypes<JobPrototype>().ToList();
        var excludedCount = 0;

        foreach (var job in jobs)
        {
            var excluded = _playTime.Exclude(session.UserId, job.ID).GetAwaiter().GetResult();
            if (excluded)
                excludedCount++;
        }

        shell.WriteLine($"Excluded {excludedCount} of {jobs.Count} jobs for {session.Name}.");
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
