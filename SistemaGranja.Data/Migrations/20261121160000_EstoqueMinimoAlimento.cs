using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGranja.Data.Migrations
{
    /// <inheritdoc />
    public partial class EstoqueMinimoAlimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Estoque mínimo por alimento, usado para avisar quando o estoque está baixo
            migrationBuilder.AddColumn<double>(
                name: "EstoqueMinimo",
                table: "TiposAlimento",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstoqueMinimo",
                table: "TiposAlimento");
        }
    }
}
