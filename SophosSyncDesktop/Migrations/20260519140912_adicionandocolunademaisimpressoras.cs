using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophosSyncDesktop.Migrations
{
    /// <inheritdoc />
    public partial class adicionandocolunademaisimpressoras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "impressora_Bar2",
                table: "impressoras",
                type: "TEXT",
                nullable: false,
                defaultValue: "Sem Impressora");

            migrationBuilder.AddColumn<string>(
                name: "impressora_Bar3",
                table: "impressoras",
                type: "TEXT",
                nullable: false,
                defaultValue: "Sem Impressora");

            migrationBuilder.AddColumn<string>(
                name: "impressora_Cz4",
                table: "impressoras",
                type: "TEXT",
                nullable: false,
                defaultValue: "Sem Impressora");

            migrationBuilder.AddColumn<string>(
                name: "impressora_Cz5",
                table: "impressoras",
                type: "TEXT",
                nullable: false,
                defaultValue: "Sem Impressora");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "impressora_Bar2",
                table: "impressoras");

            migrationBuilder.DropColumn(
                name: "impressora_Bar3",
                table: "impressoras");

            migrationBuilder.DropColumn(
                name: "impressora_Cz4",
                table: "impressoras");

            migrationBuilder.DropColumn(
                name: "impressora_Cz5",
                table: "impressoras");
        }
    }
}
