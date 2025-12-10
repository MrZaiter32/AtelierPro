using System;
using System.Collections.Generic;
using System.Linq;

namespace AtelierPro.Models;

#region Enums

public enum EstadoPresupuesto
{
    Borrador,
    Aprobado,
    Rechazado,
    Cerrado,
    Facturado
}

public enum EstadoOrdenReparacion
{
    Pendiente,
    EnCurso,
    Completada,
    Facturada,
    Cancelada
}

public enum EstadoOrdenCompra
{
    Generada,
    Enviada,
    Recibida,
    Parcial,
    Cancelada
}

public enum EstadoOrdenServicio
{
    Pendiente,
    EnCurso,
    Completada,
    Facturada,
    Cancelada
}

public enum TipoMovimientoInventario
{
    Entrada,
    Salida,
    Ajuste,
    Devolucion
}

public enum TipoItemPresupuesto
{
    Pieza,
    ManoObra,
    Pintura
}

#endregion

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
    public int Cantidad { get; set; } = 1;
    public double TiempoAsignadoHoras { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PorcentajeAjuste { get; set; }
    public bool RequierePintura { get; set; }
    public bool RequiereDesmontajeDoble { get; set; }
    public bool RequiereAlineacion { get; set; }

    public decimal CostoBase => PrecioUnitario * (decimal)TiempoAsignadoHoras * Cantidad;
    public decimal CostoAjustado => CostoBase * (1 + PorcentajeAjuste / 100m);
}

#region Taller Models

/// <summary>
/// Técnico del taller con especialidades y disponibilidad.
/// </summary>
public sealed class Tecnico
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty; // Mecánica, Electricidad, Pintura, etc.
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public decimal HorasPorSemana { get; set; } = 40; // Horas disponibles por semana
    public decimal CostoPorHora { get; set; } // Para cálculos de costo real
    
    // Relaciones
    public IList<OrdenReparacion> OrdenesAsignadas { get; set; } = new List<OrdenReparacion>();
    
    public string NombreCompleto => $"{Nombre} {Apellido}";
}

/// <summary>
/// Orden de reparación mejorada con estados y seguimiento.
/// </summary>
public sealed class OrdenReparacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PresupuestoId { get; set; } // Referencia al presupuesto aprobado
    public Guid? TecnicoId { get; set; } // FK a Tecnico
    public Tecnico? TecnicoAsignado { get; set; }
    
    public EstadoOrdenReparacion Estado { get; set; } = EstadoOrdenReparacion.Pendiente;
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFinEstimada { get; set; }
    public DateTime? FechaFinReal { get; set; }
    
    public decimal HorasEstimadas { get; set; }
    public decimal HorasReales { get; set; }
    
    public string Observaciones { get; set; } = string.Empty;
    public string Prioridad { get; set; } = "Normal"; // Alto, Normal, Bajo
    
    // Relaciones
    public IList<ItemOrdenReparacion> Items { get; set; } = new List<ItemOrdenReparacion>();
    public IList<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();
}

/// <summary>
/// Items en una orden de reparación (copia del presupuesto con estado real).
/// </summary>
public sealed class ItemOrdenReparacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrdenReparacionId { get; set; }
    public Guid ItemPresupuestoId { get; set; }
    
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public TipoItemPresupuesto Tipo { get; set; }
    
    public decimal TiempoEstimadoHoras { get; set; }
    public decimal TiempoRealHoras { get; set; }
    
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; } = 1;
    
    public bool Completado { get; set; } = false;
}

#endregion

#region Almacen Models

/// <summary>
/// Refacción mejorada con control de inventario.
/// </summary>
public sealed class Refaccion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Sku { get; set; } = string.Empty; // Código único
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public int StockMaximo { get; set; }
    
    public decimal CostoPromedio { get; set; }
    public decimal PrecioVenta { get; set; }
    
    public string Categoria { get; set; } = string.Empty; // Motor, Eléctrica, Carrocería, etc.
    public string Ubicacion { get; set; } = string.Empty; // Pasillo/Estante/Posición
    
    public bool Activa { get; set; } = true;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    
    // Relaciones
    public IList<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    public IList<CuentoFisicoDetalle> DetallesCuentos { get; set; } = new List<CuentoFisicoDetalle>();
}

