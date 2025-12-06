using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AtelierPro.Models;
using AtelierPro.Services;

namespace AtelierPro.Controllers;

/// <summary>
/// API REST para la gestión del módulo Taller.
/// Endpoints para técnicos, órdenes de reparación, y calendario.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TallerController : ControllerBase
{
    private readonly TallerService _tallerService;
    private readonly ILogger<TallerController> _logger;

    public TallerController(TallerService tallerService, ILogger<TallerController> logger)
    {
        _tallerService = tallerService ?? throw new ArgumentNullException(nameof(tallerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Técnicos

    /// <summary>
    /// Obtiene todos los técnicos activos.
    /// GET /api/taller/tecnicos
    /// </summary>
    [HttpGet("tecnicos")]
    public async Task<IActionResult> ObtenerTecnicos()
    {
        try
        {
            var tecnicos = await _tallerService.ObtenerTecnicosAsync();
            return Ok(new { success = true, data = tecnicos, count = tecnicos.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener técnicos");
            return StatusCode(500, new { success = false, message = "Error al obtener técnicos" });
        }
    }

    /// <summary>
    /// Obtiene un técnico por ID.
    /// GET /api/taller/tecnicos/{id}
    /// </summary>
    [HttpGet("tecnicos/{id}")]
    public async Task<IActionResult> ObtenerTecnico(Guid id)
    {
        try
        {
            var tecnico = await _tallerService.ObtenerTecnicoPorIdAsync(id);
            if (tecnico == null)
                return NotFound(new { success = false, message = $"Técnico {id} no encontrado" });

            return Ok(new { success = true, data = tecnico });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener técnico {id}", id);
            return StatusCode(500, new { success = false, message = "Error al obtener técnico" });
        }
    }

    /// <summary>
    /// Crea un nuevo técnico.
    /// POST /api/taller/tecnicos
    /// </summary>
    [HttpPost("tecnicos")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> CrearTecnico([FromBody] Tecnico tecnico)
    {
        try
        {
            if (tecnico == null)
                return BadRequest(new { success = false, message = "Datos del técnico inválidos" });

            var nuevoTecnico = await _tallerService.CrearTecnicoAsync(tecnico);
            return CreatedAtAction(nameof(ObtenerTecnico), new { id = nuevoTecnico.Id }, 
                new { success = true, data = nuevoTecnico });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear técnico");
            return StatusCode(500, new { success = false, message = "Error al crear técnico" });
        }
    }

    /// <summary>
    /// Actualiza un técnico existente.
    /// PUT /api/taller/tecnicos/{id}
    /// </summary>
    [HttpPut("tecnicos/{id}")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> ActualizarTecnico(Guid id, [FromBody] Tecnico tecnico)
    {
        try
        {
            var tecnicoActualizado = await _tallerService.ActualizarTecnicoAsync(id, tecnico);
            return Ok(new { success = true, data = tecnicoActualizado });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar técnico {id}", id);
            return StatusCode(500, new { success = false, message = "Error al actualizar técnico" });
        }
    }

    /// <summary>
    /// Desactiva un técnico.
    /// DELETE /api/taller/tecnicos/{id}
    /// </summary>
    [HttpDelete("tecnicos/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DesactivarTecnico(Guid id)
    {
        try
        {
            await _tallerService.DesactivarTecnicoAsync(id);
            return Ok(new { success = true, message = "Técnico desactivado" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar técnico {id}", id);
            return StatusCode(500, new { success = false, message = "Error al desactivar técnico" });
        }
    }

    #endregion

    #region Órdenes de Reparación

    /// <summary>
    /// Obtiene órdenes de reparación con filtros opcionales.
    /// GET /api/taller/ordenes?estado=Pendiente&tecnicoId={id}&desde={fecha}&hasta={fecha}
    /// </summary>
    [HttpGet("ordenes")]
    public async Task<IActionResult> ObtenerOrdenes(
        [FromQuery] EstadoOrdenReparacion? estado = null,
        [FromQuery] Guid? tecnicoId = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        try
        {
            var ordenes = await _tallerService.ObtenerOrdenesReparacionAsync(estado, tecnicoId, desde, hasta);
            return Ok(new { success = true, data = ordenes, count = ordenes.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes");
            return StatusCode(500, new { success = false, message = "Error al obtener órdenes" });
        }
    }

    /// <summary>
    /// Obtiene órdenes activas (Pendiente o EnCurso).
    /// GET /api/taller/ordenes/activas
    /// </summary>
    [HttpGet("ordenes/activas")]
    public async Task<IActionResult> ObtenerOrdenesActivas()
    {
        try
        {
            var ordenes = await _tallerService.ObtenerOrdenesActivasAsync();
            return Ok(new { success = true, data = ordenes, count = ordenes.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes activas");
            return StatusCode(500, new { success = false, message = "Error al obtener órdenes activas" });
        }
    }

    /// <summary>
    /// Obtiene una orden de reparación por ID.
    /// GET /api/taller/ordenes/{id}
    /// </summary>
    [HttpGet("ordenes/{id}")]
    public async Task<IActionResult> ObtenerOrden(Guid id)
    {
        try
        {
            var orden = await _tallerService.ObtenerOrdenReparacionAsync(id);
            if (orden == null)
                return NotFound(new { success = false, message = $"Orden {id} no encontrada" });

            return Ok(new { success = true, data = orden });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener orden {id}", id);
            return StatusCode(500, new { success = false, message = "Error al obtener orden" });
        }
    }

    /// <summary>
    /// Crea una nueva orden de reparación.
    /// POST /api/taller/ordenes
    /// Body: { presupuestoId, horasEstimadas, prioridad }
    /// </summary>
    [HttpPost("ordenes")]
    [Authorize(Roles = "Admin,Finanzas,Taller")]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenRequest request)
    {
        try
        {
            if (request?.PresupuestoId == Guid.Empty)
                return BadRequest(new { success = false, message = "PresupuestoId es requerido" });

            var orden = await _tallerService.CrearOrdenReparacionAsync(
                request.PresupuestoId,
                request.HorasEstimadas,
                request.Prioridad ?? "Normal");

            return CreatedAtAction(nameof(ObtenerOrden), new { id = orden.Id },
                new { success = true, data = orden });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear orden");
            return StatusCode(500, new { success = false, message = "Error al crear orden" });
        }
    }

    /// <summary>
    /// Asigna un técnico a una orden.
    /// POST /api/taller/ordenes/{id}/asignar/{tecnicoId}
    /// </summary>
    [HttpPost("ordenes/{id}/asignar/{tecnicoId}")]
    [Authorize(Roles = "Admin,Finanzas,Taller")]
    public async Task<IActionResult> AsignarTecnico(Guid id, Guid tecnicoId)
    {
        try
        {
            var orden = await _tallerService.AsignarTecnicoAsync(id, tecnicoId);
            return Ok(new { success = true, data = orden, message = "Técnico asignado" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar técnico");
            return StatusCode(500, new { success = false, message = "Error al asignar técnico" });
        }
    }

    /// <summary>
    /// Cambia el estado de una orden.
    /// PATCH /api/taller/ordenes/{id}/estado
    /// Body: { estado: "EnCurso" | "Completada" | "Cancelada" }
    /// </summary>
    [HttpPatch("ordenes/{id}/estado")]
    [Authorize(Roles = "Admin,Finanzas,Taller")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoRequest request)
    {
        try
        {
            var orden = await _tallerService.CambiarEstadoAsync(id, request.Estado);
            return Ok(new { success = true, data = orden, message = $"Estado cambiado a {request.Estado}" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado");
            return StatusCode(500, new { success = false, message = "Error al cambiar estado" });
        }
    }

    /// <summary>
    /// Actualiza las horas reales de una orden.
    /// PATCH /api/taller/ordenes/{id}/horas
    /// Body: { horasReales: 5.5 }
    /// </summary>
    [HttpPatch("ordenes/{id}/horas")]
    [Authorize(Roles = "Admin,Finanzas,Taller")]
    public async Task<IActionResult> ActualizarHoras(Guid id, [FromBody] ActualizarHorasRequest request)
    {
        try
        {
            var orden = await _tallerService.ActualizarHorasRealesAsync(id, request.HorasReales);
            return Ok(new { success = true, data = orden, message = "Horas actualizadas" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar horas");
            return StatusCode(500, new { success = false, message = "Error al actualizar horas" });
        }
    }

    #endregion

    #region Calendario y Reportes

    /// <summary>
    /// Obtiene el calendario del taller para un rango de fechas.
    /// GET /api/taller/calendario?desde={fecha}&hasta={fecha}
    /// </summary>
    [HttpGet("calendario")]
    public async Task<IActionResult> ObtenerCalendario(
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        try
        {
            desde ??= DateTime.UtcNow.AddDays(-7);
            hasta ??= DateTime.UtcNow.AddDays(30);

            var calendario = await _tallerService.ObtenerCalendarioAsync(desde.Value, hasta.Value);
            return Ok(new { success = true, data = calendario, desde, hasta });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener calendario");
            return StatusCode(500, new { success = false, message = "Error al obtener calendario" });
        }
    }

    /// <summary>
    /// Obtiene estadísticas del taller.
    /// GET /api/taller/estadisticas
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<IActionResult> ObtenerEstadísticas()
    {
        try
        {
            var stats = await _tallerService.ObtenerEstadísticasAsync();
            return Ok(new { success = true, data = stats });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas");
            return StatusCode(500, new { success = false, message = "Error al obtener estadísticas" });
        }
    }

    #endregion
}

#region DTOs

public class CrearOrdenRequest
{
    public Guid PresupuestoId { get; set; }
    public decimal HorasEstimadas { get; set; }
    public string? Prioridad { get; set; }
}

public class CambiarEstadoRequest
{
    public EstadoOrdenReparacion Estado { get; set; }
}

public class ActualizarHorasRequest
{
    public decimal HorasReales { get; set; }
}

#endregion
