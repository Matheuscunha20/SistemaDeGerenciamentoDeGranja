using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGranja.Data.Migrations
{
    /// <inheritdoc />
    // Saída de animais do rebanho (venda, morte...) e compra no estoque ligada à despesa
    public partial class IntegracaoFinanceira : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Todos os animais que já existem continuam no rebanho
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Animais",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Ativo",
                table: "Animais",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataSaida",
                table: "Animais",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MotivoSaida",
                table: "Animais",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorVenda",
                table: "Animais",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MovimentacaoEstoqueId",
                table: "LancamentosFinanceiros",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFinanceiros_MovimentacaoEstoqueId",
                table: "LancamentosFinanceiros",
                column: "MovimentacaoEstoqueId");

            migrationBuilder.AddForeignKey(
                name: "FK_LancamentosFinanceiros_MovimentacoesEstoqueAlimento_MovimentacaoEstoqueId",
                table: "LancamentosFinanceiros",
                column: "MovimentacaoEstoqueId",
                principalTable: "MovimentacoesEstoqueAlimento",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LancamentosFinanceiros_MovimentacoesEstoqueAlimento_MovimentacaoEstoqueId",
                table: "LancamentosFinanceiros");

            migrationBuilder.DropIndex(
                name: "IX_LancamentosFinanceiros_MovimentacaoEstoqueId",
                table: "LancamentosFinanceiros");

            migrationBuilder.DropColumn(name: "MovimentacaoEstoqueId", table: "LancamentosFinanceiros");
            migrationBuilder.DropColumn(name: "ValorVenda", table: "Animais");
            migrationBuilder.DropColumn(name: "MotivoSaida", table: "Animais");
            migrationBuilder.DropColumn(name: "DataSaida", table: "Animais");
            migrationBuilder.DropColumn(name: "Ativo", table: "Animais");
        }
    }
}
