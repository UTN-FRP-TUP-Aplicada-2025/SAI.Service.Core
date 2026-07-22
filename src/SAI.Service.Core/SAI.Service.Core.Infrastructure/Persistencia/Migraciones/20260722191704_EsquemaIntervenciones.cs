using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EsquemaIntervenciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Intervencion",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Posicion = table.Column<string>(type: "TEXT", nullable: false),
                    BateriaSalienteCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    BateriaEntranteCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    InstanteOcurrido = table.Column<long>(type: "INTEGER", nullable: false),
                    InstanteRegistrado = table.Column<long>(type: "INTEGER", nullable: false),
                    Proveedor = table.Column<string>(type: "TEXT", nullable: false),
                    Ejecutor = table.Column<string>(type: "TEXT", nullable: false),
                    Hallazgos = table.Column<string>(type: "TEXT", nullable: false),
                    DisposicionDestino = table.Column<string>(type: "TEXT", nullable: false),
                    DisposicionReceptor = table.Column<string>(type: "TEXT", nullable: false),
                    ManoObraFecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ManoObraMoneda = table.Column<string>(type: "TEXT", nullable: false),
                    ManoObraMonto = table.Column<decimal>(type: "TEXT", nullable: false),
                    RepuestosFecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    RepuestosMoneda = table.Column<string>(type: "TEXT", nullable: false),
                    RepuestosMonto = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalFecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    TotalMoneda = table.Column<string>(type: "TEXT", nullable: false),
                    TotalMonto = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intervencion", x => x.Codigo);
                    table.ForeignKey(
                        name: "FK_Intervencion_UnidadFisica_DispositivoCodigo",
                        column: x => x.DispositivoCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FichaVidaUtil",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    IntervencionCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    BateriaCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    DiasEnServicio = table.Column<int>(type: "INTEGER", nullable: false),
                    VidaEsperadaDias = table.Column<int>(type: "INTEGER", nullable: false),
                    CumplioExpectativa = table.Column<bool>(type: "INTEGER", nullable: false),
                    DesvioDias = table.Column<int>(type: "INTEGER", nullable: false),
                    FuenteCotizacion = table.Column<string>(type: "TEXT", nullable: false),
                    CostoAnioFecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CostoAnioMoneda = table.Column<string>(type: "TEXT", nullable: false),
                    CostoAnioMonto = table.Column<decimal>(type: "TEXT", nullable: false),
                    CostoAnioUsdFecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CostoAnioUsdMoneda = table.Column<string>(type: "TEXT", nullable: false),
                    CostoAnioUsdMonto = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaVidaUtil", x => x.Codigo);
                    table.ForeignKey(
                        name: "FK_FichaVidaUtil_Intervencion_IntervencionCodigo",
                        column: x => x.IntervencionCodigo,
                        principalTable: "Intervencion",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FichaVidaUtil_DispositivoCodigo_IntervencionCodigo",
                table: "FichaVidaUtil",
                columns: new[] { "DispositivoCodigo", "IntervencionCodigo" });

            migrationBuilder.CreateIndex(
                name: "IX_FichaVidaUtil_IntervencionCodigo",
                table: "FichaVidaUtil",
                column: "IntervencionCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_Intervencion_DispositivoCodigo_InstanteOcurrido",
                table: "Intervencion",
                columns: new[] { "DispositivoCodigo", "InstanteOcurrido" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FichaVidaUtil");

            migrationBuilder.DropTable(
                name: "Intervencion");
        }
    }
}
