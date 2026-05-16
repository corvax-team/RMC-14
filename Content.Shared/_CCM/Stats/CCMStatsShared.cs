// CM14 rework: non-RMC edit marker.
using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._CCM.Stats;

[Serializable, NetSerializable]
public enum CCMStatsSide : byte
{
    None = 0,
    Marines,
    Xenos,
}

[Serializable, NetSerializable]
public enum CCMLeaderboardCategory : byte
{
    OverallVictoryPoints = 0,
    OverallKills,
    MarineVictoryPoints,
    MarineImpact,
    MarineKills,
    XenoVictoryPoints,
    XenoImpact,
    XenoKills,
}

[Serializable, NetSerializable]
public enum CCMLeaderboardTimeframe : byte
{
    AllTime = 0,
    CurrentMonth,
}

[Serializable, NetSerializable]
public sealed class CCMLeaderboardEntry
{
    public int Rank { get; }
    public string Ckey { get; }
    public int Score { get; }
    public bool IsViewer { get; }

    public CCMLeaderboardEntry(int rank, string ckey, int score, bool isViewer = false)
    {
        Rank = rank;
        Ckey = ckey;
        Score = score;
        IsViewer = isViewer;
    }
}

[Serializable, NetSerializable]
public sealed class CCMLeaderboardPage
{
    public CCMLeaderboardCategory Category { get; }
    public CCMLeaderboardTimeframe Timeframe { get; }
    public int Page { get; }
    public int TotalPages { get; }
    public CCMLeaderboardEntry[] Entries { get; }
    public CCMLeaderboardEntry? ViewerEntry { get; }

    public CCMLeaderboardPage(
        CCMLeaderboardCategory category,
        CCMLeaderboardTimeframe timeframe,
        int page,
        int totalPages,
        CCMLeaderboardEntry[] entries,
        CCMLeaderboardEntry? viewerEntry)
    {
        Category = category;
        Timeframe = timeframe;
        Page = page;
        TotalPages = totalPages;
        Entries = entries;
        ViewerEntry = viewerEntry;
    }
}

[Serializable, NetSerializable]
public sealed class CCMPlayerStatsSnapshot
{
    public int RoundsPlayed { get; }
    public int RoundsWon { get; }
    public int RoundsLost { get; }
    public int RoundSecondsPlayed { get; }
    public int TotalDamageDealt { get; }
    public int TotalKills { get; }
    public int VictoryPoints { get; }
    public int ImpactPoints { get; }
    public int Revives { get; }
    public int HealingDone { get; }
    public int StructuresBuilt { get; }
    public int Deaths { get; }
    public int ShotsFired { get; }

    public int MarineRoundsPlayed { get; }
    public int MarineRoundsWon { get; }
    public int MarineRoundsLost { get; }
    public int MarineDamageDealt { get; }
    public int MarineKills { get; }
    public int MarineVictoryPoints { get; }
    public int MarineImpactPoints { get; }
    public int MarineRevives { get; }
    public int MarineHealingDone { get; }
    public int MarineStructuresBuilt { get; }
    public int MarineDeaths { get; }
    public int MarineShotsFired { get; }

    public int XenoRoundsPlayed { get; }
    public int XenoRoundsWon { get; }
    public int XenoRoundsLost { get; }
    public int XenoDamageDealt { get; }
    public int XenoKills { get; }
    public int XenoVictoryPoints { get; }
    public int XenoImpactPoints { get; }
    public int XenoHealingDone { get; }
    public int XenoStructuresBuilt { get; }
    public int XenoDeaths { get; }
    public int XenoShotsFired { get; }

