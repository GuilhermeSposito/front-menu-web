using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophosSyncDesktop.Migrations
{
    /// <inheritdoc />
    public partial class inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "impressoras",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    impressora_caixa = table.Column<string>(type: "TEXT", nullable: false),
                    impressora_aux = table.Column<string>(type: "TEXT", nullable: false),
                    impressora_Cz1 = table.Column<string>(type: "TEXT", nullable: false),
                    impressora_Cz2 = table.Column<string>(type: "TEXT", nullable: false),
                    impressora_Cz3 = table.Column<string>(type: "TEXT", nullable: false),
                    impressora_Bar = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_impressoras", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "impressoras");
        }
    }
}
