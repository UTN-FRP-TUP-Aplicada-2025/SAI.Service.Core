using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <summary>
    /// P-7: sesión de ejercicio guiado. Tabla mínima —código, inicio, fin y estado— porque la sesión solo
    /// registra la <i>intención</i> de estar haciendo el ejercicio completo y <i>cuándo</i> empezó: el
    /// progreso no se guarda acá, se deriva de las verificaciones (única verdad). Se persiste para que el
    /// ejercicio sobreviva al reinicio del host y se pueda consultar desde cualquier navegador.
    /// </summary>
    /// <inheritdoc />
    public partial class EsquemaSesionEjercicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SesionEjercicio",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    IniciadaEn = table.Column<long>(type: "INTEGER", nullable: false),
                    FinalizadaEn = table.Column<long>(type: "INTEGER", nullable: true),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionEjercicio", x => x.Codigo);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SesionEjercicio");
        }
    }
}
