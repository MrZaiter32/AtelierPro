using AtelierPro.Models;
using AtelierPro.Data;
using Microsoft.EntityFrameworkCore;

namespace AtelierPro.Services;

/// <summary>
/// Servicio de lógica de negocio para el módulo Almacén.
/// Gestiona refacciones, movimientos de inventario y cuentos físicos.
/// </summary>
public class AlmacenService
{
    private readonly AtelierProDbContext _context;
    private readonly ILogger<AlmacenService> _logger;

    public AlmacenService(AtelierProDbContext context, ILogger<AlmacenService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Refacciones

    /// <summary>
    /// Obtiene todas las refacciones activas.
    /// </summary>
    public async Task<IEnumerable<Refaccion>> ObtenerRefaccionesAsync(bool soloActivas = true)
    {
        _logger.LogInformation("Obteniendo refacciones {filtro}", soloActivas ? "activas" : "todas");

        var query = _context.Refacciones.AsQueryable();
        if (soloActivas)
            query = query.Where(r => r.Activa);

        return await query
            .OrderBy(r => r.Categoria)
            .ThenBy(r => r.Sku)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene una refacción por SKU.
    /// </summary>
    public async Task<Refaccion?> ObtenerRefaccionPorSkuAsync(string sku)
    {
        return await _context.Refacciones
            .Include(r => r.Movimientos)
            .FirstOrDefaultAsync(r => r.Sku == sku);
    }

    /// <summary>
    /// Obtiene una refacción por ID.
    /// </summary>
    public async Task<Refaccion?> ObtenerRefaccionAsync(Guid id)
    {
        return await _context.Refacciones
            .Include(r => r.Movimientos)
            .Include(r => r.DetallesCuentos)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <summary>
    /// Crea una nueva refacción.
    /// </summary>
    public async Task<Refaccion> CrearRefaccionAsync(Refaccion refaccion)
    {
        if (string.IsNullOrWhiteSpace(refaccion.Sku))
            throw new ArgumentException("El SKU es requerido.");

        // Validar que el SKU no exista
        var existe = await _context.Refacciones.AnyAsync(r => r.Sku == refaccion.Sku);
        if (existe)
            throw new InvalidOperationException($"Ya existe una refacción con SKU {refaccion.Sku}");

        if (refaccion.StockMinimo < 0 || refaccion.StockMaximo <= 0)
            throw new ArgumentException("Los límites de stock deben ser válidos.");

        _logger.LogInformation("Creando refacción: {sku} - {nombre}", refaccion.Sku, refaccion.Nombre);

        _context.Refacciones.Add(refaccion);
        await _context.SaveChangesAsync();

        return refaccion;
    }

    /// <summary>
    /// Actualiza una refacción existente.
    /// </summary>
    public async Task<Refaccion> ActualizarRefaccionAsync(Guid id, Refaccion refaccionActualizada)
    {
        var refaccion = await _context.Refacciones.FindAsync(id)
            ?? throw new KeyNotFoundException($"Refacción {id} no encontrada.");

        refaccion.Nombre = refaccionActualizada.Nombre;
        refaccion.Descripcion = refaccionActualizada.Descripcion;
        refaccion.StockMinimo = refaccionActualizada.StockMinimo;
        refaccion.StockMaximo = refaccionActualizada.StockMaximo;
        refaccion.CostoPromedio = refaccionActualizada.CostoPromedio;
        refaccion.PrecioVenta = refaccionActualizada.PrecioVenta;
        refaccion.Categoria = refaccionActualizada.Categoria;
        refaccion.Ubicacion = refaccionActualizada.Ubicacion;
        refaccion.FechaActualizacion = DateTime.UtcNow;

        _logger.LogInformation("Actualizando refacción: {id}", id);
        await _context.SaveChangesAsync();

        return refaccion;
    }

    /// <summary>
    /// Desactiva una refacción.
    /// </summary>
    public async Task DesactivarRefaccionAsync(Guid id)
    {
        var refaccion = await _context.Refacciones.FindAsync(id)
            ?? throw new KeyNotFoundException($"Refacción {id} no encontrada.");

        refaccion.Activa = false;
        refaccion.FechaActualizacion = DateTime.UtcNow;

        _logger.LogInformation("Desactivando refacción: {id}", id);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Movimientos de Inventario

    /// <summary>
    /// Registra un movimiento de inventario (entrada, salida, ajuste).
    /// </summary>
    public async Task<MovimientoInventario> RegistrarMovimientoAsync(
        Guid refaccionId,
        TipoMovimientoInventario tipo,
        int cantidad,
        string razon,
        string responsableUsuarioId,
        Guid? ordenCompraId = null,
        Guid? ordenReparacionId = null)
    {
        var refaccion = await _context.Refacciones.FindAsync(refaccionId)
            ?? throw new KeyNotFoundException($"Refacción {refaccionId} no encontrada.");

        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a 0.");

        if (string.IsNullOrWhiteSpace(razon))
            throw new ArgumentException("La razón del movimiento es requerida.");

        int stockAnterior = refaccion.StockActual;
        int stockNuevo = stockAnterior;

        // Validar disponibilidad para salidas
        if (tipo == TipoMovimientoInventario.Salida || tipo == TipoMovimientoInventario.Devolucion)
        {
            if (refaccion.StockActual < cantidad)
                throw new InvalidOperationException(
                    $"Stock insuficiente. Disponible: {refaccion.StockActual}, Solicitado: {cantidad}");
            stockNuevo = stockAnterior - cantidad;
        }
        else if (tipo == TipoMovimientoInventario.Entrada)
        {
            if (stockAnterior + cantidad > refaccion.StockMaximo)
                _logger.LogWarning("Entrada de {refaccion} excede stock máximo", refaccionId);
            stockNuevo = stockAnterior + cantidad;
        }
        else if (tipo == TipoMovimientoInventario.Ajuste)
        {
            stockNuevo = cantidad; // Ajuste define el nuevo stock
        }

        // Crear movimiento
        var movimiento = new MovimientoInventario
        {
            RefaccionId = refaccionId,
            Tipo = tipo,
            Cantidad = cantidad,
            Razon = razon,
            ResponsableUsuarioId = responsableUsuarioId,
            OrdenCompraId = ordenCompraId,
            OrdenReparacionId = ordenReparacionId,
            StockAnterior = stockAnterior,
            StockPosterior = stockNuevo
        };

        // Actualizar stock de la refacción
        refaccion.StockActual = stockNuevo;
        refaccion.FechaActualizacion = DateTime.UtcNow;

        _logger.LogInformation(
            "Movimiento registrado: {tipo} x{cantidad} de {refaccion}. Stock: {anterior} → {nuevo}",
            tipo, cantidad, refaccionId, stockAnterior, stockNuevo);

        _context.MovimientosInventario.Add(movimiento);
        await _context.SaveChangesAsync();

        return movimiento;
    }

    /// <summary>
    /// Obtiene el historial de movimientos de una refacción.
    /// </summary>
    public async Task<IEnumerable<MovimientoInventario>> ObtenerHistorialMovimientosAsync(
        Guid refaccionId,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var query = _context.MovimientosInventario
            .Where(m => m.RefaccionId == refaccionId)
            .AsQueryable();

        if (desde.HasValue)
            query = query.Where(m => m.Fecha >= desde);

        if (hasta.HasValue)
            query = query.Where(m => m.Fecha <= hasta);

        return await query
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }

    #endregion

    #region Cuentos Físicos

    /// <summary>
    /// Crea un nuevo cuento físico de inventario.
    /// </summary>
    public async Task<CuentoFisico> CrearCuentoFisicoAsync(string responsableUsuarioId, string observaciones = "")
    {
        if (string.IsNullOrWhiteSpace(responsableUsuarioId))
            throw new ArgumentException("El responsable es requerido.");

        _logger.LogInformation("Creando cuento físico");

        var cuento = new CuentoFisico
        {
            ResponsableUsuarioId = responsableUsuarioId,
            Observaciones = observaciones,
            Estado = "Pendiente"
        };

        _context.CuentosFisicos.Add(cuento);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cuento físico creado: {id}", cuento.Id);
        return cuento;
    }

    /// <summary>
    /// Agrega un detalle a un cuento físico.
    /// </summary>
    public async Task<CuentoFisicoDetalle> AgregarDetalleAsync(
        Guid cuentoId,
        Guid refaccionId,
        int stockFisico,
        string observaciones = "")
    {
        var cuento = await _context.CuentosFisicos.FindAsync(cuentoId)
            ?? throw new KeyNotFoundException($"Cuento físico {cuentoId} no encontrado.");

        var refaccion = await _context.Refacciones.FindAsync(refaccionId)
            ?? throw new KeyNotFoundException($"Refacción {refaccionId} no encontrada.");

        if (cuento.Estado != "Pendiente" && cuento.Estado != "EnCurso")
            throw new InvalidOperationException("Solo se pueden agregar detalles a cuentos en Pendiente o EnCurso.");

        var detalle = new CuentoFisicoDetalle
        {
            CuentoFisicoId = cuentoId,
            RefaccionId = refaccionId,
            StockSistema = refaccion.StockActual,
            StockFisico = stockFisico,
            Observaciones = observaciones
        };

        _logger.LogInformation("Detalle agregado: {refaccion} Sistema={sistema} Físico={fisico}",
            refaccionId, refaccion.StockActual, stockFisico);

        _context.DetallesCuentoFisico.Add(detalle);
        await _context.SaveChangesAsync();

        return detalle;
    }

    /// <summary>
    /// Completa un cuento físico y aplica ajustes.
    /// </summary>
    public async Task<CuentoFisico> CompletarCuentoFisicoAsync(Guid cuentoId, string responsableUsuarioId)
    {
        var cuento = await _context.CuentosFisicos
            .Include(c => c.Detalles)
            .FirstOrDefaultAsync(c => c.Id == cuentoId)
            ?? throw new KeyNotFoundException($"Cuento físico {cuentoId} no encontrado.");

        if (cuento.Estado == "Completado")
            throw new InvalidOperationException("Este cuento ya fue completado.");

        cuento.FechaCompletacion = DateTime.UtcNow;
        cuento.Estado = "Completado";

        _logger.LogInformation("Completando cuento físico: {id}. Aplicando {cantidad} ajustes",
            cuentoId, cuento.Detalles.Count(d => d.Diferencia != 0));

        // Aplicar ajustes automáticos
        foreach (var detalle in cuento.Detalles.Where(d => d.Diferencia != 0))
        {
            await RegistrarMovimientoAsync(
                refaccionId: detalle.RefaccionId,
                tipo: TipoMovimientoInventario.Ajuste,
                cantidad: detalle.StockFisico,
                razon: $"Ajuste por cuento físico {cuentoId}",
                responsableUsuarioId: responsableUsuarioId);
        }

        await _context.SaveChangesAsync();
        return cuento;
    }

    /// <summary>
    /// Obtiene los cuentos físicos completados.
    /// </summary>
    public async Task<IEnumerable<CuentoFisico>> ObtenerCuentosFisicosAsync(string? estado = null)
    {
        var query = _context.CuentosFisicos
            .Include(c => c.Detalles)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(c => c.Estado == estado);

        return await query
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();
    }

    #endregion

    #region Alertas y Análisis

    /// <summary>
    /// Obtiene refacciones con stock bajo.
    /// </summary>
    public async Task<IEnumerable<Refaccion>> ObtenerRefaccionesStockBajoAsync()
    {
        _logger.LogInformation("Buscando refacciones con stock bajo");

        return await _context.Refacciones
            .Where(r => r.Activa && r.StockActual <= r.StockMinimo)
            .OrderBy(r => r.StockActual)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene refacciones con stock crítico (cero o negativo).
    /// </summary>
    public async Task<IEnumerable<Refaccion>> ObtenerRefaccionesCriticasAsync()
    {
        return await _context.Refacciones
            .Where(r => r.Activa && r.StockActual <= 0)
            .OrderBy(r => r.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Calcula el valor total del inventario.
    /// </summary>
    public async Task<decimal> ObtenerValorTotalInventarioAsync()
    {
        return await _context.Refacciones
            .Where(r => r.Activa)
            .SumAsync(r => r.StockActual * r.CostoPromedio);
    }

    /// <summary>
    /// Obtiene estadísticas del almacén.
    /// </summary>
    public async Task<Dictionary<string, object>> ObtenerEstadísticasAsync()
    {
        var totalRefacciones = await _context.Refacciones.CountAsync(r => r.Activa);
        var stockBajo = await ObtenerRefaccionesStockBajoAsync();
        var stockCritico = await ObtenerRefaccionesCriticasAsync();
        var valorTotal = await ObtenerValorTotalInventarioAsync();

        var ultimosMovimientos = await _context.MovimientosInventario
            .OrderByDescending(m => m.Fecha)
            .Take(10)
            .CountAsync();

        return new Dictionary<string, object>
        {
            { "TotalRefacciones", totalRefacciones },
            { "RefaccionesStockBajo", stockBajo.Count() },
            { "RefaccionesCriticas", stockCritico.Count() },
            { "ValorTotalInventario", Math.Round(valorTotal, 2) },
            { "UltimosMovimientos10Dias", ultimosMovimientos }
        };
    }

    /// <summary>
    /// Obtiene refacciones más vendidas.
    /// </summary>
    public async Task<IEnumerable<(Refaccion Refaccion, int SalidasTotal)>> ObtenerRefaccionesMasVendidasAsync(
        int top = 10,
        int diasUltimos = 90)
    {
        var desde = DateTime.UtcNow.AddDays(-diasUltimos);

        var refaccionesVendidas = await _context.MovimientosInventario
            .Where(m => m.Fecha >= desde && 
                        (m.Tipo == TipoMovimientoInventario.Salida || 
                         m.Tipo == TipoMovimientoInventario.Devolucion))
            .GroupBy(m => m.RefaccionId)
            .Select(g => new
            {
                RefaccionId = g.Key,
                TotalSalidas = g.Sum(m => m.Cantidad)
            })
            .OrderByDescending(x => x.TotalSalidas)
            .Take(top)
            .ToListAsync();

        var resultado = new List<(Refaccion, int)>();
        foreach (var item in refaccionesVendidas)
        {
            var refaccion = await _context.Refacciones.FindAsync(item.RefaccionId);
            if (refaccion != null)
                resultado.Add((refaccion, item.TotalSalidas));
        }

        return resultado;
    }

    #endregion

    #region Disponibilidad

    /// <summary>
    /// Verifica la disponibilidad de una refacción.
    /// </summary>
    public async Task<bool> VerificarDisponibilidadAsync(Guid refaccionId, int cantidad)
    {
        var refaccion = await _context.Refacciones.FindAsync(refaccionId);
        return refaccion != null && refaccion.StockActual >= cantidad;
    }

    /// <summary>
    /// Obtiene información de disponibilidad para múltiples refacciones.
    /// </summary>
    public async Task<Dictionary<Guid, (bool Disponible, int Stock)>> VerificarDisponibilidadMultipleAsync(
        Dictionary<Guid, int> refaccionesRequeridas)
    {
        var resultado = new Dictionary<Guid, (bool, int)>();

        foreach (var (refaccionId, cantidadRequerida) in refaccionesRequeridas)
        {
            var disponible = await VerificarDisponibilidadAsync(refaccionId, cantidadRequerida);
            var refaccion = await _context.Refacciones.FindAsync(refaccionId);
            resultado[refaccionId] = (disponible, refaccion?.StockActual ?? 0);
        }

        return resultado;
    }

    #endregion
}
