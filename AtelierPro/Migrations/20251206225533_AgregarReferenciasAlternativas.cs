using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtelierPro.Migrations
{
    public partial class AgregarReferenciasAlternativas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferenciasAlternativas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RefaccionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FabricanteRef = table.Column<string>(type: "TEXT", nullable: false),
                    PartNumberRef = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    ProveedorCatalogo = table.Column<string>(type: "TEXT", nullable: false),
                    UrlCatalogo = table.Column<string>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenciasAlternativas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferenciasAlternativas_Refacciones_RefaccionId",
                        column: x => x.RefaccionId,
                        principalTable: "Refacciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferenciasAlternativas_FabricanteRef_PartNumberRef",
                table: "ReferenciasAlternativas",
                columns: new[] { "FabricanteRef", "PartNumberRef" });

            migrationBuilder.CreateIndex(
                name: "IX_ReferenciasAlternativas_RefaccionId",
                table: "ReferenciasAlternativas",
                column: "RefaccionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferenciasAlternativas");
        }
    }
}
