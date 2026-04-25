// CM14 rework: non-RMC edit marker.
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class CCMPlayerCustomization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ccm_player_customization",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    xeno_defender_skin_id = table.Column<string>(type: "TEXT", nullable: false),
                    xeno_drone_skin_id = table.Column<string>(type: "TEXT", nullable: false),
                    xeno_queen_skin_id = table.Column<string>(type: "TEXT", nullable: false),
                    xeno_runner_skin_id = table.Column<string>(type: "TEXT", nullable: false),
                    xeno_sentinel_skin_id = table.Column<string>(type: "TEXT", nullable: false),
                    ghost_skin_id = table.Column<string>(type: "TEXT", nullable: false),
                    weapon_spray_id = table.Column<string>(type: "TEXT", nullable: false),
                    armor_paint_id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ccm_player_customization", x => x.player_id);
                    table.ForeignKey(
                        name: "FK_ccm_player_customization_player_player_id",
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
                name: "ccm_player_customization");
        }
    }
}
