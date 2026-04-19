// CM14 rework: non-RMC edit marker.
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class CCMPlayerStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ccm_player_monthly_stats",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    month = table.Column<int>(type: "INTEGER", nullable: false),
                    rounds_played = table.Column<int>(type: "INTEGER", nullable: false),
                    rounds_won = table.Column<int>(type: "INTEGER", nullable: false),
                    rounds_lost = table.Column<int>(type: "INTEGER", nullable: false),
                    round_seconds_played = table.Column<int>(type: "INTEGER", nullable: false),
                    total_damage_dealt = table.Column<int>(type: "INTEGER", nullable: false),
                    total_kills = table.Column<int>(type: "INTEGER", nullable: false),
                    victory_points = table.Column<int>(type: "INTEGER", nullable: false),
                    impact_points = table.Column<int>(type: "INTEGER", nullable: false),
                    revives = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_rounds_played = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_rounds_won = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_rounds_lost = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_damage_dealt = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_kills = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_victory_points = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_impact_points = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_revives = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_rounds_played = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_rounds_won = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_rounds_lost = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_damage_dealt = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_kills = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_victory_points = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_impact_points = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ccm_player_monthly_stats", x => new { x.player_id, x.year, x.month });
                    table.ForeignKey(
                        name: "FK_ccm_player_monthly_stats_player_player_id1",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ccm_player_stats",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    rounds_played = table.Column<int>(type: "INTEGER", nullable: false),
                    rounds_won = table.Column<int>(type: "INTEGER", nullable: false),
                    rounds_lost = table.Column<int>(type: "INTEGER", nullable: false),
                    round_seconds_played = table.Column<int>(type: "INTEGER", nullable: false),
                    total_damage_dealt = table.Column<int>(type: "INTEGER", nullable: false),
                    total_kills = table.Column<int>(type: "INTEGER", nullable: false),
                    victory_points = table.Column<int>(type: "INTEGER", nullable: false),
                    impact_points = table.Column<int>(type: "INTEGER", nullable: false),
                    revives = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_rounds_played = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_rounds_won = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_rounds_lost = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_damage_dealt = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_kills = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_victory_points = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_impact_points = table.Column<int>(type: "INTEGER", nullable: false),
                    marine_revives = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_rounds_played = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_rounds_won = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_rounds_lost = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_damage_dealt = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_kills = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_victory_points = table.Column<int>(type: "INTEGER", nullable: false),
                    xeno_impact_points = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ccm_player_stats", x => x.player_id);
                    table.ForeignKey(
                        name: "FK_ccm_player_stats_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ccm_player_monthly_stats");

            migrationBuilder.DropTable(
                name: "ccm_player_stats");
        }
    }
}
