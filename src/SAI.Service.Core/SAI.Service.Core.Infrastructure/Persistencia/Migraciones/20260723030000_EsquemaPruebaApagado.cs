using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <summary>
    /// Etapa 4·E: marca de prueba física en curso sobre <c>Verificacion</c>. La columna
    /// <c>PruebaEnCursoDesde</c> (instante en UTC, INTEGER como el resto de las fechas) indica que se
    /// disparó una prueba que exige un reinicio del host antes de repetirse; se limpia al rearmar tras el
    /// reinicio. Nullable: el default (sin prueba en curso) no ocupa nada en las filas existentes.
    /// </summary>
    /// <inheritdoc />
    public partial class EsquemaPruebaApagado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PruebaEnCursoDesde",
                table: "Verificacion",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PruebaEnCursoDesde",
                table: "Verificacion");
        }
    }
}
