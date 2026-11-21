using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGranja.Data.Migrations
{
    /// <inheritdoc />
    // Separa os dados por fazenda (cada cliente do AgroArca é uma fazenda)
    public partial class SeparacaoPorFazenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tabela de fazendas
            migrationBuilder.CreateTable(
                name: "Fazendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fazendas", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // 2. Os dados que já existem vão para a fazenda 1, "Minha fazenda"
            migrationBuilder.Sql("INSERT INTO `Fazendas` (`Id`, `Nome`, `DataCriacao`) VALUES (1, 'Minha fazenda', UTC_TIMESTAMP(6));");

            // 3. Usuários: ligados à fazenda 1
            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE `AspNetUsers` SET `FazendaId` = 1;");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FazendaId",
                table: "AspNetUsers",
                column: "FazendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Fazendas_FazendaId",
                table: "AspNetUsers",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 4. Espécies: o nome passa a ser único dentro de cada fazenda
            migrationBuilder.DropIndex(
                name: "IX_Especies_Nome",
                table: "Especies");

            // 5. Coluna FazendaId em cada tabela. O valor padrão 1 só serve para preencher
            //    os registros existentes e é removido em seguida.

            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "Animais",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "FazendaId",
                table: "Animais",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Animais_FazendaId",
                table: "Animais",
                column: "FazendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Animais_Fazendas_FazendaId",
                table: "Animais",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "Galpoes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "FazendaId",
                table: "Galpoes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Galpoes_FazendaId",
                table: "Galpoes",
                column: "FazendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Galpoes_Fazendas_FazendaId",
                table: "Galpoes",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "Lotes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "FazendaId",
                table: "Lotes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_FazendaId",
                table: "Lotes",
                column: "FazendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lotes_Fazendas_FazendaId",
                table: "Lotes",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "TiposAlimento",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "FazendaId",
                table: "TiposAlimento",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_TiposAlimento_FazendaId",
                table: "TiposAlimento",
                column: "FazendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_TiposAlimento_Fazendas_FazendaId",
                table: "TiposAlimento",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "MovimentacoesEstoqueAlimento",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "FazendaId",
                table: "MovimentacoesEstoqueAlimento",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoqueAlimento_FazendaId",
                table: "MovimentacoesEstoqueAlimento",
                column: "FazendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoqueAlimento_Fazendas_FazendaId",
                table: "MovimentacoesEstoqueAlimento",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "TiposVacina",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "FazendaId",
                table: "TiposVacina",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_TiposVacina_FazendaId",
                table: "TiposVacina",
                column: "FazendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_TiposVacina_Fazendas_FazendaId",
                table: "TiposVacina",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "RegistrosVacinacao",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "FazendaId",
                table: "RegistrosVacinacao",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosVacinacao_FazendaId",
                table: "RegistrosVacinacao",
                column: "FazendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosVacinacao_Fazendas_FazendaId",
                table: "RegistrosVacinacao",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "Funcionarios",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "FazendaId",
                table: "Funcionarios",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_FazendaId",
                table: "Funcionarios",
                column: "FazendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Funcionarios_Fazendas_FazendaId",
                table: "Funcionarios",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "RegistrosSaude",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "FazendaId",
                table: "RegistrosSaude",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSaude_FazendaId",
                table: "RegistrosSaude",
                column: "FazendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosSaude_Fazendas_FazendaId",
                table: "RegistrosSaude",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "Especies",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "FazendaId",
                table: "Especies",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Especies_FazendaId_Nome",
                table: "Especies",
                columns: new[] { "FazendaId", "Nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Especies_Fazendas_FazendaId",
                table: "Especies",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "FazendaId",
                table: "Racas",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "FazendaId",
                table: "Racas",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Racas_FazendaId",
                table: "Racas",
                column: "FazendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Racas_Fazendas_FazendaId",
                table: "Racas",
                column: "FazendaId",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animais_Fazendas_FazendaId",
                table: "Animais");

            migrationBuilder.DropIndex(
                name: "IX_Animais_FazendaId",
                table: "Animais");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "Animais");

            migrationBuilder.DropForeignKey(
                name: "FK_Galpoes_Fazendas_FazendaId",
                table: "Galpoes");

            migrationBuilder.DropIndex(
                name: "IX_Galpoes_FazendaId",
                table: "Galpoes");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "Galpoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Lotes_Fazendas_FazendaId",
                table: "Lotes");

            migrationBuilder.DropIndex(
                name: "IX_Lotes_FazendaId",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "Lotes");

            migrationBuilder.DropForeignKey(
                name: "FK_TiposAlimento_Fazendas_FazendaId",
                table: "TiposAlimento");

            migrationBuilder.DropIndex(
                name: "IX_TiposAlimento_FazendaId",
                table: "TiposAlimento");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "TiposAlimento");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoqueAlimento_Fazendas_FazendaId",
                table: "MovimentacoesEstoqueAlimento");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoqueAlimento_FazendaId",
                table: "MovimentacoesEstoqueAlimento");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "MovimentacoesEstoqueAlimento");

            migrationBuilder.DropForeignKey(
                name: "FK_TiposVacina_Fazendas_FazendaId",
                table: "TiposVacina");

            migrationBuilder.DropIndex(
                name: "IX_TiposVacina_FazendaId",
                table: "TiposVacina");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "TiposVacina");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosVacinacao_Fazendas_FazendaId",
                table: "RegistrosVacinacao");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosVacinacao_FazendaId",
                table: "RegistrosVacinacao");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "RegistrosVacinacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Funcionarios_Fazendas_FazendaId",
                table: "Funcionarios");

            migrationBuilder.DropIndex(
                name: "IX_Funcionarios_FazendaId",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "Funcionarios");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosSaude_Fazendas_FazendaId",
                table: "RegistrosSaude");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosSaude_FazendaId",
                table: "RegistrosSaude");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "RegistrosSaude");

            migrationBuilder.DropForeignKey(
                name: "FK_Especies_Fazendas_FazendaId",
                table: "Especies");

            migrationBuilder.DropIndex(
                name: "IX_Especies_FazendaId_Nome",
                table: "Especies");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "Especies");

            migrationBuilder.DropForeignKey(
                name: "FK_Racas_Fazendas_FazendaId",
                table: "Racas");

            migrationBuilder.DropIndex(
                name: "IX_Racas_FazendaId",
                table: "Racas");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "Racas");

            migrationBuilder.CreateIndex(
                name: "IX_Especies_Nome",
                table: "Especies",
                column: "Nome",
                unique: true);

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Fazendas_FazendaId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_FazendaId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FazendaId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Fazendas");
        }
    }
}
