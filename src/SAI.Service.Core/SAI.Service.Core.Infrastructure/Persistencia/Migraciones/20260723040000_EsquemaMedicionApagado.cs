using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <summary>
    /// Rediseño UX (H-7): valor numérico de la medición del apagado en <c>Verificacion</c>. La columna
    /// <c>MedicionSegundos</c> (INTEGER, nullable) guarda los segundos medidos de la prueba de tiempo de
    /// apagado, además de la evidencia textual, para mostrarlos comparados contra la ventana reservada.
    /// Nullable: solo aplica al presupuesto de apagado; el resto queda sin valor.
    /// </summary>
    /// <inheritdoc />
    public partial class EsquemaMedicionApagado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MedicionSegundos",
                table: "Verificacion",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicionSegundos",
                table: "Verificacion");
        }
    }
}
