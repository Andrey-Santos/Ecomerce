using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecomerce.Migrations
{
    /// <inheritdoc />
    public partial class AddCupomTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TipoDesconto = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValorMinimoPedido = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LimiteUsos = table.Column<int>(type: "int", nullable: true),
                    UsosAtuais = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cupons", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cupons");
        }
    }
}
