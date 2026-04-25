// CM14 rework: non-RMC edit marker.
using System;
using System.Threading.Tasks;
using Content.Server._RMC14.LinkAccount;
using Content.Server._RMC14.Rules;
using Content.Server._RMC14.Rules.DistressSignal;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Shared._RMC14.Rules;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;

namespace Content.Server._CCM.RoundEnd;

public sealed class CCMRoundWinTrackerSystem : EntitySystem
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private int _marineWins;
    private int _xenoWins;
    private int? _recordedRoundId;

    public int MarineWins => _marineWins;
    public int XenoWins => _xenoWins;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend,
            after: [typeof(CMDistressSignalRuleSystem)],
            before: [typeof(LinkAccountSystem)]);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        RefreshStats();
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _recordedRoundId = null;
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        ApplyCurrentRoundResult();
    }

    public async Task<(int MarineWins, int XenoWins)> AdjustWinsAsync(string side, int delta)
    {
        var normalized = side.Trim().ToLowerInvariant();
        var marineDelta = normalized switch
        {
            "marine" or "marines" or "морпех" or "морпехи" => delta,
            _ => 0,
        };
        var xenoDelta = normalized switch
        {
            "xeno" or "xenos" or "xenonid" or "xenonids" or "ксено" or "ксенониды" => delta,
            _ => 0,
        };

        if (marineDelta == 0 && xenoDelta == 0)
            throw new ArgumentException($"Unknown side '{side}'.", nameof(side));

        var stats = await _db.AdjustCCMRoundWinStats(marineDelta, xenoDelta);
        _marineWins = stats.MarineWins;
        _xenoWins = stats.XenoWins;
        return stats;
    }

    private void ApplyCurrentRoundResult()
    {
        if (_gameTicker.RoundId == 0 || _recordedRoundId == _gameTicker.RoundId)
            return;

        if (!TryGetCurrentRoundWinner(out var marineDelta, out var xenoDelta))
        {
            _recordedRoundId = _gameTicker.RoundId;
            return;
        }

        _recordedRoundId = _gameTicker.RoundId;
        _marineWins += marineDelta;
        _xenoWins += xenoDelta;

        PersistAdjustedStats(marineDelta, xenoDelta);
    }

    private bool TryGetCurrentRoundWinner(out int marineDelta, out int xenoDelta)
    {
        marineDelta = 0;
        xenoDelta = 0;

        var query = EntityQueryEnumerator<ActiveGameRuleComponent, CMDistressSignalRuleComponent>();
        while (query.MoveNext(out _, out _, out var distress))
        {
            switch (distress.Result)
            {
                case DistressSignalRuleResult.MajorMarineVictory:
                case DistressSignalRuleResult.MinorMarineVictory:
                    marineDelta = 1;
                    return true;
                case DistressSignalRuleResult.MajorXenoVictory:
                case DistressSignalRuleResult.MinorXenoVictory:
                    xenoDelta = 1;
                    return true;
            }
        }

        return false;
    }

    private async void RefreshStats()
    {
        try
        {
            var stats = await _db.GetCCMRoundWinStats();
            _marineWins = stats.MarineWins;
            _xenoWins = stats.XenoWins;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to load round win stats:\n{e}");
        }
    }

    private async void PersistAdjustedStats(int marineDelta, int xenoDelta)
    {
        try
        {
            var stats = await _db.AdjustCCMRoundWinStats(marineDelta, xenoDelta);
            _marineWins = stats.MarineWins;
            _xenoWins = stats.XenoWins;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to persist round win stats:\n{e}");
        }
    }
}
