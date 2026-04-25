// CM14 rework: non-RMC edit marker.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class CCMCustomizationOocTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "custom_ooc_tag_text",
                table: "ccm_player_customization",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "selected_ooc_tag_id",
                table: "ccm_player_customization",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "custom_ooc_tag_text",
                table: "ccm_player_customization");

            migrationBuilder.DropColumn(
                name: "selected_ooc_tag_id",
                table: "ccm_player_customization");
        }
    }
}
