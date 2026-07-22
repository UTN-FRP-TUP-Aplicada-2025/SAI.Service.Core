using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EsquemaAcciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accion",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    DispositivoCodigo = table.Column<string>(type: "TEXT", nullable: false),
                    Instante = table.Column<long>(type: "INTEGER", nullable: false),
                    ModalidadSolicitada = table.Column<string>(type: "TEXT", nullable: false),
                    ModalidadEfectiva = table.Column<string>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    TiempoReservadoSeg = table.Column<int>(type: "INTEGER", nullable: false),
                    Detalle = table.Column<string>(type: "TEXT", nullable: false),
                    EventoDisparoCodigo = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accion", x => x.Codigo);
                    table.ForeignKey(
                        name: "FK_Accion_UnidadFisica_DispositivoCodigo",
                        column: x => x.DispositivoCodigo,
                        principalTable: "UnidadFisica",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accion_DispositivoCodigo_Instante",
                table: "Accion",
                columns: new[] { "DispositivoCodigo", "Instante" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accion");
        }
    }
}
