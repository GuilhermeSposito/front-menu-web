using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SophosSyncDesktop.Migrations
{
    /// <inheritdoc />
    public partial class create_column_de_caminhos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "teste_base64",
                table: "impressoras",
                newName: "caminho_salvamento_nfe");

            migrationBuilder.AlterColumn<string>(
                name: "caminho_salvamento_nfe",
                table: "impressoras",
                type: "TEXT",
                nullable: false,
                defaultValue: @$"C:\ArqNfe\Autorizadas",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "caminho_salvamento_json",
                table: "impressoras",
                type: "TEXT",
                nullable: false,
                defaultValue: "Downloads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "caminho_salvamento_json",
                table: "impressoras");

            migrationBuilder.RenameColumn(
                name: "caminho_salvamento_nfe",
                table: "impressoras",
                newName: "teste_base64");
        }
    }
}
