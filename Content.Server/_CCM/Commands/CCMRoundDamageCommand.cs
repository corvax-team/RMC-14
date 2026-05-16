using System;
using Content.Server.Administration;
using Content.Server._CCM.Stats;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._CCM.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class CCMRoundDamageCommand : IConsoleCommand
{
    [Dependency] private readonly IEntitySystemManager _systems = default!;

    public string Command => "ccmrounddamage";
    public string Description => "Shows live CCM round combat totals, including damage, healing, and recent diagnostics.";
    public string Help => "ccmrounddamage [limit]";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var limit = 15;
        if (args.Length > 1)
        {
            shell.WriteError(Help);
            return;
        }

        if (args.Length == 1 && (!int.TryParse(args[0], out limit) || limit < 1))
        {
            shell.WriteError("Limit must be a positive integer.");
            return;
        }

        var stats = _systems.GetEntitySystem<CCMStatsSystem>();
        var snapshot = stats.GetDamageDebugSnapshot(limit);

        shell.WriteLine(
            $"Attributed damage: marines={snapshot.TotalMarineDamage}, xenos={snapshot.TotalXenoDamage}");
        shell.WriteLine(
            $"Attributed healing: marines={snapshot.TotalMarineHealing}, xenos={snapshot.TotalXenoHealing}");
        shell.WriteLine(
            $"Unattributed hits: to marines={snapshot.UnattributedHitsToMarines} ({snapshot.UnattributedDamageToMarines} dmg), " +
            $"to xenos={snapshot.UnattributedHitsToXenos} ({snapshot.UnattributedDamageToXenos} dmg)");
        shell.WriteLine(
            $"Fallback unknown-target hits: marines={snapshot.FallbackMarineHits} ({snapshot.FallbackMarineDamage} dmg), " +
            $"xenos={snapshot.FallbackXenoHits} ({snapshot.FallbackXenoDamage} dmg)");

        if (snapshot.Players.Length == 0)
        {
            shell.WriteLine("No players with recorded damage yet.");
        }
        else
        {
            shell.WriteLine("Top players by live recorded damage:");
            foreach (var player in snapshot.Players)
            {
                shell.WriteLine(
                    $"  {player.Ckey} [{player.Name}] side={player.LastKnownSide} " +
                    $"marineDamage={player.MarineDamage} marineHealing={player.MarineHealingDone} " +
                    $"xenoDamage={player.XenoDamage} xenoHealing={player.XenoHealingDone} " +
                    $"marineKills={player.MarineKills} xenoKills={player.XenoKills} " +
                    $"marinePlayed={player.MarineParticipated} xenoPlayed={player.XenoParticipated}");
            }
        }

        if (snapshot.RecentDiagnostics.Length == 0)
        {
            shell.WriteLine("Recent diagnostics: none.");
            return;
        }

        shell.WriteLine("Recent diagnostics:");
        foreach (var entry in snapshot.RecentDiagnostics)
        {
            var seconds = Math.Round(entry.Time.TotalSeconds, 1);
            shell.WriteLine(
                $"  t={seconds}s reason={entry.Reason} side={entry.Side} damage={entry.Damage:0.##} " +
                $"target={entry.Target} origin={entry.Origin} tool={entry.Tool}");
        }
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHint("limit"),
            _ => CompletionResult.Empty,
        };
    }
}
