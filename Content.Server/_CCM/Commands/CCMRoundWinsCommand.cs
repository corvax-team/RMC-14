// CM14 rework: non-RMC edit marker.
using System;
using Content.Server.Administration;
using Content.Server._CCM.RoundEnd;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._CCM.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class CCMRoundWinsCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _systems = default!;

    public override string Command => "ccmroundwins";
    public override string Description => "Adjusts persistent marine/xeno round win counters.";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError("Usage: ccmroundwins <marines|xenos> <delta>");
            return;
        }

        if (!int.TryParse(args[1], out var delta))
        {
            shell.WriteError("Delta must be an integer.");
            return;
        }

        var tracker = _systems.GetEntitySystem<CCMRoundWinTrackerSystem>();

        try
        {
            var stats = await tracker.AdjustWinsAsync(args[0], delta);
            shell.WriteLine($"Round wins updated. Marines: {stats.MarineWins}, Xenos: {stats.XenoWins}");
        }
        catch (ArgumentException e)
        {
            shell.WriteError(e.Message);
        }
        catch (Exception e)
        {
            shell.WriteError($"Failed to update round wins: {e.Message}");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromOptions(["marines", "xenos", "морпехи", "ксенониды"]),
            2 => CompletionResult.FromHint("delta"),
            _ => CompletionResult.Empty,
        };
    }
}
