// CM14 rework: non-RMC edit marker.
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
        if (LatestSnapshot != null)
        {
            LatestSnapshot = MergeSnapshot(LatestSnapshot, ev);
            AchievementsReceived?.Invoke(LatestSnapshot);
        }

        AchievementUnlocked?.Invoke(ev);
    }

    private static CCMAchievementsSnapshot MergeSnapshot(CCMAchievementsSnapshot snapshot, CCMAchievementUnlockedEvent ev)
    {
        var updated = new CCMAchievementProgressData[snapshot.Achievements.Length];
        var found = false;

        for (var i = 0; i < snapshot.Achievements.Length; i++)
        {
            var achievement = snapshot.Achievements[i];
            if (!achievement.Id.Equals(ev.Achievement.Id, StringComparison.Ordinal))
            {
                updated[i] = achievement;
                continue;
            }

            found = true;
            updated[i] = new CCMAchievementProgressData(
                achievement.Id,
                achievement.Category,
                achievement.TitleKey,
                achievement.DescriptionKey,
                ev.Achievement.Progress,
                achievement.Goal,
                true);
        }

        if (found)
            return new CCMAchievementsSnapshot(ev.CompletedCount, ev.TotalCount, updated);

        var expanded = new CCMAchievementProgressData[snapshot.Achievements.Length + 1];
        Array.Copy(updated, expanded, updated.Length);
        expanded[^1] = ev.Achievement;
        return new CCMAchievementsSnapshot(ev.CompletedCount, ev.TotalCount, expanded);
    }
}
