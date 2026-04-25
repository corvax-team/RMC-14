// CM14 rework: non-RMC edit marker.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Server.Database;

[Table("ccm_player_customization")]
public sealed class CCMPlayerCustomization
{
    [Key]
    [ForeignKey(nameof(Player))]
    [Column("player_id")]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    [Column("xeno_defender_skin_id")]
    public string XenoDefenderSkinId { get; set; } = string.Empty;

    [Column("xeno_drone_skin_id")]
    public string XenoDroneSkinId { get; set; } = string.Empty;

    [Column("xeno_queen_skin_id")]
    public string XenoQueenSkinId { get; set; } = string.Empty;

    [Column("xeno_runner_skin_id")]
    public string XenoRunnerSkinId { get; set; } = string.Empty;

    [Column("xeno_sentinel_skin_id")]
    public string XenoSentinelSkinId { get; set; } = string.Empty;

    [Column("ghost_skin_id")]
    public string GhostSkinId { get; set; } = string.Empty;

    [Column("weapon_spray_id")]
    public string WeaponSprayId { get; set; } = string.Empty;

    [Column("armor_palette_id")]
    public string ArmorPaletteId { get; set; } = string.Empty;

    [Column("armor_variant_id")]
    public string ArmorVariantId { get; set; } = string.Empty;

    [Column("armor_paint_id")]
    public string ArmorPaintId { get; set; } = string.Empty;

    [Column("selected_ooc_tag_id")]
    public string SelectedOocTagId { get; set; } = string.Empty;

    [Column("custom_ooc_tag_text")]
    public string CustomOocTagText { get; set; } = string.Empty;

    [Column("selected_ooc_color_id")]
    public string SelectedOocColorId { get; set; } = string.Empty;

    [Column("selected_looc_color_id")]
    public string SelectedLoocColorId { get; set; } = string.Empty;
}
