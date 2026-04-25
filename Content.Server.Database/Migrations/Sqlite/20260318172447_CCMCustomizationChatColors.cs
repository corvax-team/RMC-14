// CM14 rework: non-RMC edit marker.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class CCMCustomizationChatColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "selected_looc_color_id",
                table: "ccm_player_customization",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "selected_ooc_color_id",
                table: "ccm_player_customization",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "selected_looc_color_id",
                table: "ccm_player_customization");

            migrationBuilder.DropColumn(
                name: "selected_ooc_color_id",
                table: "ccm_player_customization");
        }
    }
}
