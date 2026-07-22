using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EsquemaSaludBateria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PruebaBateria",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    MontajeBateriaCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Instante = table.Column<long>(type: "INTEGER", nullable: false),
                    CargaPorcentaje = table.Column<int>(type: "INTEGER", nullable: true),
                    Comparable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Veredicto = table.Column<string>(type: "TEXT", nullable: true),
                    Confianza = table.Column<string>(type: "TEXT", nullable: true),
                    CaidaTensionValor = table.Column<double>(type: "REAL", nullable: true),
                    CaidaTensionOrigen = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PruebaBateria", x => x.Codigo);
                    table.ForeignKey(
                        name: "FK_PruebaBateria_MontajeBateria_MontajeBateriaCodigo",
                        column: x => x.MontajeBateriaCodigo,
                        principalTable: "MontajeBateria",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PruebaBateria_UnidadFisica_DispositivoCodigo",
                        column: x => x.DispositivoCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PruebaBateria_DispositivoCodigo_Instante",
                table: "PruebaBateria",
                columns: new[] { "DispositivoCodigo", "Instante" });

            migrationBuilder.CreateIndex(
                name: "IX_PruebaBateria_MontajeBateriaCodigo",
                table: "PruebaBateria",
                column: "MontajeBateriaCodigo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PruebaBateria");
        }
    }
}
