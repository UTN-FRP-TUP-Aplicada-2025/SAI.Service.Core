using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EsquemaEquipos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fabricante",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Identificado = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fabricante", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "Verificacion",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Supuesto = table.Column<string>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    Metodo = table.Column<string>(type: "TEXT", nullable: true),
                    Evidencia = table.Column<string>(type: "TEXT", nullable: true),
                    VigenciaHasta = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ActualizadoEn = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verificacion", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "ModeloBateria",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    FabricanteCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Tecnologia = table.Column<string>(type: "TEXT", nullable: true),
                    CapacidadAh = table.Column<double>(type: "REAL", nullable: true),
                    TensionNominalV = table.Column<double>(type: "REAL", nullable: true),
                    VidaFlotacionAniosMin = table.Column<double>(type: "REAL", nullable: true),
                    VidaFlotacionAniosMax = table.Column<double>(type: "REAL", nullable: true),
                    TemperaturaReferenciaC = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModeloBateria", x => x.Codigo);
                    table.CheckConstraint("CK_ModeloBateria_VidaTemp", "\"VidaFlotacionAniosMin\" IS NULL AND \"VidaFlotacionAniosMax\" IS NULL OR \"TemperaturaReferenciaC\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_ModeloBateria_Fabricante_FabricanteCodigo",
                        column: x => x.FabricanteCodigo,
                        principalTable: "Fabricante",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModeloDispositivo",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    FabricanteCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    LineaTopologia = table.Column<string>(type: "TEXT", nullable: true),
                    TensionNominalV = table.Column<double>(type: "REAL", nullable: true),
                    PotenciaVaNominalValor = table.Column<double>(type: "REAL", nullable: true),
                    PotenciaVaNominalOrigen = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModeloDispositivo", x => x.Codigo);
                    table.ForeignKey(
                        name: "FK_ModeloDispositivo_Fabricante_FabricanteCodigo",
                        column: x => x.FabricanteCodigo,
                        principalTable: "Fabricante",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnidadFisica",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    FechaBaja = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    MotivoBaja = table.Column<string>(type: "TEXT", nullable: true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    ModeloBateriaCodigo = table.Column<string>(type: "TEXT", nullable: true),
                    FechaFabricacion = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    FechaCompra = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ModeloDispositivoCodigo = table.Column<string>(type: "TEXT", nullable: true),
                    NumeroSerie = table.Column<string>(type: "TEXT", nullable: true),
                    Criticidad = table.Column<string>(type: "TEXT", nullable: true),
                    EnServicioDesde = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadFisica", x => x.Codigo);
                    table.CheckConstraint("CK_UnidadFisica_Baja", "(\"Estado\" = 'DadoDeBaja') = (\"FechaBaja\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_UnidadFisica_ModeloBateria_ModeloBateriaCodigo",
                        column: x => x.ModeloBateriaCodigo,
                        principalTable: "ModeloBateria",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnidadFisica_ModeloDispositivo_ModeloDispositivoCodigo",
                        column: x => x.ModeloDispositivoCodigo,
                        principalTable: "ModeloDispositivo",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoberturaHost",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    HostCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Desde = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Hasta = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoberturaHost", x => x.Codigo);
                    table.CheckConstraint("CK_CoberturaHost_Intervalo", "\"Hasta\" IS NULL OR \"Hasta\" >= \"Desde\"");
                    table.ForeignKey(
                        name: "FK_CoberturaHost_UnidadFisica_DispositivoCodigo",
                        column: x => x.DispositivoCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoberturaHost_UnidadFisica_HostCodigo",
                        column: x => x.HostCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MontajeBateria",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    BateriaCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Posicion = table.Column<string>(type: "TEXT", nullable: false),
                    Desde = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Hasta = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MontajeBateria", x => x.Codigo);
                    table.CheckConstraint("CK_MontajeBateria_Intervalo", "\"Hasta\" IS NULL OR \"Hasta\" >= \"Desde\"");
                    table.ForeignKey(
                        name: "FK_MontajeBateria_UnidadFisica_BateriaCodigo",
                        column: x => x.BateriaCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MontajeBateria_UnidadFisica_DispositivoCodigo",
                        column: x => x.DispositivoCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoberturaHost_DispositivoCodigo",
                table: "CoberturaHost",
                column: "DispositivoCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_CoberturaHost_HostCodigo",
                table: "CoberturaHost",
                column: "HostCodigo",
                unique: true,
                filter: "\"Hasta\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ModeloBateria_FabricanteCodigo",
                table: "ModeloBateria",
                column: "FabricanteCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_ModeloDispositivo_FabricanteCodigo",
                table: "ModeloDispositivo",
                column: "FabricanteCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_MontajeBateria_BateriaCodigo",
                table: "MontajeBateria",
                column: "BateriaCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_MontajeBateria_DispositivoCodigo_Posicion",
                table: "MontajeBateria",
                columns: new[] { "DispositivoCodigo", "Posicion" },
                unique: true,
                filter: "\"Hasta\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadFisica_ModeloBateriaCodigo",
                table: "UnidadFisica",
                column: "ModeloBateriaCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadFisica_ModeloDispositivoCodigo",
                table: "UnidadFisica",
                column: "ModeloDispositivoCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadFisica_Tipo_Estado",
                table: "UnidadFisica",
                columns: new[] { "Tipo", "Estado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoberturaHost");

            migrationBuilder.DropTable(
                name: "MontajeBateria");

            migrationBuilder.DropTable(
                name: "Verificacion");

            migrationBuilder.DropTable(
                name: "UnidadFisica");

            migrationBuilder.DropTable(
                name: "ModeloBateria");

            migrationBuilder.DropTable(
                name: "ModeloDispositivo");

            migrationBuilder.DropTable(
                name: "Fabricante");
        }
    }
}
