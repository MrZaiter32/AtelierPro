using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AtelierPro.Models;

namespace AtelierPro.Data;

/// <summary>
/// Contexto de base de datos para el ERP AtelierPro con soporte para Identity.
/// </summary>
public class AtelierProDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public AtelierProDbContext(DbContextOptions<AtelierProDbContext> options)
        : base(options)
    {
    }

    #region Módulo Central: Presupuestos
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<ItemPresupuesto> ItemsPresupuesto => Set<ItemPresupuesto>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<Tarifa> Tarifas => Set<Tarifa>();
    #endregion

    #region Módulo CRM
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Interaccion> Interacciones => Set<Interaccion>();
    #endregion

    #region Módulo de Calidad
    public DbSet<ChecklistControl> ChecklistsControl => Set<ChecklistControl>();
    public DbSet<ReclamoGarantia> ReclamosGarantia => Set<ReclamoGarantia>();
    #endregion

    #region Módulo de Taller (FASE 1)
    public DbSet<Tecnico> Tecnicos => Set<Tecnico>();
    public DbSet<OrdenReparacion> OrdenesReparacion => Set<OrdenReparacion>();
    public DbSet<ItemOrdenReparacion> ItemsOrdenReparacion => Set<ItemOrdenReparacion>();
    #endregion

    #region Módulo de Almacén e Inventario (FASE 1)
    public DbSet<Refaccion> Refacciones => Set<Refaccion>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<CuentoFisico> CuentosFisicos => Set<CuentoFisico>();
    public DbSet<CuentoFisicoDetalle> DetallesCuentoFisico => Set<CuentoFisicoDetalle>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<ReferenciaAlternativa> ReferenciasAlternativas => Set<ReferenciaAlternativa>();
    #endregion

    #region Módulo de Compras (FASE 1)
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<ItemOrdenCompra> ItemsOrdenCompra => Set<ItemOrdenCompra>();
    public DbSet<Requisicion> Requisiciones => Set<Requisicion>();
    public DbSet<ItemRequisicion> ItemsRequisicion => Set<ItemRequisicion>();
    #endregion

    #region Módulo de Servicios Adicionales (FASE 1)
    public DbSet<OrdenServicio> OrdenesServicio => Set<OrdenServicio>();
    public DbSet<FotoServicio> FotosServicio => Set<FotoServicio>();
    #endregion

    #region Módulo de Activos
    public DbSet<Activo> Activos => Set<Activo>();
    public DbSet<PlanMantenimiento> PlanesMantenimiento => Set<PlanMantenimiento>();
    #endregion

    #region Módulo de Movilidad
    public DbSet<CapturaFotografica> CapturassFotograficas => Set<CapturaFotografica>();
    public DbSet<FirmaDigital> FirmasDigitales => Set<FirmaDigital>();
    #endregion

    #region Módulo Financiero
    public DbSet<FacturaCliente> FacturasClientes => Set<FacturaCliente>();
    public DbSet<CuentaPorCobrar> CuentasPorCobrar => Set<CuentaPorCobrar>();
    public DbSet<Transaccion> Transacciones => Set<Transaccion>();
    #endregion

    #region Módulo de RR.HH.
    public DbSet<Empleado> Empleados => Set<Empleado>();
    #endregion

    #region Model Configuration

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Presupuesto
        modelBuilder.Entity<Presupuesto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.Subtotal);
            entity.Property(e => e.IvaAplicado).HasPrecision(18, 2);
            entity.Property(e => e.TotalFinal).HasPrecision(18, 2);
            entity.HasOne(e => e.Vehiculo)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de ItemPresupuesto
        modelBuilder.Entity<ItemPresupuesto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(e => e.PorcentajeAjuste).HasPrecision(5, 2);
            entity.Ignore(e => e.CostoBase);
            entity.Ignore(e => e.CostoAjustado);
        });

        // Configuración de Vehiculo
        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.Vin);
            entity.Property(e => e.ValorResidual).HasPrecision(18, 2);
        });

        // Configuración de Tarifa
        modelBuilder.Entity<Tarifa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PrecioManoObraHora).HasPrecision(18, 2);
            entity.Property(e => e.PrecioPinturaHora).HasPrecision(18, 2);
            entity.Property(e => e.TasaIva).HasPrecision(5, 4);
            entity.Property(e => e.FactorRecargo).HasPrecision(5, 2);
            entity.Property(e => e.FactorDescuento).HasPrecision(5, 2);
        });

        // Configuración de Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Interacciones)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de Interaccion
        modelBuilder.Entity<Interaccion>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Configuración de Tecnico (FASE 1)
        modelBuilder.Entity<Tecnico>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CostoPorHora).HasPrecision(18, 2);
            entity.Property(e => e.HorasPorSemana).HasPrecision(5, 2);
            entity.Ignore(e => e.NombreCompleto);
            entity.HasMany(e => e.OrdenesAsignadas)
                .WithOne(o => o.TecnicoAsignado)
                .HasForeignKey(o => o.TecnicoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de OrdenReparacion (FASE 1)
        modelBuilder.Entity<OrdenReparacion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HorasEstimadas).HasColumnType("decimal(5,2)");
            entity.Property(e => e.HorasReales).HasColumnType("decimal(5,2)");
            entity.HasMany(e => e.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.MovimientosInventario)
                .WithOne()
                .HasForeignKey(m => m.OrdenReparacionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de ItemOrdenReparacion (FASE 1)
        modelBuilder.Entity<ItemOrdenReparacion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TiempoEstimadoHoras).HasColumnType("decimal(5,2)");
            entity.Property(e => e.TiempoRealHoras).HasColumnType("decimal(5,2)");
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
        });

        // Configuración de Refaccion (FASE 1)
        modelBuilder.Entity<Refaccion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.Property(e => e.CostoPromedio).HasPrecision(18, 2);
            entity.Property(e => e.PrecioVenta).HasPrecision(18, 2);
            entity.HasMany(e => e.Movimientos)
                .WithOne(m => m.Refaccion)
                .HasForeignKey(m => m.RefaccionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.DetallesCuentos)
                .WithOne(d => d.Refaccion)
                .HasForeignKey(d => d.RefaccionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de MovimientoInventario (FASE 1)
        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Refaccion)
                .WithMany(r => r.Movimientos)
                .HasForeignKey(e => e.RefaccionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de CuentoFisico (FASE 1)
        modelBuilder.Entity<CuentoFisico>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Detalles)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de CuentoFisicoDetalle (FASE 1)
        modelBuilder.Entity<CuentoFisicoDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.Diferencia);
            entity.HasOne(e => e.Refaccion)
                .WithMany(r => r.DetallesCuentos)
                .HasForeignKey(e => e.RefaccionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Proveedor (FASE 1)
        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Rfc).IsUnique();
            entity.Property(e => e.CalificacionPromedio).HasPrecision(3, 1);
            entity.HasMany(e => e.OrdenesCompra)
                .WithOne(o => o.Proveedor)
                .HasForeignKey(o => o.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de OrdenCompra (FASE 1)
        modelBuilder.Entity<OrdenCompra>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Numero).IsUnique();
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.Iva).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.HasMany(e => e.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de ItemOrdenCompra (FASE 1)
        modelBuilder.Entity<ItemOrdenCompra>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            entity.Ignore(e => e.Subtotal);
            entity.Ignore(e => e.Completado);
            entity.HasOne(e => e.Refaccion)
                .WithMany()
                .HasForeignKey(e => e.RefaccionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Requisicion (FASE 1)
        modelBuilder.Entity<Requisicion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Numero).IsUnique();
            entity.HasMany(e => e.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de ItemRequisicion (FASE 1)
        modelBuilder.Entity<ItemRequisicion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PrecioEstimado).HasPrecision(18, 2);
        });

        // Configuración de OrdenServicio (FASE 1)
        modelBuilder.Entity<OrdenServicio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Precio).HasPrecision(18, 2);
            entity.HasMany(e => e.Fotos)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de FotoServicio (FASE 1)
        modelBuilder.Entity<FotoServicio>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Configuración de Activo
        modelBuilder.Entity<Activo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.CalibracionVencida);
        });

        // Configuración de PlanMantenimiento
        modelBuilder.Entity<PlanMantenimiento>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Configuración de FacturaCliente
        modelBuilder.Entity<FacturaCliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Importe).HasPrecision(18, 2);
        });

        // Configuración de CuentaPorCobrar
        modelBuilder.Entity<CuentaPorCobrar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Saldo).HasPrecision(18, 2);
            entity.Ignore(e => e.Vencida);
        });

        // Configuración de Transaccion
        modelBuilder.Entity<Transaccion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Monto).HasPrecision(18, 2);
        });

        // Configuración de Empleado
        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SalarioBase).HasPrecision(18, 2);
        });

        // Configuración de ChecklistControl
        modelBuilder.Entity<ChecklistControl>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Configuración de ReclamoGarantia
        modelBuilder.Entity<ReclamoGarantia>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Configuración de CapturaFotografica
        modelBuilder.Entity<CapturaFotografica>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Configuración de FirmaDigital
        modelBuilder.Entity<FirmaDigital>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Configuración de ReferenciaAlternativa (Catálogos)
        modelBuilder.Entity<ReferenciaAlternativa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Refaccion)
                .WithMany()
                .HasForeignKey(e => e.RefaccionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.FabricanteRef, e.PartNumberRef });
        });
    }

    #endregion
}
