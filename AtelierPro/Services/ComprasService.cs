using AtelierPro.Models;
using AtelierPro.Data;
using Microsoft.EntityFrameworkCore;

namespace AtelierPro.Services;

/// <summary>
/// Servicio de lógica de negocio para el módulo Compras.
/// Gestiona órdenes de compra, requisiciones y proveedores.
/// </summary>
public class ComprasService
{
    private readonly AtelierProDbContext _context;
    private readonly ILogger<ComprasService> _logger;

    public ComprasService(AtelierProDbContext context, ILogger<ComprasService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Proveedores

    /// <summary>
    /// Obtiene todos los proveedores activos.
    /// </summary>
    public async Task<IEnumerable<Proveedor>> ObtenerProveedoresAsync(bool soloActivos = true)
    {
        _logger.LogInformation("Obteniendo proveedores {filtro}", soloActivos ? "activos" : "todos");

        var query = _context.Proveedores.AsQueryable();
        if (soloActivos)
            query = query.Where(p => p.Activo);

        return await query
            .OrderBy(p => p.RazonSocial)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene un proveedor por ID.
    /// </summary>
    public async Task<Proveedor?> ObtenerProveedorAsync(Guid id)
    {
        return await _context.Proveedores
            .Include(p => p.OrdenesCompra)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Crea un nuevo proveedor.
    /// </summary>
    public async Task<Proveedor> CrearProveedorAsync(Proveedor proveedor)
    {
        if (string.IsNullOrWhiteSpace(proveedor.RazonSocial))
            throw new ArgumentException("La razón social es requerida.");

        if (string.IsNullOrWhiteSpace(proveedor.Rfc))
            throw new ArgumentException("El RFC es requerido.");

        // Validar RFC único
        var existe = await _context.Proveedores.AnyAsync(p => p.Rfc == proveedor.Rfc);
        if (existe)
            throw new InvalidOperationException($"Ya existe un proveedor con RFC {proveedor.Rfc}");

        _logger.LogInformation("Creando proveedor: {razonSocial} - {rfc}", 
            proveedor.RazonSocial, proveedor.Rfc);

        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        return proveedor;
    }

    /// <summary>
    /// Actualiza un proveedor existente.
    /// </summary>
    public async Task<Proveedor> ActualizarProveedorAsync(Guid id, Proveedor proveedorActualizado)
    {
        var proveedor = await _context.Proveedores.FindAsync(id)
            ?? throw new KeyNotFoundException($"Proveedor {id} no encontrado.");

        proveedor.RazonSocial = proveedorActualizado.RazonSocial;
        proveedor.Telefono = proveedorActualizado.Telefono;
        proveedor.Email = proveedorActualizado.Email;
        proveedor.Direccion = proveedorActualizado.Direccion;
        proveedor.ContactoPrincipal = proveedorActualizado.ContactoPrincipal;
        proveedor.CondicionesPago = proveedorActualizado.CondicionesPago;

        _logger.LogInformation("Actualizando proveedor: {id}", id);
        await _context.SaveChangesAsync();

        return proveedor;
    }

    /// <summary>
    /// Califica un proveedor (1-5 estrellas).
    /// </summary>
    public async Task<Proveedor> CalificarProveedorAsync(Guid id, decimal calificacion)
    {
        if (calificacion < 1 || calificacion > 5)
            throw new ArgumentException("La calificación debe estar entre 1 y 5.");

        var proveedor = await _context.Proveedores.FindAsync(id)
            ?? throw new KeyNotFoundException($"Proveedor {id} no encontrado.");

        proveedor.CalificacionPromedio = calificacion;

        _logger.LogInformation("Proveedor {id} calificado: {estrellas} estrellas", id, calificacion);
        await _context.SaveChangesAsync();

        return proveedor;
    }

    #endregion

    #region Requisiciones

    /// <summary>
    /// Crea una nueva requisición de compra.
    /// </summary>
    public async Task<Requisicion> CrearRequisicionAsync(
        string solicitanteUsuarioId,
        List<(string Sku, string Descripcion, int Cantidad, decimal PrecioEstimado)> items,
        string justificacion = "")
    {
        if (string.IsNullOrWhiteSpace(solicitanteUsuarioId))
            throw new ArgumentException("El solicitante es requerido.");

        if (!items.Any())
            throw new ArgumentException("La requisición debe tener al menos un item.");

        _logger.LogInformation("Creando requisición de compra por {usuario}", solicitanteUsuarioId);

        var requisicion = new Requisicion
        {
            Numero = await GenerarNumeroRequisicionAsync(),
            SolicitanteUsuarioId = solicitanteUsuarioId,
            Justificacion = justificacion,
            Estado = "Pendiente"
        };

        foreach (var (sku, descripcion, cantidad, precioEstimado) in items)
        {
            var item = new ItemRequisicion
            {
                Sku = sku,
                Descripcion = descripcion,
                Cantidad = cantidad,
                PrecioEstimado = precioEstimado
            };
            requisicion.Items.Add(item);
        }

        _context.Requisiciones.Add(requisicion);
        await _context.SaveChangesAsync();

        return requisicion;
    }

    /// <summary>
    /// Aprueba una requisición de compra.
    /// </summary>
    public async Task<Requisicion> AprobarRequisicionAsync(Guid requisiconId, string aprobadorUsuarioId)
    {
        var requisicion = await _context.Requisiciones
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == requisiconId)
            ?? throw new KeyNotFoundException($"Requisición {requisiconId} no encontrada.");

        if (requisicion.Estado != "Pendiente")
            throw new InvalidOperationException("Solo requisiciones pendientes pueden ser aprobadas.");

        requisicion.Estado = "Aprobada";
        requisicion.AprobadorUsuarioId = aprobadorUsuarioId;

        _logger.LogInformation("Requisición {numero} aprobada por {usuario}", 
            requisicion.Numero, aprobadorUsuarioId);

        await _context.SaveChangesAsync();
        return requisicion;
    }

    /// <summary>
    /// Rechaza una requisición de compra.
    /// </summary>
    public async Task<Requisicion> RechazarRequisicionAsync(Guid requisiconId, string observaciones = "")
    {
        var requisicion = await _context.Requisiciones.FindAsync(requisiconId)
            ?? throw new KeyNotFoundException($"Requisición {requisiconId} no encontrada.");

        if (requisicion.Estado != "Pendiente")
            throw new InvalidOperationException("Solo requisiciones pendientes pueden ser rechazadas.");

        requisicion.Estado = "Rechazada";
        requisicion.Observaciones = observaciones;

        _logger.LogInformation("Requisición {numero} rechazada", requisicion.Numero);
        await _context.SaveChangesAsync();

        return requisicion;
    }

    /// <summary>
    /// Obtiene requisiciones por estado.
    /// </summary>
    public async Task<IEnumerable<Requisicion>> ObtenerRequisicionesAsync(string? estado = null)
    {
        var query = _context.Requisiciones
            .Include(r => r.Items)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(r => r.Estado == estado);

        return await query
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();
    }

    #endregion

    #region Órdenes de Compra

    /// <summary>
    /// Crea una nueva orden de compra.
    /// </summary>
    public async Task<OrdenCompra> CrearOrdenCompraAsync(
        Guid proveedorId,
        List<(Guid RefaccionId, int Cantidad, decimal PrecioUnitario)> items,
        string responsableUsuarioId,
        string observaciones = "")
    {
        var proveedor = await _context.Proveedores.FindAsync(proveedorId)
            ?? throw new KeyNotFoundException($"Proveedor {proveedorId} no encontrado.");

        if (!proveedor.Activo)
            throw new InvalidOperationException($"El proveedor {proveedor.RazonSocial} está inactivo.");

        if (!items.Any())
            throw new ArgumentException("La orden debe tener al menos un item.");

        _logger.LogInformation("Creando orden de compra para proveedor: {proveedor}",
            proveedor.RazonSocial);

        var orden = new OrdenCompra
        {
            ProveedorId = proveedorId,
            Numero = await GenerarNumeroOrdenCompraAsync(),
            Estado = EstadoOrdenCompra.Generada,
            ResponsableUsuarioId = responsableUsuarioId,
            Observaciones = observaciones
        };

        decimal subtotal = 0;

        foreach (var (refaccionId, cantidad, precioUnitario) in items)
        {
            var refaccion = await _context.Refacciones.FindAsync(refaccionId)
                ?? throw new KeyNotFoundException($"Refacción {refaccionId} no encontrada.");

            var item = new ItemOrdenCompra
            {
                RefaccionId = refaccionId,
                Cantidad = cantidad,
                PrecioUnitario = precioUnitario
            };
            orden.Items.Add(item);
            subtotal += cantidad * precioUnitario;
        }

        // Calcular IVA (16% México)
        orden.Subtotal = subtotal;
        orden.Iva = Math.Round(subtotal * 0.16m, 2);
        orden.Total = orden.Subtotal + orden.Iva;

        _context.OrdenesCompra.Add(orden);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Orden de compra creada: {numero} - Total: ${total}",
            orden.Numero, orden.Total);

        return orden;
    }

    /// <summary>
    /// Obtiene órdenes de compra con filtros.
    /// </summary>
    public async Task<IEnumerable<OrdenCompra>> ObtenerOrdenesCompraAsync(
        EstadoOrdenCompra? estado = null,
        Guid? proveedorId = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var query = _context.OrdenesCompra
            .Include(o => o.Proveedor)
            .Include(o => o.Items)
            .AsQueryable();

        if (estado.HasValue)
            query = query.Where(o => o.Estado == estado);

        if (proveedorId.HasValue)
            query = query.Where(o => o.ProveedorId == proveedorId);

        if (desde.HasValue)
            query = query.Where(o => o.FechaCreacion >= desde);

        if (hasta.HasValue)
            query = query.Where(o => o.FechaCreacion <= hasta);

        return await query
            .OrderByDescending(o => o.FechaCreacion)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene una orden de compra por ID.
    /// </summary>
    public async Task<OrdenCompra?> ObtenerOrdenCompraAsync(Guid id)
    {
        return await _context.OrdenesCompra
            .Include(o => o.Proveedor)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    /// <summary>
    /// Envía una orden de compra al proveedor.
    /// </summary>
    public async Task<OrdenCompra> EnviarOrdenCompraAsync(Guid ordenId)
    {
        var orden = await _context.OrdenesCompra.FindAsync(ordenId)
            ?? throw new KeyNotFoundException($"Orden de compra {ordenId} no encontrada.");

        if (orden.Estado != EstadoOrdenCompra.Generada)
            throw new InvalidOperationException("Solo órdenes generadas pueden ser enviadas.");

        orden.Estado = EstadoOrdenCompra.Enviada;
        orden.FechaEnvio = DateTime.UtcNow;

        _logger.LogInformation("Orden de compra {numero} enviada al proveedor", orden.Numero);
        await _context.SaveChangesAsync();

        return orden;
    }

    /// <summary>
    /// Registra la recepción de una orden de compra.
    /// </summary>
    public async Task<OrdenCompra> ConfirmarRecepcionAsync(
        Guid ordenId,
        Dictionary<Guid, int> cantidadesRecibidas,
        string observaciones = "")
    {
        var orden = await _context.OrdenesCompra
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == ordenId)
            ?? throw new KeyNotFoundException($"Orden de compra {ordenId} no encontrada.");

        if (orden.Estado == EstadoOrdenCompra.Recibida)
            throw new InvalidOperationException("Esta orden ya fue completamente recibida.");

        bool todoRecibido = true;

        foreach (var item in orden.Items)
        {
            if (cantidadesRecibidas.TryGetValue(item.RefaccionId, out var cantidadRecibida))
            {
                item.CantidadRecibida = cantidadRecibida;
                if (cantidadRecibida < item.Cantidad)
                    todoRecibido = false;
            }
        }

        if (todoRecibido)
            orden.Estado = EstadoOrdenCompra.Recibida;
        else
            orden.Estado = EstadoOrdenCompra.Parcial;

        orden.FechaRecepcion = DateTime.UtcNow;

        _logger.LogInformation("Orden de compra {numero} recepción confirmada. Estado: {estado}",
            orden.Numero, orden.Estado);

        await _context.SaveChangesAsync();
        return orden;
    }

    /// <summary>
    /// Calcula el estado de pago de una orden.
    /// </summary>
    public async Task<(EstadoOrdenCompra Estado, decimal Pagado, decimal Pendiente)> ObtenerEstadoPagoAsync(Guid ordenId)
    {
        var orden = await _context.OrdenesCompra.FindAsync(ordenId)
            ?? throw new KeyNotFoundException($"Orden de compra {ordenId} no encontrada.");

        // Por ahora, asumimos que todo se paga al recibir (simplificación)
        var pagado = orden.Estado == EstadoOrdenCompra.Recibida ? orden.Total : 0m;
        var pendiente = orden.Total - pagado;

        return (orden.Estado, pagado, pendiente);
    }

    #endregion

    #region Utilidades

    /// <summary>
    /// Genera un número único para nueva orden de compra.
    /// </summary>
    private async Task<string> GenerarNumeroOrdenCompraAsync()
    {
        var anio = DateTime.UtcNow.Year;
        var ultimaOrden = await _context.OrdenesCompra
            .Where(o => o.Numero.StartsWith($"OC-{anio}"))
            .OrderByDescending(o => o.Numero)
            .FirstOrDefaultAsync();

        int secuencia = 1;
        if (ultimaOrden != null)
        {
            var numeroParts = ultimaOrden.Numero.Split('-');
            if (int.TryParse(numeroParts.Last(), out int numeroActual))
                secuencia = numeroActual + 1;
        }

        return $"OC-{anio}-{secuencia:D3}";
    }

    /// <summary>
    /// Genera un número único para nueva requisición.
    /// </summary>
    private async Task<string> GenerarNumeroRequisicionAsync()
    {
        var anio = DateTime.UtcNow.Year;
        var ultimaRequisicion = await _context.Requisiciones
            .Where(r => r.Numero.StartsWith($"REQ-{anio}"))
            .OrderByDescending(r => r.Numero)
            .FirstOrDefaultAsync();

        int secuencia = 1;
        if (ultimaRequisicion != null)
        {
            var numeroParts = ultimaRequisicion.Numero.Split('-');
            if (int.TryParse(numeroParts.Last(), out int numeroActual))
                secuencia = numeroActual + 1;
        }

        return $"REQ-{anio}-{secuencia:D3}";
    }

    #endregion

    #region Reportes

    /// <summary>
    /// Obtiene estadísticas del módulo de compras.
    /// </summary>
    public async Task<Dictionary<string, object>> ObtenerEstadísticasAsync()
    {
        var ordenesTotal = await _context.OrdenesCompra.CountAsync();
        var ordenesPendientes = await _context.OrdenesCompra
            .CountAsync(o => o.Estado == EstadoOrdenCompra.Generada || 
                            o.Estado == EstadoOrdenCompra.Enviada);

        var proveedoresActivos = await _context.Proveedores.CountAsync(p => p.Activo);

        var gastosTotal = await _context.OrdenesCompra
            .Where(o => o.Estado == EstadoOrdenCompra.Recibida)
            .SumAsync(o => o.Total);

        var requisicionesPendientes = await _context.Requisiciones
            .CountAsync(r => r.Estado == "Pendiente");

        return new Dictionary<string, object>
        {
            { "OrdenesTotales", ordenesTotal },
            { "OrdenesPendientes", ordenesPendientes },
            { "ProveedoresActivos", proveedoresActivos },
            { "GastosTotalRecibidos", Math.Round(gastosTotal, 2) },
            { "RequisicionesPendientes", requisicionesPendientes }
        };
    }

    /// <summary>
    /// Obtiene proveedores más utilizados.
    /// </summary>
    public async Task<IEnumerable<(Proveedor Proveedor, int OrdenesTotales, decimal MontoTotal)>> ObtenerProveedoresMasUtilizadosAsync(
        int top = 5,
        int diasUltimos = 90)
    {
        var desde = DateTime.UtcNow.AddDays(-diasUltimos);

        var proveedoresUsados = await _context.OrdenesCompra
            .Where(o => o.FechaCreacion >= desde)
            .GroupBy(o => o.ProveedorId)
            .Select(g => new
            {
                ProveedorId = g.Key,
                OrdenesTotales = g.Count(),
                MontoTotal = g.Sum(o => o.Total)
            })
            .OrderByDescending(x => x.MontoTotal)
            .Take(top)
            .ToListAsync();

        var resultado = new List<(Proveedor, int, decimal)>();
        foreach (var item in proveedoresUsados)
        {
            var proveedor = await _context.Proveedores.FindAsync(item.ProveedorId);
            if (proveedor != null)
                resultado.Add((proveedor, item.OrdenesTotales, item.MontoTotal));
        }

        return resultado;
    }

    #endregion
}