/// <summary>
/// Movimiento de inventario (entrada, salida, ajuste).
/// </summary>
public sealed class MovimientoInventario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RefaccionId { get; set; }
    public Refaccion? Refaccion { get; set; }
    
    public TipoMovimientoInventario Tipo { get; set; }
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    
    public string Razon { get; set; } = string.Empty; // "Compra a Proveedor XYZ", "Uso en Orden #123", etc.
    public string ResponsableUsuarioId { get; set; } = string.Empty; // FK a ApplicationUser
    
    // Opcional: referencia a compra u orden de reparación
    public Guid? OrdenCompraId { get; set; }
    public Guid? OrdenReparacionId { get; set; }
    
    public int StockAnterior { get; set; }
    public int StockPosterior { get; set; }
}

/// <summary>
/// Cuento físico de inventario.
/// </summary>
public sealed class CuentoFisico
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCompletacion { get; set; }
    
    public string Estado { get; set; } = "Pendiente"; // Pendiente, EnCurso, Completado
    public string Observaciones { get; set; } = string.Empty;
    public string ResponsableUsuarioId { get; set; } = string.Empty; // FK a ApplicationUser
    
    // Relaciones
    public IList<CuentoFisicoDetalle> Detalles { get; set; } = new List<CuentoFisicoDetalle>();
}

/// <summary>
/// Detalle de un cuento físico (por refacción).
/// </summary>
public sealed class CuentoFisicoDetalle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CuentoFisicoId { get; set; }
    public Guid RefaccionId { get; set; }
    public Refaccion? Refaccion { get; set; }
    
    public int StockSistema { get; set; }
    public int StockFisico { get; set; }
    public int Diferencia => StockFisico - StockSistema;
    
    public string Observaciones { get; set; } = string.Empty;
}

/// <summary>
/// Proveedor mejorado.
/// </summary>
public sealed class Proveedor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RazonSocial { get; set; } = string.Empty;
    public string Rfc { get; set; } = string.Empty; // Registro Federal de Contribuyentes (México)
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    
    public string ContactoPrincipal { get; set; } = string.Empty;
    public string CondicionesPago { get; set; } = "Contado"; // Contado, Crédito 15 días, Crédito 30 días, etc.
    
    public bool Activo { get; set; } = true;
    public decimal CalificacionPromedio { get; set; } = 5.0m; // 1-5 estrellas
    
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    
    // Relaciones
    public IList<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();
}

#endregion

#region Compras Models

/// <summary>
/// Orden de compra mejorada con workflow.
/// </summary>
public sealed class OrdenCompra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }
    
    public EstadoOrdenCompra Estado { get; set; } = EstadoOrdenCompra.Generada;
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEnvio { get; set; }
    public DateTime? FechaRecepcion { get; set; }
    
    public string Numero { get; set; } = string.Empty; // Número único por año: "OC-2024-001"
    public string Referencia { get; set; } = string.Empty; // Referencia del proveedor
    
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    
    public string Observaciones { get; set; } = string.Empty;
    public string ResponsableUsuarioId { get; set; } = string.Empty; // FK a ApplicationUser
    
    // Relaciones
    public IList<ItemOrdenCompra> Items { get; set; } = new List<ItemOrdenCompra>();
}

/// <summary>
/// Item en una orden de compra.
/// </summary>
public sealed class ItemOrdenCompra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrdenCompraId { get; set; }
    public Guid RefaccionId { get; set; }
    public Refaccion? Refaccion { get; set; }
    
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
    
    public int CantidadRecibida { get; set; } = 0;
    public bool Completado => CantidadRecibida >= Cantidad;
}

/// <summary>
/// Requisición de compra (solicitud previa a orden de compra).
/// </summary>
public sealed class Requisicion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    public string Numero { get; set; } = string.Empty; // Número único
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobada, Rechazada
    
    public string SolicitanteUsuarioId { get; set; } = string.Empty; // FK a ApplicationUser
    public string AprobadorUsuarioId { get; set; } = string.Empty; // FK a ApplicationUser
    
    public string Justificacion { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    
    // Relaciones
    public IList<ItemRequisicion> Items { get; set; } = new List<ItemRequisicion>();
}

