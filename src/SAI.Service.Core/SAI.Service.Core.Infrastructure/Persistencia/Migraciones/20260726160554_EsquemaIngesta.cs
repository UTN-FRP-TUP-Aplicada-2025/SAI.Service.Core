using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EsquemaIngesta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntervencionIngerida",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    ClaveIdempotencia = table.Column<string>(type: "TEXT", nullable: false),
                    HuellaCuerpo = table.Column<string>(type: "TEXT", nullable: false),
                    FuenteDatosCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Confianza = table.Column<string>(type: "TEXT", nullable: false),
                    TipoIntervencion = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Baterias = table.Column<string>(type: "TEXT", nullable: false),
                    Proveedor = table.Column<string>(type: "TEXT", nullable: true),
                    Hallazgos = table.Column<string>(type: "TEXT", nullable: true),
                    DisposicionDestino = table.Column<string>(type: "TEXT", nullable: true),
                    DisposicionReceptor = table.Column<string>(type: "TEXT", nullable: true),
                    TiempoValido = table.Column<long>(type: "INTEGER", nullable: false),
                    TiempoRegistrado = table.Column<long>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_IntervencionIngerida", x => x.Codigo);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntervencionIngerida_ClaveIdempotencia",
                table: "IntervencionIngerida",
                column: "ClaveIdempotencia",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntervencionIngerida");
        }
    }
}
