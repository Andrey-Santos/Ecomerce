using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecomerce.Migrations
{
    /// <inheritdoc />
    public partial class ForcarRestricoesDeExclusao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensPedido_Produtos_ProdutoId",
                table: "ItensPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Variacoes_Produtos_ProdutoId",
                table: "Variacoes");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoUnitario",
                table: "ItensPedido",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "VariacaoId",
                table: "ItensPedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItensPedido_VariacaoId",
                table: "ItensPedido",
                column: "VariacaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItensPedido_Produtos_ProdutoId",
                table: "ItensPedido",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensPedido_Variacoes_VariacaoId",
                table: "ItensPedido",
                column: "VariacaoId",
                principalTable: "Variacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Variacoes_Produtos_ProdutoId",
                table: "Variacoes",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensPedido_Produtos_ProdutoId",
                table: "ItensPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensPedido_Variacoes_VariacaoId",
                table: "ItensPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Variacoes_Produtos_ProdutoId",
                table: "Variacoes");

            migrationBuilder.DropIndex(
                name: "IX_ItensPedido_VariacaoId",
                table: "ItensPedido");

            migrationBuilder.DropColumn(
                name: "VariacaoId",
                table: "ItensPedido");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoUnitario",
                table: "ItensPedido",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_ItensPedido_Produtos_ProdutoId",
                table: "ItensPedido",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Variacoes_Produtos_ProdutoId",
                table: "Variacoes",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
