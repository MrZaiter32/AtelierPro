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

    // Módulo Central: Presupuestos y Siniestros
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<ItemPresupuesto> ItemsPresupuesto => Set<ItemPresupuesto>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<Tarifa> Tarifas => Set<Tarifa>();

    // Módulo CRM
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Interaccion> Interacciones => Set<Interaccion>();

    // Módulo de Calidad
    public DbSet<ChecklistControl> ChecklistsControl => Set<ChecklistControl>();
    public DbSet<ReclamoGarantia> ReclamosGarantia => Set<ReclamoGarantia>();

    // Módulo de Almacén e Inventario
    public DbSet<Refaccion> Refacciones => Set<Refaccion>();

    // Módulo de Compras
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();

    // Módulo de Taller
    public DbSet<OrdenReparacion> OrdenesReparacion => Set<OrdenReparacion>();

    // Módulo de Activos
    public DbSet<Activo> Activos => Set<Activo>();
    public DbSet<PlanMantenimiento> PlanesMantenimiento => Set<PlanMantenimiento>();

    // Módulo de Movilidad
    public DbSet<CapturaFotografica> CapturassFotograficas => Set<CapturaFotografica>();
    public DbSet<FirmaDigital> FirmasDigitales => Set<FirmaDigital>();

    // Módulo Financiero
    public DbSet<FacturaCliente> FacturasClientes => Set<FacturaCliente>();
    public DbSet<CuentaPorCobrar> CuentasPorCobrar => Set<CuentaPorCobrar>();
    public DbSet<Transaccion> Transacciones => Set<Transaccion>();

    // Módulo de RR.HH.
    public DbSet<Empleado> Empleados => Set<Empleado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Presupuesto
        modelBuilder.Entity<Presupuesto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.Subtotal); // Propiedad calculada
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

        // Configuración de Refaccion
        modelBuilder.Entity<Refaccion>(entity =>
        {
            entity.HasKey(e => e.Sku);
            entity.Property(e => e.CostoPromedio).HasPrecision(18, 2);
        });

        // Configuración de OrdenCompra
        modelBuilder.Entity<OrdenCompra>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de OrdenReparacion
        modelBuilder.Entity<OrdenReparacion>(entity =>
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
    }
}
