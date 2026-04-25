// CM14 rework: non-RMC edit marker.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class CCMPlayerStatsCombatMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "deaths",
                table: "ccm_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "marine_deaths",
                table: "ccm_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "marine_shots_fired",
                table: "ccm_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "shots_fired",
                table: "ccm_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "xeno_deaths",
                table: "ccm_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "xeno_shots_fired",
                table: "ccm_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "deaths",
                table: "ccm_player_monthly_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "marine_deaths",
                table: "ccm_player_monthly_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "marine_shots_fired",
                table: "ccm_player_monthly_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "shots_fired",
                table: "ccm_player_monthly_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "xeno_deaths",
                table: "ccm_player_monthly_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "xeno_shots_fired",
                table: "ccm_player_monthly_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deaths",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "marine_deaths",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "marine_shots_fired",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "shots_fired",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "xeno_deaths",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "xeno_shots_fired",
                table: "ccm_player_stats");

            migrationBuilder.DropColumn(
                name: "deaths",
                table: "ccm_player_monthly_stats");

            migrationBuilder.DropColumn(
                name: "marine_deaths",
                table: "ccm_player_monthly_stats");

            migrationBuilder.DropColumn(
                name: "marine_shots_fired",
                table: "ccm_player_monthly_stats");

            migrationBuilder.DropColumn(
                name: "shots_fired",
                table: "ccm_player_monthly_stats");

            migrationBuilder.DropColumn(
                name: "xeno_deaths",
                table: "ccm_player_monthly_stats");

            migrationBuilder.DropColumn(
                name: "xeno_shots_fired",
                table: "ccm_player_monthly_stats");
        }
    }
}
