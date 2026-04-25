// CM14 rework: non-RMC edit marker.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Server.Database;

[Table("ccm_player_achievement_stats")]
public sealed class CCMPlayerAchievementStats
{
    [Key]
    [ForeignKey(nameof(Player))]
    [Column("player_id")]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    [Column("friendly_fire_damage")]
    public int FriendlyFireDamage { get; set; }

    [Column("requisition_orders")]
    public int RequisitionOrders { get; set; }

    [Column("xeno_evolutions")]
    public int XenoEvolutions { get; set; }

    [Column("officer_wins")]
    public int OfficerWins { get; set; }

    [Column("queen_kills")]
    public int QueenKills { get; set; }

    [Column("queen_wins")]
    public int QueenWins { get; set; }

    [Column("queen_kill_participations")]
    public int QueenKillParticipations { get; set; }

    [Column("unlocked_achievement_ids")]
    public string UnlockedAchievementIds { get; set; } = string.Empty;
}
