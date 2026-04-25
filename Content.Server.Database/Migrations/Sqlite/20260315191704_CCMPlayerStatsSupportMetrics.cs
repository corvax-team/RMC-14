// CM14 rework: non-RMC edit marker.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class CCMPlayerStatsSupportMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "healing_done",
                table: "ccm_player_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "marine_healing_done",
                table: "ccm_player_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "marine_structures_built",
                table: "ccm_player_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "structures_built",
                table: "ccm_player_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "xeno_healing_done",
                table: "ccm_player_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "xeno_structures_built",
                table: "ccm_player_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "healing_done",
                table: "ccm_player_monthly_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "marine_healing_done",
                table: "ccm_player_monthly_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "marine_structures_built",
                table: "ccm_player_monthly_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "structures_built",
                table: "ccm_player_monthly_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "xeno_healing_done",
                table: "ccm_player_monthly_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "xeno_structures_built",
                table: "ccm_player_monthly_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "healing_done",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "marine_healing_done",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "marine_structures_built",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "structures_built",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "xeno_healing_done",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "xeno_structures_built",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "healing_done",
                table: "ccm_player_monthly_stats");

            migrationBuilder.DropColumn(
                name: "marine_healing_done",
                table: "ccm_player_monthly_stats");

            migrationBuilder.DropColumn(
                name: "marine_structures_built",
                table: "ccm_player_monthly_stats");

            migrationBuilder.DropColumn(
                name: "structures_built",
                table: "ccm_player_monthly_stats");

            migrationBuilder.DropColumn(
                name: "xeno_healing_done",
                table: "ccm_player_monthly_stats");

            migrationBuilder.DropColumn(
                name: "xeno_structures_built",
                table: "ccm_player_monthly_stats");
        }
    }
}
