// CM14 rework: non-RMC edit marker.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class CCMProfileBackgroundFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "corporate_relation_id",
                table: "profile",
                type: "TEXT",
                nullable: false,
                defaultValue: "neutral");

            migrationBuilder.AddColumn<string>(
                name: "origin_id",
                table: "profile",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "religion_id",
                table: "profile",
                type: "TEXT",
                nullable: false,
                defaultValue: "agnostic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "corporate_relation_id",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "origin_id",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "religion_id",
                table: "profile");
        }
    }
}
