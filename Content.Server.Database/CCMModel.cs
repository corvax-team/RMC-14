// CM14 rework: non-RMC edit marker.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Server.Database;

[Table("ccm_round_win_stats")]
public sealed class CCMRoundWinStats
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("ccm_round_win_stats_id")]
    public int Id { get; set; }

    public int MarineWins { get; set; }

    public int XenoWins { get; set; }
}
