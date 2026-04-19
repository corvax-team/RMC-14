// CM14 rework: non-RMC edit marker.
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    public partial class CCMRoundWinStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ccm_round_win_stats",
                columns: table => new
                {
                    ccm_round_win_stats_id = table.Column<int>(type: "integer", nullable: false),
                    marine_wins = table.Column<int>(type: "integer", nullable: false),
                    xeno_wins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ccm_round_win_stats", x => x.ccm_round_win_stats_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ccm_round_win_stats");
        }
    }
}
