using System;
using Content.Shared._CCM.Achievements;

namespace Content.Client._CCM.Achievements;

public sealed class CCMAchievementsSystem : EntitySystem
{
    public event Action<CCMAchievementsSnapshot>? AchievementsReceived;
    public event Action<CCMAchievementUnlockedEvent>? AchievementUnlocked;

    public CCMAchievementsSnapshot? LatestSnapshot { get; private set; }

    public override void Initialize()
    {
        SubscribeNetworkEvent<CCMAchievementsResponseEvent>(OnAchievementsResponse);
        SubscribeNetworkEvent<CCMAchievementUnlockedEvent>(OnAchievementUnlocked);
    }

    public void RequestAchievements()
    {
        RaiseNetworkEvent(new RequestCCMAchievementsEvent());
    }

    private void OnAchievementsResponse(CCMAchievementsResponseEvent ev)
    {
        LatestSnapshot = ev.Snapshot;
        AchievementsReceived?.Invoke(ev.Snapshot);
    }

    private void OnAchievementUnlocked(CCMAchievementUnlockedEvent ev)
    {
        AchievementUnlocked?.Invoke(ev);
    }
}
