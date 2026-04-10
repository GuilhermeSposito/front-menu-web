using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophosSyncDesktop.Migrations
{
    /// <inheritdoc />
    public partial class add_imprimir_comanda_mesa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "imprimir_comanda_mesa",
                table: "impressoras",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imprimir_comanda_mesa",
                table: "impressoras");
        }
    }
}
