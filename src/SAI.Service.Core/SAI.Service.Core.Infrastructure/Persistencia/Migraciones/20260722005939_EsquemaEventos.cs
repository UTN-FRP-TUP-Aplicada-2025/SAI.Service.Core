using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EsquemaEventos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReglaDerivacion",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    Parametros = table.Column<string>(type: "TEXT", nullable: false),
                    VigenteDesde = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglaDerivacion", x => new { x.Codigo, x.Version });
                });

            migrationBuilder.CreateTable(
                name: "Evento",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Instante = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DuracionSeg = table.Column<double>(type: "REAL", nullable: true),
                    IncertidumbreDuracionSeg = table.Column<double>(type: "REAL", nullable: true),
                    ReglaDerivacionCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    ReglaVersion = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evento", x => x.Codigo);
                    table.ForeignKey(
                        name: "FK_Evento_ReglaDerivacion_ReglaDerivacionCodigo_ReglaVersion",
                        columns: x => new { x.ReglaDerivacionCodigo, x.ReglaVersion },
                        principalTable: "ReglaDerivacion",
                        principalColumns: new[] { "Codigo", "Version" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evento_UnidadFisica_DispositivoCodigo",
                        column: x => x.DispositivoCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Evento_DispositivoCodigo_Instante",
                table: "Evento",
                columns: new[] { "DispositivoCodigo", "Instante" });

            migrationBuilder.CreateIndex(
                name: "IX_Evento_ReglaDerivacionCodigo_ReglaVersion",
                table: "Evento",
                columns: new[] { "ReglaDerivacionCodigo", "ReglaVersion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Evento");

            migrationBuilder.DropTable(
                name: "ReglaDerivacion");
        }
    }
}
