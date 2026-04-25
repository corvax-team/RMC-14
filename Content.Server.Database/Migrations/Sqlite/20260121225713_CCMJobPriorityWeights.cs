using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class CCMJobPriorityWeights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_job_one_high_priority",
                table: "job");

            migrationBuilder.CreateTable(
                name: "profile_job_priority_weights",
                columns: table => new
                {
                    player_user_id = table.Column<Guid>(nullable: false),
                    slot = table.Column<int>(nullable: false),
                    job_name = table.Column<string>(nullable: false),
                    missed_rounds = table.Column<int>(nullable: false),
                    last_assigned_round_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_profile_job_priority_weights",
                        x => new { x.player_user_id, x.slot, x.job_name });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profile_job_priority_weights");

            migrationBuilder.CreateIndex(
                name: "IX_job_one_high_priority",
                table: "job",
                column: "profile_id",
                unique: true,
                filter: "priority = 3");
        }
    }
}

// # CCM priority rework
