using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EsquemaMonitoreo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agregado",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Variable = table.Column<string>(type: "TEXT", nullable: false),
                    VentanaInicio = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    VentanaDuracion = table.Column<string>(type: "TEXT", nullable: false),
                    NMuestras = table.Column<int>(type: "INTEGER", nullable: false),
                    Cobertura = table.Column<double>(type: "REAL", nullable: false),
                    Advertencia = table.Column<string>(type: "TEXT", nullable: true),
                    Promedio = table.Column<double>(type: "REAL", nullable: true),
                    Minimo = table.Column<double>(type: "REAL", nullable: true),
                    Maximo = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agregado", x => x.Codigo);
                    table.CheckConstraint("CK_Agregado_Cobertura", "\"Cobertura\" >= 0 AND \"Cobertura\" <= 1");
                    table.ForeignKey(
                        name: "FK_Agregado_UnidadFisica_DispositivoCodigo",
                        column: x => x.DispositivoCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FuenteDatos",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    ConfianzaBase = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuenteDatos", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "SesionSondeo",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    FuenteDatosCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Driver = table.Column<string>(type: "TEXT", nullable: false),
                    DriverVersion = table.Column<string>(type: "TEXT", nullable: true),
                    Dialecto = table.Column<string>(type: "TEXT", nullable: true),
                    IntervaloSeg = table.Column<int>(type: "INTEGER", nullable: false),
                    MapaVariableOrigen = table.Column<string>(type: "TEXT", nullable: false),
                    Desde = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Hasta = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionSondeo", x => x.Codigo);
                    table.ForeignKey(
                        name: "FK_SesionSondeo_FuenteDatos_FuenteDatosCodigo",
                        column: x => x.FuenteDatosCodigo,
                        principalTable: "FuenteDatos",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SesionSondeo_UnidadFisica_DispositivoCodigo",
                        column: x => x.DispositivoCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Muestra",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    SesionSondeoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Instante = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Calidad = table.Column<string>(type: "TEXT", nullable: false),
                    Valores = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Muestra", x => x.Codigo);
                    table.CheckConstraint("CK_Muestra_Calidad", "\"Calidad\" IN ('Completa','Parcial','Perdida')");
                    table.ForeignKey(
                        name: "FK_Muestra_SesionSondeo_SesionSondeoCodigo",
                        column: x => x.SesionSondeoCodigo,
                        principalTable: "SesionSondeo",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Muestra_UnidadFisica_DispositivoCodigo",
                        column: x => x.DispositivoCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agregado_DispositivoCodigo_Variable_VentanaInicio",
                table: "Agregado",
                columns: new[] { "DispositivoCodigo", "Variable", "VentanaInicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Muestra_DispositivoCodigo_Instante",
                table: "Muestra",
                columns: new[] { "DispositivoCodigo", "Instante" });

            migrationBuilder.CreateIndex(
                name: "IX_Muestra_SesionSondeoCodigo",
                table: "Muestra",
                column: "SesionSondeoCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_SesionSondeo_DispositivoCodigo",
                table: "SesionSondeo",
                column: "DispositivoCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_SesionSondeo_FuenteDatosCodigo",
                table: "SesionSondeo",
                column: "FuenteDatosCodigo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agregado");

            migrationBuilder.DropTable(
                name: "Muestra");

            migrationBuilder.DropTable(
                name: "SesionSondeo");

            migrationBuilder.DropTable(
                name: "FuenteDatos");
        }
    }
}
