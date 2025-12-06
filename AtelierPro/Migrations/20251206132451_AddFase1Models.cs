using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtelierPro.Migrations
{
    public partial class AddFase1Models : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsPresupuesto_OrdenesCompra_OrdenCompraId",
                table: "ItemsPresupuesto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Refacciones",
                table: "Refacciones");

            migrationBuilder.DropIndex(
                name: "IX_ItemsPresupuesto_OrdenCompraId",
                table: "ItemsPresupuesto");

            migrationBuilder.DropColumn(
                name: "OrdenCompraId",
                table: "ItemsPresupuesto");

            migrationBuilder.RenameColumn(
                name: "TecnicoAsignado",
                table: "OrdenesReparacion",
                newName: "Prioridad");

            migrationBuilder.RenameColumn(
                name: "Inicio",
                table: "OrdenesReparacion",
                newName: "PresupuestoId");

            migrationBuilder.RenameColumn(
                name: "Fin",
                table: "OrdenesReparacion",
                newName: "TecnicoId");

            migrationBuilder.RenameColumn(
                name: "Proveedor",
                table: "OrdenesCompra",
                newName: "ResponsableUsuarioId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Refacciones",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "Activa",
                table: "Refacciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Refacciones",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Refacciones",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Refacciones",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Refacciones",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioVenta",
                table: "Refacciones",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "StockMaximo",
                table: "Refacciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion",
                table: "Refacciones",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "HorasReales",
                table: "OrdenesReparacion",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "OrdenesReparacion",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "OrdenesReparacion",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFinEstimada",
                table: "OrdenesReparacion",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFinReal",
                table: "OrdenesReparacion",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicio",
                table: "OrdenesReparacion",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasEstimadas",
                table: "OrdenesReparacion",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "OrdenesReparacion",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "OrdenesCompra",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "OrdenesCompra",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnvio",
                table: "OrdenesCompra",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRecepcion",
                table: "OrdenesCompra",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Iva",
                table: "OrdenesCompra",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "OrdenesCompra",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "OrdenesCompra",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ProveedorId",
                table: "OrdenesCompra",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Referencia",
                table: "OrdenesCompra",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "OrdenesCompra",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "OrdenesCompra",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Refacciones",
                table: "Refacciones",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CuentosFisicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaCompletacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: false),
                    ResponsableUsuarioId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentosFisicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemsOrdenCompra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrdenCompraId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RefaccionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CantidadRecibida = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsOrdenCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsOrdenCompra_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "OrdenesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemsOrdenCompra_Refacciones_RefaccionId",
                        column: x => x.RefaccionId,
                        principalTable: "Refacciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemsOrdenReparacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrdenReparacionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemPresupuestoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    TiempoEstimadoHoras = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TiempoRealHoras = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Completado = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsOrdenReparacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsOrdenReparacion_OrdenesReparacion_OrdenReparacionId",
                        column: x => x.OrdenReparacionId,
                        principalTable: "OrdenesReparacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RefaccionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Razon = table.Column<string>(type: "TEXT", nullable: false),
                    ResponsableUsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    OrdenCompraId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OrdenReparacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StockAnterior = table.Column<int>(type: "INTEGER", nullable: false),
                    StockPosterior = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_OrdenesReparacion_OrdenReparacionId",
                        column: x => x.OrdenReparacionId,
                        principalTable: "OrdenesReparacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Refacciones_RefaccionId",
                        column: x => x.RefaccionId,
                        principalTable: "Refacciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesServicio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrdenReparacionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Precio = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ResponsableUsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesServicio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RazonSocial = table.Column<string>(type: "TEXT", nullable: false),
                    Rfc = table.Column<string>(type: "TEXT", nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", nullable: false),
                    ContactoPrincipal = table.Column<string>(type: "TEXT", nullable: false),
                    CondicionesPago = table.Column<string>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CalificacionPromedio = table.Column<decimal>(type: "TEXT", precision: 3, scale: 1, nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Requisiciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    SolicitanteUsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    AprobadorUsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    Justificacion = table.Column<string>(type: "TEXT", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requisiciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tecnicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Apellido = table.Column<string>(type: "TEXT", nullable: false),
                    Especialidad = table.Column<string>(type: "TEXT", nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HorasPorSemana = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    CostoPorHora = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tecnicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetallesCuentoFisico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CuentoFisicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RefaccionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StockSistema = table.Column<int>(type: "INTEGER", nullable: false),
                    StockFisico = table.Column<int>(type: "INTEGER", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesCuentoFisico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesCuentoFisico_CuentosFisicos_CuentoFisicoId",
                        column: x => x.CuentoFisicoId,
                        principalTable: "CuentosFisicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesCuentoFisico_Refacciones_RefaccionId",
                        column: x => x.RefaccionId,
                        principalTable: "Refacciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FotosServicio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrdenServicioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Etiqueta = table.Column<string>(type: "TEXT", nullable: false),
                    FechaCarga = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FotosServicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FotosServicio_OrdenesServicio_OrdenServicioId",
                        column: x => x.OrdenServicioId,
                        principalTable: "OrdenesServicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemsRequisicion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequisicionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sku = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioEstimado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsRequisicion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsRequisicion_Requisiciones_RequisicionId",
                        column: x => x.RequisicionId,
                        principalTable: "Requisiciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Refacciones_Sku",
                table: "Refacciones",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesReparacion_TecnicoId",
                table: "OrdenesReparacion",
                column: "TecnicoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_Numero",
                table: "OrdenesCompra",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_ProveedorId",
                table: "OrdenesCompra",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCuentoFisico_CuentoFisicoId",
                table: "DetallesCuentoFisico",
                column: "CuentoFisicoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCuentoFisico_RefaccionId",
                table: "DetallesCuentoFisico",
                column: "RefaccionId");

            migrationBuilder.CreateIndex(
                name: "IX_FotosServicio_OrdenServicioId",
                table: "FotosServicio",
                column: "OrdenServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsOrdenCompra_OrdenCompraId",
                table: "ItemsOrdenCompra",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsOrdenCompra_RefaccionId",
                table: "ItemsOrdenCompra",
                column: "RefaccionId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsOrdenReparacion_OrdenReparacionId",
                table: "ItemsOrdenReparacion",
                column: "OrdenReparacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsRequisicion_RequisicionId",
                table: "ItemsRequisicion",
                column: "RequisicionId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_OrdenReparacionId",
                table: "MovimientosInventario",
                column: "OrdenReparacionId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_RefaccionId",
                table: "MovimientosInventario",
                column: "RefaccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_Rfc",
                table: "Proveedores",
                column: "Rfc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requisiciones_Numero",
                table: "Requisiciones",
                column: "Numero",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompra_Proveedores_ProveedorId",
                table: "OrdenesCompra",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesReparacion_Tecnicos_TecnicoId",
                table: "OrdenesReparacion",
                column: "TecnicoId",
                principalTable: "Tecnicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompra_Proveedores_ProveedorId",
                table: "OrdenesCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesReparacion_Tecnicos_TecnicoId",
                table: "OrdenesReparacion");

            migrationBuilder.DropTable(
                name: "DetallesCuentoFisico");

            migrationBuilder.DropTable(
                name: "FotosServicio");

            migrationBuilder.DropTable(
                name: "ItemsOrdenCompra");

            migrationBuilder.DropTable(
                name: "ItemsOrdenReparacion");

            migrationBuilder.DropTable(
                name: "ItemsRequisicion");

            migrationBuilder.DropTable(
                name: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Tecnicos");

            migrationBuilder.DropTable(
                name: "CuentosFisicos");

            migrationBuilder.DropTable(
                name: "OrdenesServicio");

            migrationBuilder.DropTable(
                name: "Requisiciones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Refacciones",
                table: "Refacciones");

            migrationBuilder.DropIndex(
                name: "IX_Refacciones_Sku",
                table: "Refacciones");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesReparacion_TecnicoId",
                table: "OrdenesReparacion");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesCompra_Numero",
                table: "OrdenesCompra");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesCompra_ProveedorId",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Refacciones");

            migrationBuilder.DropColumn(
                name: "Activa",
                table: "Refacciones");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Refacciones");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Refacciones");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Refacciones");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Refacciones");

            migrationBuilder.DropColumn(
                name: "PrecioVenta",
                table: "Refacciones");

            migrationBuilder.DropColumn(
                name: "StockMaximo",
                table: "Refacciones");

            migrationBuilder.DropColumn(
                name: "Ubicacion",
                table: "Refacciones");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "OrdenesReparacion");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "OrdenesReparacion");

            migrationBuilder.DropColumn(
                name: "FechaFinEstimada",
                table: "OrdenesReparacion");

            migrationBuilder.DropColumn(
                name: "FechaFinReal",
                table: "OrdenesReparacion");

            migrationBuilder.DropColumn(
                name: "FechaInicio",
                table: "OrdenesReparacion");

            migrationBuilder.DropColumn(
                name: "HorasEstimadas",
                table: "OrdenesReparacion");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "OrdenesReparacion");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "FechaEnvio",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "FechaRecepcion",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "Iva",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "ProveedorId",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "Referencia",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "OrdenesCompra");

            migrationBuilder.RenameColumn(
                name: "TecnicoId",
                table: "OrdenesReparacion",
                newName: "Fin");

            migrationBuilder.RenameColumn(
                name: "Prioridad",
                table: "OrdenesReparacion",
                newName: "TecnicoAsignado");

            migrationBuilder.RenameColumn(
                name: "PresupuestoId",
                table: "OrdenesReparacion",
                newName: "Inicio");

            migrationBuilder.RenameColumn(
                name: "ResponsableUsuarioId",
                table: "OrdenesCompra",
                newName: "Proveedor");

            migrationBuilder.AlterColumn<double>(
                name: "HorasReales",
                table: "OrdenesReparacion",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "OrdenesCompra",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<Guid>(
                name: "OrdenCompraId",
                table: "ItemsPresupuesto",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Refacciones",
                table: "Refacciones",
                column: "Sku");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPresupuesto_OrdenCompraId",
                table: "ItemsPresupuesto",
                column: "OrdenCompraId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsPresupuesto_OrdenesCompra_OrdenCompraId",
                table: "ItemsPresupuesto",
                column: "OrdenCompraId",
                principalTable: "OrdenesCompra",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