    public CCMPlayerStatsSnapshot(
        int roundsPlayed,
        int roundsWon,
        int roundsLost,
        int roundSecondsPlayed,
        int totalDamageDealt,
        int totalKills,
        int victoryPoints,
        int impactPoints,
        int revives,
        int healingDone,
        int structuresBuilt,
        int deaths,
        int shotsFired,
        int marineRoundsPlayed,
        int marineRoundsWon,
        int marineRoundsLost,
        int marineDamageDealt,
        int marineKills,
        int marineVictoryPoints,
        int marineImpactPoints,
        int marineRevives,
        int marineHealingDone,
        int marineStructuresBuilt,
        int marineDeaths,
        int marineShotsFired,
        int xenoRoundsPlayed,
        int xenoRoundsWon,
        int xenoRoundsLost,
        int xenoDamageDealt,
        int xenoKills,
        int xenoVictoryPoints,
        int xenoImpactPoints,
        int xenoHealingDone,
        int xenoStructuresBuilt,
        int xenoDeaths,
        int xenoShotsFired)
    {
        RoundsPlayed = roundsPlayed;
        RoundsWon = roundsWon;
        RoundsLost = roundsLost;
        RoundSecondsPlayed = roundSecondsPlayed;
        TotalDamageDealt = totalDamageDealt;
        TotalKills = totalKills;
        VictoryPoints = victoryPoints;
        ImpactPoints = impactPoints;
        Revives = revives;
        HealingDone = healingDone;
        StructuresBuilt = structuresBuilt;
        Deaths = deaths;
        ShotsFired = shotsFired;
        MarineRoundsPlayed = marineRoundsPlayed;
        MarineRoundsWon = marineRoundsWon;
        MarineRoundsLost = marineRoundsLost;
        MarineDamageDealt = marineDamageDealt;
        MarineKills = marineKills;
        MarineVictoryPoints = marineVictoryPoints;
        MarineImpactPoints = marineImpactPoints;
        MarineRevives = marineRevives;
        MarineHealingDone = marineHealingDone;
        MarineStructuresBuilt = marineStructuresBuilt;
        MarineDeaths = marineDeaths;
        MarineShotsFired = marineShotsFired;
        XenoRoundsPlayed = xenoRoundsPlayed;
        XenoRoundsWon = xenoRoundsWon;
        XenoRoundsLost = xenoRoundsLost;
        XenoDamageDealt = xenoDamageDealt;
        XenoKills = xenoKills;
        XenoVictoryPoints = xenoVictoryPoints;
        XenoImpactPoints = xenoImpactPoints;
        XenoHealingDone = xenoHealingDone;
        XenoStructuresBuilt = xenoStructuresBuilt;
        XenoDeaths = xenoDeaths;
        XenoShotsFired = xenoShotsFired;
    }
}

[Serializable, NetSerializable]
public sealed class CCMRoundMvpData
{
    public string Name { get; }
    public string Ckey { get; }
    public NetEntity? NetEntity { get; }
    public CCMStatsSide Side { get; }
    public int ImpactPoints { get; }
    public int DamageDone { get; }
    public int Kills { get; }
    public int HealingDone { get; }
    public int Revives { get; }
    public int StructuresBuilt { get; }

    public CCMRoundMvpData(
        string name,
        string ckey,
        NetEntity? netEntity,
        CCMStatsSide side,
        int impactPoints,
        int damageDone,
        int kills,
        int healingDone,
        int revives,
        int structuresBuilt)
    {
        Name = name;
        Ckey = ckey;
        NetEntity = netEntity;
        Side = side;
        ImpactPoints = impactPoints;
        DamageDone = damageDone;
        Kills = kills;
        HealingDone = healingDone;
        Revives = revives;
        StructuresBuilt = structuresBuilt;
    }
}

[Serializable, NetSerializable]
public sealed class CCMRoundPersonalStatsData
{
    public int RoundScore { get; }
    public int VictoryPoints { get; }
    public int ImpactPoints { get; }
    public int DamageDone { get; }
    public int Kills { get; }
    public int HealingDone { get; }
    public int Revives { get; }
    public int StructuresBuilt { get; }
    public int RoundSecondsPlayed { get; }
    public bool MarineParticipated { get; }
    public bool XenoParticipated { get; }

    public int MarineVictoryPoints { get; }
    public int MarineImpactPoints { get; }
    public int MarineDamageDone { get; }
    public int MarineKills { get; }
    public int MarineHealingDone { get; }
    public int MarineRevives { get; }
    public int MarineStructuresBuilt { get; }

    public int XenoVictoryPoints { get; }
    public int XenoImpactPoints { get; }
    public int XenoDamageDone { get; }
    public int XenoKills { get; }
    public int XenoHealingDone { get; }
    public int XenoStructuresBuilt { get; }