/// <summary>
/// Item en una requisición.
/// </summary>
public sealed class ItemRequisicion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RequisicionId { get; set; }
    
    public string Sku { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioEstimado { get; set; }
    
    public string Observaciones { get; set; } = string.Empty;
}

#endregion

#region Servicios Adicionales

/// <summary>
/// Orden de servicio para servicios adicionales (pintura, detallado, etc.).
/// </summary>
public sealed class OrdenServicio
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrdenReparacionId { get; set; }
    
    public EstadoOrdenServicio Estado { get; set; } = EstadoOrdenServicio.Pendiente;
    
    public string Tipo { get; set; } = string.Empty; // Pintura, Detallado, Tapizado, Cristales, etc.
    public string Descripcion { get; set; } = string.Empty;
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    
    public decimal Precio { get; set; }
    public string ResponsableUsuarioId { get; set; } = string.Empty; // FK a ApplicationUser
    
    public string Observaciones { get; set; } = string.Empty;
    
    // Relaciones
    public IList<FotoServicio> Fotos { get; set; } = new List<FotoServicio>();
}

/// <summary>
/// Foto de un servicio (antes/después).
/// </summary>
public sealed class FotoServicio
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrdenServicioId { get; set; }
    
    public string Url { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty; // "Antes", "Después", "Detalle", etc.
    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;
}

#endregion

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
    
    /// <summary>Número único de presupuesto (ej: "P2024-00001").</summary>
    public string Numero { get; set; } = string.Empty;
    
    /// <summary>Referencia al cliente que solicita el presupuesto.</summary>
    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    
    public Vehiculo? Vehiculo { get; set; }
    public string? PlacaVehiculo { get; set; }
    public string? VinVehiculo { get; set; }
    
    public IList<ItemPresupuesto> Items { get; set; } = new List<ItemPresupuesto>();
    
    public decimal Subtotal => Items.Sum(i => i.CostoAjustado);
    public decimal IvaAplicado { get; set; }
    public decimal Total { get; set; }
    
    public EstadoPresupuesto Estado { get; set; } = EstadoPresupuesto.Borrador;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaAprobacion { get; set; }
    public string? Observaciones { get; set; }
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

#region Catálogos Models

/// <summary>
/// Referencias alternativas/cruzadas para refacciones.
/// </summary>
public sealed class ReferenciaAlternativa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RefaccionId { get; set; }
    public Refaccion? Refaccion { get; set; }
    
    public string FabricanteRef { get; set; } = string.Empty;
    public string PartNumberRef { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // 'Equivalente', 'Alternativo', 'Reemplazo'
    
    public string ProveedorCatalogo { get; set; } = string.Empty; // 'FinditParts', 'FleetPride', etc.
    public string UrlCatalogo { get; set; } = string.Empty;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Información de productos de catálogos externos.
/// </summary>
public sealed class ProductoCatalogo
{
    public string Proveedor { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<CrossReference> CrossReferences { get; set; } = new List<CrossReference>();
    public string AdditionalInfo { get; set; } = string.Empty;
}

/// <summary>
/// Referencia cruzada de productos.
/// </summary>
public sealed class CrossReference
{
    public string Manufacturer { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Equivalente";
    
    public override string ToString()
    {
        return $"{Manufacturer} {PartNumber}";
    }
}

/// <summary>
/// Resultado de búsqueda en catálogos.
/// </summary>
public sealed class ResultadoBusqueda
{
    public bool Success { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public List<ProductoCatalogo> Productos { get; set; } = new List<ProductoCatalogo>();
    public int TotalResultados => Productos.Count;
    public Dictionary<string, int> ResultadosPorProveedor { get; set; } = new Dictionary<string, int>();
    public DateTime FechaBusqueda { get; set; } = DateTime.UtcNow;
}

#endregion
