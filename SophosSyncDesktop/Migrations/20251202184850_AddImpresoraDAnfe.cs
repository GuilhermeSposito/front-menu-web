using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophosSyncDesktop.Migrations
{
    /// <inheritdoc />
    public partial class AddImpresoraDAnfe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "impressora_Danfe",
                table: "impressoras",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "impressora_Danfe",
                table: "impressoras");
        }
    }
}