    public CCMRoundPersonalStatsData(
        int roundScore,
        int victoryPoints,
        int impactPoints,
        int damageDone,
        int kills,
        int healingDone,
        int revives,
        int structuresBuilt,
        int roundSecondsPlayed,
        bool marineParticipated,
        bool xenoParticipated,
        int marineVictoryPoints,
        int marineImpactPoints,
        int marineDamageDone,
        int marineKills,
        int marineHealingDone,
        int marineRevives,
        int marineStructuresBuilt,
        int xenoVictoryPoints,
        int xenoImpactPoints,
        int xenoDamageDone,
        int xenoKills,
        int xenoHealingDone,
        int xenoStructuresBuilt)
    {
        RoundScore = roundScore;
        VictoryPoints = victoryPoints;
        ImpactPoints = impactPoints;
        DamageDone = damageDone;
        Kills = kills;
        HealingDone = healingDone;
        Revives = revives;
        StructuresBuilt = structuresBuilt;
        RoundSecondsPlayed = roundSecondsPlayed;
        MarineParticipated = marineParticipated;
        XenoParticipated = xenoParticipated;
        MarineVictoryPoints = marineVictoryPoints;
        MarineImpactPoints = marineImpactPoints;
        MarineDamageDone = marineDamageDone;
        MarineKills = marineKills;
        MarineHealingDone = marineHealingDone;
        MarineRevives = marineRevives;
        MarineStructuresBuilt = marineStructuresBuilt;
        XenoVictoryPoints = xenoVictoryPoints;
        XenoImpactPoints = xenoImpactPoints;
        XenoDamageDone = xenoDamageDone;
        XenoKills = xenoKills;
        XenoHealingDone = xenoHealingDone;
        XenoStructuresBuilt = xenoStructuresBuilt;
    }
}

[Serializable, NetSerializable]
public sealed class RequestCCMPlayerStatsEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class CCMPlayerStatsResponseEvent : EntityEventArgs
{
    public CCMPlayerStatsSnapshot Stats { get; }

    public CCMPlayerStatsResponseEvent(CCMPlayerStatsSnapshot stats)
    {
        Stats = stats;
    }
}

[Serializable, NetSerializable]
public sealed class RequestCCMLeaderboardEvent : EntityEventArgs
{
    public CCMLeaderboardCategory Category { get; }
    public CCMLeaderboardTimeframe Timeframe { get; }
    public int Page { get; }

    public RequestCCMLeaderboardEvent(CCMLeaderboardCategory category, CCMLeaderboardTimeframe timeframe, int page)
    {
        Category = category;
        Timeframe = timeframe;
        Page = page;
    }
}

[Serializable, NetSerializable]
public sealed class CCMLeaderboardResponseEvent : EntityEventArgs
{
    public CCMLeaderboardPage PageData { get; }

    public CCMLeaderboardResponseEvent(CCMLeaderboardPage pageData)
    {
        PageData = pageData;
    }
}

[Serializable, NetSerializable]
public sealed class CCMRoundEndStatsEvent : EntityEventArgs
{
    public int RoundId { get; }
    public int PersonalScore { get; }
    public int MarineCampaignWins { get; }
    public int XenoCampaignWins { get; }
    public CCMStatsSide WinningSide { get; }
    public CCMRoundPersonalStatsData? PersonalStats { get; }
    public CCMRoundMvpData? MarineMvp { get; }
    public CCMRoundMvpData? XenoMvp { get; }

    public CCMRoundEndStatsEvent(
        int roundId,
        int personalScore,
        int marineCampaignWins,
        int xenoCampaignWins,
        CCMStatsSide winningSide,
        CCMRoundPersonalStatsData? personalStats,
        CCMRoundMvpData? marineMvp,
        CCMRoundMvpData? xenoMvp)
    {
        RoundId = roundId;
        PersonalScore = personalScore;
        MarineCampaignWins = marineCampaignWins;
        XenoCampaignWins = xenoCampaignWins;
        WinningSide = winningSide;
        PersonalStats = personalStats;
        MarineMvp = marineMvp;
        XenoMvp = xenoMvp;
    }
}
