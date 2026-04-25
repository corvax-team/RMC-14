// CM14 rework: non-RMC edit marker.
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    public partial class CCMAchievements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ccm_player_achievement_stats",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    friendly_fire_damage = table.Column<int>(type: "integer", nullable: false),
                    requisition_orders = table.Column<int>(type: "integer", nullable: false),
                    xeno_evolutions = table.Column<int>(type: "integer", nullable: false),
                    officer_wins = table.Column<int>(type: "integer", nullable: false),
                    queen_kills = table.Column<int>(type: "integer", nullable: false),
                    queen_wins = table.Column<int>(type: "integer", nullable: false),
                    queen_kill_participations = table.Column<int>(type: "integer", nullable: false),
                    unlocked_achievement_ids = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ccm_player_achievement_stats", x => x.player_id);
                    table.ForeignKey(
                        name: "FK_ccm_player_achievement_stats_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ccm_player_achievement_stats");
        }
    }
}
