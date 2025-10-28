using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecomerce.Migrations
{
    /// <inheritdoc />
    public partial class AddPrecoTabelaToItens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecoPromocional",
                table: "ItensPedido",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoTabela",
                table: "ItensPedido",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecoPromocional",
                table: "ItensPedido");

            migrationBuilder.DropColumn(
                name: "PrecoTabela",
                table: "ItensPedido");
        }
    }
}
