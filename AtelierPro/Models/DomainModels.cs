using System;
using System.Collections.Generic;
using System.Linq;

namespace AtelierPro.Models;

public enum EstadoPresupuesto
{
    Borrador,
    Aprobado,
    Cerrado,
    Facturado
}

public enum TipoItemPresupuesto
{
    Pieza,
    ManoObra,
    Pintura
}

public sealed class Vehiculo
{
    public string Vin { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int AntiguedadAnios { get; set; }
    public decimal ValorResidual { get; set; }
}

public sealed class ItemPresupuesto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public TipoItemPresupuesto Tipo { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public double TiempoAsignadoHoras { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PorcentajeAjuste { get; set; }
    public bool RequierePintura { get; set; }
    public bool RequiereDesmontajeDoble { get; set; }
    public bool RequiereAlineacion { get; set; }

    public decimal CostoBase => PrecioUnitario * (decimal)TiempoAsignadoHoras;
    public decimal CostoAjustado => CostoBase * (1 + PorcentajeAjuste / 100m);
}

/// <summary>
/// Representa las tarifas base para cálculos del taller.
/// </summary>
public sealed class Tarifa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>Precio por hora de mano de obra (en moneda local).</summary>
    public decimal PrecioManoObraHora { get; set; }
    
    /// <summary>Precio por hora de pintura (en moneda local).</summary>
    public decimal PrecioPinturaHora { get; set; }
    
    /// <summary>Tasa de IVA en formato decimal (ej: 0.16 = 16%, 0.21 = 21%).</summary>
    public decimal TasaIva { get; set; }
    
    /// <summary>Factor multiplicador de recargo (1.0 = sin recargo, 1.05 = 5% recargo).</summary>
    public decimal FactorRecargo { get; set; } = 1m;
    
    /// <summary>Factor multiplicador de descuento (1.0 = sin descuento, 0.9 = 10% descuento).</summary>
    public decimal FactorDescuento { get; set; } = 1m;
}

public sealed class Presupuesto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Vehiculo? Vehiculo { get; set; }
    public IList<ItemPresupuesto> Items { get; set; } = new List<ItemPresupuesto>();
    public decimal Subtotal => Items.Sum(i => i.CostoAjustado);
    public decimal IvaAplicado { get; set; }
    public decimal TotalFinal { get; set; }
    public EstadoPresupuesto Estado { get; set; } = EstadoPresupuesto.Borrador;
}

public sealed class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Historial { get; set; } = string.Empty;
    public string Preferencias { get; set; } = string.Empty;
    public double Nps { get; set; }
    public double TasaRetencion { get; set; }
    public IList<Interaccion> Interacciones { get; set; } = new List<Interaccion>();
}

public sealed class Interaccion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
}

public sealed class ChecklistControl
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Responsable { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public bool Aprobado { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}

public sealed class ReclamoGarantia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Motivo { get; set; } = string.Empty;
    public string Resolucion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

public sealed class Refaccion
{
    public string Sku { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public decimal CostoPromedio { get; set; }
}

public sealed class OrdenCompra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Proveedor { get; set; } = string.Empty;
    public IList<ItemPresupuesto> Items { get; set; } = new List<ItemPresupuesto>();
    public string Estado { get; set; } = "Generada";
}

public sealed class OrdenReparacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TecnicoAsignado { get; set; } = string.Empty;
    public DateTime Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public double HorasReales { get; set; }
}

public sealed class Activo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime UltimaCalibracion { get; set; }
    public TimeSpan FrecuenciaCalibracion { get; set; }
    public bool CalibracionVencida => DateTime.UtcNow - UltimaCalibracion > FrecuenciaCalibracion;
}

public sealed class PlanMantenimiento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ActivoNombre { get; set; } = string.Empty;
    public string Frecuencia { get; set; } = string.Empty;
    public DateTime ProximaFecha { get; set; }
}

public sealed class CapturaFotografica
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

public sealed class FirmaDigital
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NombreFirmante { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

public sealed class FacturaCliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PresupuestoId { get; set; }
    public decimal Importe { get; set; }
    public DateTime FechaEmision { get; set; }
    public bool Pagada { get; set; }
}

public sealed class CuentaPorCobrar
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FacturaId { get; set; }
    public decimal Saldo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public bool Vencida => DateTime.UtcNow.Date > FechaVencimiento.Date && Saldo > 0;
}

public sealed class Transaccion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Tipo { get; set; } = string.Empty; // Ingreso/Egreso
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public string Concepto { get; set; } = string.Empty;
}

public sealed class Empleado
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public decimal SalarioBase { get; set; }
}

public sealed class DashboardKpi
{
    public decimal MargenPromedio { get; set; }
    public double Nps { get; set; }
    public double TasaRetencion { get; set; }
    public double TasaRetrabajo { get; set; }
    public double CycleTimePromedioDias { get; set; }
    public int OrdenesActivas { get; set; }
    public decimal FlujoCajaProyectado { get; set; }
}
