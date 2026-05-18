// CM14 rework: non-RMC edit marker.
using System;
using Content.Shared._CCM.Stats;

namespace Content.Client._CCM.Stats;

public sealed class CCMStatsSystem : EntitySystem
{
    public event Action<CCMPlayerStatsSnapshot>? PlayerStatsReceived;
    public event Action<CCMLeaderboardPage>? LeaderboardReceived;
    public event Action<CCMRoundEndStatsEvent>? RoundEndStatsReceived;

    public CCMRoundEndStatsEvent? LatestRoundEndStats { get; private set; }

    public override void Initialize()
    {
        SubscribeNetworkEvent<CCMPlayerStatsResponseEvent>(OnPlayerStatsResponse);
        SubscribeNetworkEvent<CCMLeaderboardResponseEvent>(OnLeaderboardResponse);
        SubscribeNetworkEvent<CCMRoundEndStatsEvent>(OnRoundEndStats);
    }

    public void RequestPlayerStats()
    {
        RaiseNetworkEvent(new RequestCCMPlayerStatsEvent());
    }

    public void RequestLeaderboard(CCMLeaderboardCategory category, CCMLeaderboardTimeframe timeframe, int page)
    {
        RaiseNetworkEvent(new RequestCCMLeaderboardEvent(category, timeframe, page));
    }

    public void ClearLatestRoundEndStats()
    {
        LatestRoundEndStats = null;
    }

    private void OnPlayerStatsResponse(CCMPlayerStatsResponseEvent ev)
    {
        PlayerStatsReceived?.Invoke(ev.Stats);
    }

    private void OnLeaderboardResponse(CCMLeaderboardResponseEvent ev)
    {
        LeaderboardReceived?.Invoke(ev.PageData);
    }

    private void OnRoundEndStats(CCMRoundEndStatsEvent ev)
    {
        LatestRoundEndStats = ev;
        RoundEndStatsReceived?.Invoke(ev);
    }
}
