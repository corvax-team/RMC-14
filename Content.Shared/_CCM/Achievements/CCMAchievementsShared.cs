// CM14 rework: non-RMC edit marker.
using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Achievements;

[Serializable, NetSerializable]
public enum CCMAchievementCategory : byte
{
    General = 0,
    Misc,
    Xenos,
    Marines,
}

[Serializable, NetSerializable]
public sealed class CCMAchievementProgressData
{
    public string Id { get; }
    public CCMAchievementCategory Category { get; }
    public string TitleKey { get; }
    public string DescriptionKey { get; }
    public int Progress { get; }
    public int Goal { get; }
    public bool Completed { get; }

    public CCMAchievementProgressData(
        string id,
        CCMAchievementCategory category,
        string titleKey,
        string descriptionKey,
        int progress,
        int goal,
        bool completed)
    {
        Id = id;
        Category = category;
        TitleKey = titleKey;
        DescriptionKey = descriptionKey;
        Progress = progress;
        Goal = goal;
        Completed = completed;
    }
}

[Serializable, NetSerializable]
public sealed class CCMAchievementsSnapshot
{
    public int CompletedCount { get; }
    public int TotalCount { get; }
    public CCMAchievementProgressData[] Achievements { get; }

    public CCMAchievementsSnapshot(int completedCount, int totalCount, CCMAchievementProgressData[] achievements)
    {
        CompletedCount = completedCount;
        TotalCount = totalCount;
        Achievements = achievements;
    }
}

[Serializable, NetSerializable]
public sealed class CCMPlayerAchievementStatsSnapshot
{
    public int FriendlyFireDamage { get; }
    public int RequisitionOrders { get; }
    public int XenoEvolutions { get; }
    public int OfficerWins { get; }
    public int QueenKills { get; }
    public int QueenWins { get; }
    public int QueenKillParticipations { get; }
    public string[] UnlockedIds { get; }

    public CCMPlayerAchievementStatsSnapshot(
        int friendlyFireDamage,
        int requisitionOrders,
        int xenoEvolutions,
        int officerWins,
        int queenKills,
        int queenWins,
        int queenKillParticipations,
        string[] unlockedIds)
    {
        FriendlyFireDamage = friendlyFireDamage;
        RequisitionOrders = requisitionOrders;
        XenoEvolutions = xenoEvolutions;
        OfficerWins = officerWins;
        QueenKills = queenKills;
        QueenWins = queenWins;
        QueenKillParticipations = queenKillParticipations;
        UnlockedIds = unlockedIds;
    }
}

[Serializable, NetSerializable]
public sealed class RequestCCMAchievementsEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class CCMAchievementsResponseEvent : EntityEventArgs
{
    public CCMAchievementsSnapshot Snapshot { get; }

    public CCMAchievementsResponseEvent(CCMAchievementsSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}

[Serializable, NetSerializable]
public sealed class CCMAchievementUnlockedEvent : EntityEventArgs
{
    public CCMAchievementProgressData Achievement { get; }
    public int CompletedCount { get; }
    public int TotalCount { get; }

    public CCMAchievementUnlockedEvent(CCMAchievementProgressData achievement, int completedCount, int totalCount)
    {
        Achievement = achievement;
        CompletedCount = completedCount;
        TotalCount = totalCount;
    }
}
