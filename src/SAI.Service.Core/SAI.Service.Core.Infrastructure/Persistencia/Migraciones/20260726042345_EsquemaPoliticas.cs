using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EsquemaPoliticas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VersionPolitica",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    ModalidadSolicitada = table.Column<string>(type: "TEXT", nullable: false),
                    UmbralDisparoSegundos = table.Column<int>(type: "INTEGER", nullable: false),
                    TiempoReservadoApagadoSeg = table.Column<int>(type: "INTEGER", nullable: false),
                    VigenteDesde = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionPolitica", x => x.Codigo);
                    table.CheckConstraint("CK_VersionPolitica_TechoApagado", "\"TiempoReservadoApagadoSeg\" >= 0 AND \"TiempoReservadoApagadoSeg\" <= 540");
                });

            migrationBuilder.CreateIndex(
                name: "IX_VersionPolitica_Numero",
                table: "VersionPolitica",
                column: "Numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VersionPolitica");
        }
    }
}
