using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGranja.Data.Migrations
{
    /// <inheritdoc />
    public partial class EspeciesERacas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Especies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Racas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EspecieId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Racas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Racas_Especies_EspecieId",
                        column: x => x.EspecieId,
                        principalTable: "Especies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Especies_Nome",
                table: "Especies",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Racas_EspecieId_Nome",
                table: "Racas",
                columns: new[] { "EspecieId", "Nome" },
                unique: true);

            // A raça deixa de ser texto livre e passa a apontar para a tabela Racas.
            // Animais já cadastrados ficam sem raça e podem ser editados depois.
            migrationBuilder.DropColumn(
                name: "Raca",
                table: "Animais");

            migrationBuilder.AddColumn<int>(
                name: "RacaId",
                table: "Animais",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Animais_RacaId",
                table: "Animais",
                column: "RacaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Animais_Racas_RacaId",
                table: "Animais",
                column: "RacaId",
                principalTable: "Racas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animais_Racas_RacaId",
                table: "Animais");

            migrationBuilder.DropTable(
                name: "Racas");

            migrationBuilder.DropTable(
                name: "Especies");

            migrationBuilder.DropIndex(
                name: "IX_Animais_RacaId",
                table: "Animais");

            migrationBuilder.DropColumn(
                name: "RacaId",
                table: "Animais");

            migrationBuilder.AddColumn<string>(
                name: "Raca",
                table: "Animais",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
