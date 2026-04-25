// CM14 rework: non-RMC edit marker.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Server.Database;

[Table("ccm_player_stats")]
public sealed class CCMPlayerStats
{
    [Key]
    [ForeignKey(nameof(Player))]
    [Column("player_id")]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    public int RoundsPlayed { get; set; }
    public int RoundsWon { get; set; }
    public int RoundsLost { get; set; }
    public int RoundSecondsPlayed { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalKills { get; set; }
    public int VictoryPoints { get; set; }
    public int ImpactPoints { get; set; }
    public int Revives { get; set; }
    public int HealingDone { get; set; }
    public int StructuresBuilt { get; set; }
    public int Deaths { get; set; }
    public int ShotsFired { get; set; }

    public int MarineRoundsPlayed { get; set; }
    public int MarineRoundsWon { get; set; }
    public int MarineRoundsLost { get; set; }
    public int MarineDamageDealt { get; set; }
    public int MarineKills { get; set; }
    public int MarineVictoryPoints { get; set; }
    public int MarineImpactPoints { get; set; }
    public int MarineRevives { get; set; }
    public int MarineHealingDone { get; set; }
    public int MarineStructuresBuilt { get; set; }
    public int MarineDeaths { get; set; }
    public int MarineShotsFired { get; set; }

    public int XenoRoundsPlayed { get; set; }
    public int XenoRoundsWon { get; set; }
    public int XenoRoundsLost { get; set; }
    public int XenoDamageDealt { get; set; }
    public int XenoKills { get; set; }
    public int XenoVictoryPoints { get; set; }
    public int XenoImpactPoints { get; set; }
    public int XenoHealingDone { get; set; }
    public int XenoStructuresBuilt { get; set; }
    public int XenoDeaths { get; set; }
    public int XenoShotsFired { get; set; }
}

[Table("ccm_player_monthly_stats")]
public sealed class CCMPlayerMonthlyStats
{
    [Column("player_id")]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    [Column("year")]
    public int Year { get; set; }

    [Column("month")]
    public int Month { get; set; }

    public int RoundsPlayed { get; set; }
    public int RoundsWon { get; set; }
    public int RoundsLost { get; set; }
    public int RoundSecondsPlayed { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalKills { get; set; }
    public int VictoryPoints { get; set; }
    public int ImpactPoints { get; set; }
    public int Revives { get; set; }
    public int HealingDone { get; set; }
    public int StructuresBuilt { get; set; }
    public int Deaths { get; set; }
    public int ShotsFired { get; set; }

    public int MarineRoundsPlayed { get; set; }
    public int MarineRoundsWon { get; set; }
    public int MarineRoundsLost { get; set; }
    public int MarineDamageDealt { get; set; }
    public int MarineKills { get; set; }
    public int MarineVictoryPoints { get; set; }
    public int MarineImpactPoints { get; set; }
    public int MarineRevives { get; set; }
    public int MarineHealingDone { get; set; }
    public int MarineStructuresBuilt { get; set; }
    public int MarineDeaths { get; set; }
    public int MarineShotsFired { get; set; }

    public int XenoRoundsPlayed { get; set; }
    public int XenoRoundsWon { get; set; }
    public int XenoRoundsLost { get; set; }
    public int XenoDamageDealt { get; set; }
    public int XenoKills { get; set; }
    public int XenoVictoryPoints { get; set; }
    public int XenoImpactPoints { get; set; }
    public int XenoHealingDone { get; set; }
    public int XenoStructuresBuilt { get; set; }
    public int XenoDeaths { get; set; }
    public int XenoShotsFired { get; set; }
}
