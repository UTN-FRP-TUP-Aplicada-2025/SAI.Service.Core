using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EsquemaSustituciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SustitucionSai",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    HostCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoSalienteCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoEntranteCodigo = table.Column<string>(type: "TEXT", nullable: true),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    InstanteOcurrido = table.Column<long>(type: "INTEGER", nullable: false),
                    InstanteRegistrado = table.Column<long>(type: "INTEGER", nullable: false),
                    Proveedor = table.Column<string>(type: "TEXT", nullable: false),
                    Ejecutor = table.Column<string>(type: "TEXT", nullable: false),
                    Hallazgos = table.Column<string>(type: "TEXT", nullable: false),
                    FirmwareReiniciado = table.Column<bool>(type: "INTEGER", nullable: false),
                    CostoMonto = table.Column<decimal>(type: "TEXT", nullable: true),
                    CostoMoneda = table.Column<string>(type: "TEXT", nullable: true),
                    CostoFecha = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    DisposicionDestino = table.Column<string>(type: "TEXT", nullable: true),
                    DisposicionReceptor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SustitucionSai", x => x.Codigo);
                    table.ForeignKey(
                        name: "FK_SustitucionSai_UnidadFisica_DispositivoSalienteCodigo",
                        column: x => x.DispositivoSalienteCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SustitucionSai_DispositivoSalienteCodigo",
                table: "SustitucionSai",
                column: "DispositivoSalienteCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_SustitucionSai_HostCodigo_InstanteOcurrido",
                table: "SustitucionSai",
                columns: new[] { "HostCodigo", "InstanteOcurrido" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SustitucionSai");
        }
    }
}
