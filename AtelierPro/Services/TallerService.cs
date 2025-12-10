using AtelierPro.Models;
using AtelierPro.Data;
using Microsoft.EntityFrameworkCore;

namespace AtelierPro.Services;

/// <summary>
/// Servicio de lógica de negocio para el módulo Taller.
/// Gestiona órdenes de reparación, técnicos, y seguimiento.
/// </summary>
public class TallerService
{
    private readonly AtelierProDbContext _context;
    private readonly ILogger<TallerService> _logger;

    public TallerService(AtelierProDbContext context, ILogger<TallerService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Técnicos

    /// <summary>
    /// Obtiene todos los técnicos activos.
    /// </summary>
    public async Task<IEnumerable<Tecnico>> ObtenerTecnicosAsync(bool soloActivos = true)
    {
        _logger.LogInformation("Obteniendo técnicos {filtro}", soloActivos ? "activos" : "todos");
        
        var query = _context.Tecnicos.AsQueryable();
        if (soloActivos)
            query = query.Where(t => t.Activo);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene un técnico por ID.
    /// </summary>
    public async Task<Tecnico?> ObtenerTecnicoPorIdAsync(Guid id)
    {
        return await _context.Tecnicos
            .Include(t => t.OrdenesAsignadas)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <summary>
    /// Crea un nuevo técnico.
    /// </summary>
    public async Task<Tecnico> CrearTecnicoAsync(Tecnico tecnico)
    {
        if (string.IsNullOrWhiteSpace(tecnico.Nombre))
            throw new ArgumentException("El nombre del técnico es requerido.");
        if (tecnico.CostoPorHora <= 0)
            throw new ArgumentException("El costo por hora debe ser mayor a 0.");

        _logger.LogInformation("Creando técnico: {nombre}", tecnico.Nombre);
        
        _context.Tecnicos.Add(tecnico);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Técnico creado: {id}", tecnico.Id);
        return tecnico;
    }

    /// <summary>
    /// Actualiza un técnico existente.
    /// </summary>
    public async Task<Tecnico> ActualizarTecnicoAsync(Guid id, Tecnico tecnicoActualizado)
    {
        var tecnico = await _context.Tecnicos.FindAsync(id)
            ?? throw new KeyNotFoundException($"Técnico {id} no encontrado.");

        tecnico.Nombre = tecnicoActualizado.Nombre;
        tecnico.Apellido = tecnicoActualizado.Apellido;
        tecnico.Especialidad = tecnicoActualizado.Especialidad;
        tecnico.Telefono = tecnicoActualizado.Telefono;
        tecnico.Email = tecnicoActualizado.Email;
        tecnico.CostoPorHora = tecnicoActualizado.CostoPorHora;
        tecnico.HorasPorSemana = tecnicoActualizado.HorasPorSemana;
        tecnico.Activo = tecnicoActualizado.Activo;

        _logger.LogInformation("Actualizando técnico: {id}", id);
        await _context.SaveChangesAsync();
        
        return tecnico;
    }

    /// <summary>
    /// Desactiva un técnico (soft delete).
    /// </summary>
    public async Task DesactivarTecnicoAsync(Guid id)
    {
        var tecnico = await _context.Tecnicos.FindAsync(id)
            ?? throw new KeyNotFoundException($"Técnico {id} no encontrado.");

        tecnico.Activo = false;
        _logger.LogInformation("Desactivando técnico: {id}", id);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Órdenes de Reparación

    /// <summary>
    /// Obtiene todas las órdenes de reparación con filtros opcionales.
    /// </summary>
    public async Task<IEnumerable<OrdenReparacion>> ObtenerOrdenesReparacionAsync(
        EstadoOrdenReparacion? estado = null,
        Guid? tecnicoId = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var query = _context.OrdenesReparacion
            .Include(o => o.TecnicoAsignado)
            .Include(o => o.Items)
            .AsQueryable();

        if (estado.HasValue)
            query = query.Where(o => o.Estado == estado);

        if (tecnicoId.HasValue)
            query = query.Where(o => o.TecnicoId == tecnicoId);

        if (desde.HasValue)
            query = query.Where(o => o.FechaCreacion >= desde);

        if (hasta.HasValue)
            query = query.Where(o => o.FechaCreacion <= hasta);

        _logger.LogInformation("Obteniendo órdenes de reparación con filtros");
        return await query.OrderByDescending(o => o.FechaCreacion).ToListAsync();
    }

    /// <summary>
    /// Obtiene órdenes de reparación activas (en progreso o pendientes).
    /// </summary>
    public async Task<IEnumerable<OrdenReparacion>> ObtenerOrdenesActivasAsync()
    {
        return await ObtenerOrdenesReparacionAsync(
            estado: EstadoOrdenReparacion.Pendiente);
    }

    /// <summary>
    /// Obtiene una orden de reparación por ID.
    /// </summary>
    public async Task<OrdenReparacion?> ObtenerOrdenReparacionAsync(Guid id)
    {
        return await _context.OrdenesReparacion
            .Include(o => o.TecnicoAsignado)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    /// <summary>
    /// Crea una nueva orden de reparación a partir de un presupuesto aprobado.
    /// </summary>
    public async Task<OrdenReparacion> CrearOrdenReparacionAsync(
        Guid presupuestoId,
        decimal horasEstimadas,
        string prioridad = "Normal")
    {
        var presupuesto = await _context.Presupuestos
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == presupuestoId)
            ?? throw new KeyNotFoundException($"Presupuesto {presupuestoId} no encontrado.");

        if (presupuesto.Estado != EstadoPresupuesto.Aprobado)
            throw new InvalidOperationException(
                $"Solo se pueden crear órdenes a partir de presupuestos aprobados. Estado actual: {presupuesto.Estado}");

        _logger.LogInformation("Creando orden de reparación para presupuesto: {id}", presupuestoId);

        var orden = new OrdenReparacion
        {
            PresupuestoId = presupuestoId,
            Estado = EstadoOrdenReparacion.Pendiente,
            HorasEstimadas = horasEstimadas,
            Prioridad = prioridad
        };

        // Crear items de la orden a partir del presupuesto
        foreach (var item in presupuesto.Items)
        {
            var itemOrden = new ItemOrdenReparacion
            {
                ItemPresupuestoId = item.Id,
                Codigo = item.Codigo,
                Descripcion = item.Descripcion,
                Tipo = item.Tipo,
                TiempoEstimadoHoras = (decimal)item.TiempoAsignadoHoras,
                PrecioUnitario = item.PrecioUnitario,
                Cantidad = 1
            };
            orden.Items.Add(itemOrden);
        }

        _context.OrdenesReparacion.Add(orden);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Orden de reparación creada: {id}", orden.Id);
        return orden;
    }

    /// <summary>
    /// Asigna un técnico a una orden de reparación.
    /// </summary>
    public async Task<OrdenReparacion> AsignarTecnicoAsync(Guid ordenId, Guid tecnicoId)
    {
        var orden = await _context.OrdenesReparacion.FindAsync(ordenId)
            ?? throw new KeyNotFoundException($"Orden de reparación {ordenId} no encontrada.");

        var tecnico = await _context.Tecnicos.FindAsync(tecnicoId)
            ?? throw new KeyNotFoundException($"Técnico {tecnicoId} no encontrado.");

        if (!tecnico.Activo)
            throw new InvalidOperationException($"El técnico {tecnico.NombreCompleto} está inactivo.");

        orden.TecnicoId = tecnicoId;
        orden.TecnicoAsignado = tecnico;

        _logger.LogInformation("Técnico {tecnico} asignado a orden {orden}", 
            tecnico.NombreCompleto, ordenId);
        
        await _context.SaveChangesAsync();
        return orden;
    }

    /// <summary>
    /// Cambia el estado de una orden de reparación.
    /// </summary>
    public async Task<OrdenReparacion> CambiarEstadoAsync(
        Guid ordenId,
        EstadoOrdenReparacion nuevoEstado)
    {
        var orden = await _context.OrdenesReparacion.FindAsync(ordenId)
            ?? throw new KeyNotFoundException($"Orden de reparación {ordenId} no encontrada.");

        var estadoAnterior = orden.Estado;

        // Validar transiciones de estado válidas
        var transicionesValidas = ValidarTransicionEstado(estadoAnterior, nuevoEstado);
        if (!transicionesValidas)
            throw new InvalidOperationException(
                $"Transición inválida de {estadoAnterior} a {nuevoEstado}");

        // Actualizar tiempos según estado
        if (nuevoEstado == EstadoOrdenReparacion.EnCurso && !orden.FechaInicio.HasValue)
            orden.FechaInicio = DateTime.UtcNow;

        if (nuevoEstado == EstadoOrdenReparacion.Completada && !orden.FechaFinReal.HasValue)
            orden.FechaFinReal = DateTime.UtcNow;

        orden.Estado = nuevoEstado;

        _logger.LogInformation("Orden {id} cambió de {anterior} a {nuevo}",
            ordenId, estadoAnterior, nuevoEstado);

        await _context.SaveChangesAsync();
        return orden;
    }

    /// <summary>
    /// Valida si una transición de estado es permitida.
    /// </summary>
    private static bool ValidarTransicionEstado(
        EstadoOrdenReparacion estadoActual,
        EstadoOrdenReparacion estadoNuevo)
    {
        // Máquina de estados permitida
        var transiciones = new Dictionary<EstadoOrdenReparacion, List<EstadoOrdenReparacion>>
        {
            { EstadoOrdenReparacion.Pendiente, new() { EstadoOrdenReparacion.EnCurso, EstadoOrdenReparacion.Cancelada } },
            { EstadoOrdenReparacion.EnCurso, new() { EstadoOrdenReparacion.Completada, EstadoOrdenReparacion.Cancelada } },
            { EstadoOrdenReparacion.Completada, new() { EstadoOrdenReparacion.Facturada } },
            { EstadoOrdenReparacion.Facturada, new() { EstadoOrdenReparacion.Cancelada } },
            { EstadoOrdenReparacion.Cancelada, new() { } }
        };

        return transiciones.TryGetValue(estadoActual, out var estadosPermitidos) &&
               estadosPermitidos.Contains(estadoNuevo);
    }

    /// <summary>
    /// Actualiza horas reales trabajadas en una orden.
    /// </summary>
    public async Task<OrdenReparacion> ActualizarHorasRealesAsync(Guid ordenId, decimal horasReales)
    {
        var orden = await _context.OrdenesReparacion.FindAsync(ordenId)
            ?? throw new KeyNotFoundException($"Orden de reparación {ordenId} no encontrada.");

        if (horasReales < 0)
            throw new ArgumentException("Las horas reales no pueden ser negativas.");

        orden.HorasReales = horasReales;
        _logger.LogInformation("Horas reales actualizadas para orden {id}: {horas}", 
            ordenId, horasReales);

        await _context.SaveChangesAsync();
        return orden;
    }

    #endregion

    #region Calendario

    /// <summary>
    /// Obtiene el calendario del taller para un rango de fechas.
    /// </summary>
    public async Task<Dictionary<DateTime, List<OrdenReparacion>>> ObtenerCalendarioAsync(
        DateTime desde,
        DateTime hasta)
    {
        var ordenes = await _context.OrdenesReparacion
            .Where(o => o.FechaInicio != null && 
                        o.FechaInicio.Value.Date >= desde.Date &&
                        o.FechaInicio.Value.Date <= hasta.Date)
            .Include(o => o.TecnicoAsignado)
            .ToListAsync();

        var calendario = new Dictionary<DateTime, List<OrdenReparacion>>();
        
        foreach (var orden in ordenes)
        {
            if (orden.FechaInicio.HasValue)
            {
                var fecha = orden.FechaInicio.Value.Date;
                if (!calendario.ContainsKey(fecha))
                    calendario[fecha] = new List<OrdenReparacion>();

                calendario[fecha].Add(orden);
            }
        }

        _logger.LogInformation("Calendario generado de {desde} a {hasta}", desde, hasta);
        return calendario;
    }

    /// <summary>
    /// Obtiene las órdenes asignadas a un técnico en un rango de fechas.
    /// </summary>
    public async Task<List<OrdenReparacion>> ObtenerOrdenesTodayAsync(Guid tecnicoId)
    {
        var hoy = DateTime.UtcNow.Date;
        
        return await _context.OrdenesReparacion
            .Where(o => o.TecnicoId == tecnicoId &&
                        o.FechaInicio != null &&
                        o.FechaInicio.Value.Date == hoy &&
                        o.Estado != EstadoOrdenReparacion.Cancelada)
            .Include(o => o.Items)
            .ToListAsync();
    }

    #endregion

    #region Reportes

    /// <summary>
    /// Obtiene estadísticas del taller.
    /// </summary>
    public async Task<Dictionary<string, object>> ObtenerEstadísticasAsync()
    {
        var ordenesTotal = await _context.OrdenesReparacion.CountAsync();
        var ordenesActivas = await _context.OrdenesReparacion
            .Where(o => o.Estado == EstadoOrdenReparacion.Pendiente || 
                        o.Estado == EstadoOrdenReparacion.EnCurso)
            .CountAsync();
        
        var tecnicos = await _context.Tecnicos.CountAsync(t => t.Activo);
        
        var promedioCycleTime = await _context.OrdenesReparacion
            .Where(o => o.FechaInicio.HasValue && o.FechaFinReal.HasValue)
            .AverageAsync(o => (o.FechaFinReal.Value - o.FechaInicio.Value).TotalDays);

        return new Dictionary<string, object>
        {
            { "OrdenesTotales", ordenesTotal },
            { "OrdenesActivas", ordenesActivas },
            { "TecnicosActivos", tecnicos },
            { "PromedioCycleTimeDías", Math.Round(promedioCycleTime, 2) }
        };
    }

    /// <summary>
    /// Crea una orden de reparación con validación exhaustiva y transaccionalidad.
    /// Patrón mejorado basado en ComprasService.
    /// </summary>
    public async Task<(bool Exitoso, string? Mensaje, Guid? OrdenId)> 
        CrearOrdenReparacionMejoradaAsync(
            Guid presupuestoId,
            Guid? tecnicoId,
            decimal horasEstimadas,
            string prioridad = "Normal",
            string observaciones = "")
    {
        try
        {
            // ===== VALIDACIÓN 1: Presupuesto válido y aprobado =====
            if (presupuestoId == Guid.Empty)
                return (false, "El presupuesto es requerido", null);

            var presupuesto = await _context.Presupuestos
                .Include(p => p.Cliente)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == presupuestoId);

            if (presupuesto == null)
                return (false, "Presupuesto no encontrado", null);

            if (presupuesto.Estado != EstadoPresupuesto.Aprobado)
                return (false, $"El presupuesto debe estar aprobado. Estado actual: {presupuesto.Estado}", null);

            // ===== VALIDACIÓN 2: Técnico (si se proporciona) =====
            Tecnico? tecnico = null;
            if (tecnicoId.HasValue && tecnicoId.Value != Guid.Empty)
            {
                tecnico = await _context.Tecnicos.FindAsync(tecnicoId.Value);
                if (tecnico == null)
                    return (false, "Técnico no encontrado", null);

                if (!tecnico.Activo)
                    return (false, "El técnico asignado está inactivo", null);

                // Validar disponibilidad (opcional: puede implementarse)
                if (tecnico.OrdenesAsignadas?.Count >= 10)
                    _logger.LogWarning("Técnico {tecnico} tiene muchas órdenes asignadas", tecnico.Nombre);
            }

            // ===== VALIDACIÓN 3: Horas estimadas válidas =====
            if (horasEstimadas <= 0 || horasEstimadas > 500)
                return (false, "Las horas estimadas deben estar entre 0.5 y 500", null);

            // ===== VALIDACIÓN 4: Prioridad válida =====
            var prioridadesValidas = new[] { "Baja", "Normal", "Alta", "Urgente" };
            if (!prioridadesValidas.Contains(prioridad))
                return (false, "Prioridad inválida. Válidas: Baja, Normal, Alta, Urgente", null);

            // ===== VALIDACIÓN 5: Items del presupuesto =====
            if (!presupuesto.Items.Any())
                return (false, "El presupuesto no tiene items", null);

            _logger.LogInformation(
                "Creando orden de reparación mejorada | Presupuesto: {presupuestoId} | " +
                "Técnico: {tecnico} | Horas: {horas} | Prioridad: {prioridad}",
                presupuestoId, tecnico?.Nombre ?? "No asignado", horasEstimadas, prioridad);

            // ===== CREAR ORDEN CON TRANSACCIÓN =====
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var orden = new OrdenReparacion
                    {
                        Id = Guid.NewGuid(),
                        PresupuestoId = presupuestoId,
                        TecnicoId = tecnicoId,
                        Estado = EstadoOrdenReparacion.Pendiente,
                        HorasEstimadas = horasEstimadas,
                        Prioridad = prioridad,
                        Observaciones = observaciones,
                        FechaCreacion = DateTime.UtcNow
                    };

                    // Copiar items del presupuesto a la orden
                    foreach (var itemPresupuesto in presupuesto.Items)
                    {
                        var itemOrden = new ItemOrdenReparacion
                        {
                            Id = Guid.NewGuid(),
                            ItemPresupuestoId = itemPresupuesto.Id,
                            Codigo = itemPresupuesto.Codigo,
                            Descripcion = itemPresupuesto.Descripcion,
                            Tipo = itemPresupuesto.Tipo,
                            TiempoEstimadoHoras = (decimal)itemPresupuesto.TiempoAsignadoHoras,
                            PrecioUnitario = itemPresupuesto.PrecioUnitario,
                            Cantidad = 1
                        };
                        orden.Items.Add(itemOrden);
                    }

                    // Agregar orden a la BD
                    _context.OrdenesReparacion.Add(orden);

                    // Actualizar estado del presupuesto a "Cerrado" (en taller)
                    presupuesto.Estado = EstadoPresupuesto.Cerrado;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Orden de reparación creada exitosamente | OrdenId: {ordenId} | " +
                        "PresupuestoId: {presupuestoId} | TecnicoId: {tecnicoId}",
                        orden.Id, presupuestoId, tecnicoId);

                    return (true, null, orden.Id);
                }
                catch (Exception transEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(transEx, 
                        "Error en transacción al crear orden reparación. Rollback ejecutado");
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error al crear orden de reparación mejorada para presupuesto {presupuestoId}",
                presupuestoId);
            return (false, $"Error: {ex.Message}", null);
        }
    }

    #endregion
}
