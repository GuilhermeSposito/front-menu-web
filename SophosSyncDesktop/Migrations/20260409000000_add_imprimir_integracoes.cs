using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophosSyncDesktop.Migrations
{
    /// <inheritdoc />
    public partial class add_imprimir_integracoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "imprimir_ifood",
                table: "impressoras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "imprimir_sophos_cardapio",
                table: "impressoras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imprimir_ifood",
                table: "impressoras");

            migrationBuilder.DropColumn(
                name: "imprimir_sophos_cardapio",
                table: "impressoras");
        }
    }
}
