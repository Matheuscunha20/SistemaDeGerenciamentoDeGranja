using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGranja.Data.Migrations
{
    /// <inheritdoc />
    // Financeiro: contas a pagar e a receber (situação "pago" e data do pagamento)
    public partial class ContasAPagarReceber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Os lançamentos que já existem foram pagos/recebidos na própria data
            migrationBuilder.AddColumn<bool>(
                name: "Pago",
                table: "LancamentosFinanceiros",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Pago",
                table: "LancamentosFinanceiros",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPagamento",
                table: "LancamentosFinanceiros",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.Sql("UPDATE `LancamentosFinanceiros` SET `DataPagamento` = `Data`;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DataPagamento", table: "LancamentosFinanceiros");
            migrationBuilder.DropColumn(name: "Pago", table: "LancamentosFinanceiros");
        }
    }
}
