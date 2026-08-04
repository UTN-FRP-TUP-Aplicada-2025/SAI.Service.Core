using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PoliticaTiempoRetorno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Las políticas ya existentes toman el retorno que antes estaba fijo en el adaptador (180 s),
            // preservando el comportamiento previo. El default solo llena las filas históricas; las
            // versiones nuevas fijan su propio valor.
            migrationBuilder.AddColumn<int>(
                name: "TiempoRetornoSeg",
                table: "VersionPolitica",
                type: "INTEGER",
                nullable: false,
                defaultValue: 180);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TiempoRetornoSeg",
                table: "VersionPolitica");
        }
    }
}
