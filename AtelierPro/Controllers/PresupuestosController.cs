using Microsoft.AspNetCore.Mvc;
using AtelierPro.Models;
using AtelierPro.Services;

namespace AtelierPro.Controllers;

/// <summary>
/// API Controller para gestionar presupuestos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PresupuestosController : ControllerBase
{
    private readonly PresupuestoRepository _repository;
    private readonly PresupuestoService _presupuestoService;
    private readonly WorkflowService _workflowService;
    private readonly ILogger<PresupuestosController> _logger;

    public PresupuestosController(
        PresupuestoRepository repository,
        PresupuestoService presupuestoService,
        WorkflowService workflowService,
        ILogger<PresupuestosController> logger)
    {
        _repository = repository;
        _presupuestoService = presupuestoService;
        _workflowService = workflowService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los presupuestos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Presupuesto>>> GetPresupuestos()
    {
        try
        {
            var presupuestos = await _repository.ObtenerTodosAsync();
            return Ok(presupuestos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener presupuestos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un presupuesto por ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Presupuesto>> GetPresupuesto(Guid id)
    {
        try
        {
            var presupuesto = await _repository.ObtenerPorIdAsync(id);
            if (presupuesto == null)
            {
                return NotFound($"Presupuesto con ID {id} no encontrado");
            }
            return Ok(presupuesto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener presupuesto {PresupuestoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene presupuestos por estado.
    /// </summary>
    [HttpGet("estado/{estado}")]
    public async Task<ActionResult<IEnumerable<Presupuesto>>> GetPresupuestosPorEstado(EstadoPresupuesto estado)
    {
        try
        {
            var presupuestos = await _repository.ObtenerPorEstadoAsync(estado);
            return Ok(presupuestos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener presupuestos por estado {Estado}", estado);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea un nuevo presupuesto.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Presupuesto>> CreatePresupuesto([FromBody] Presupuesto presupuesto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nuevoPresupuesto = await _repository.CrearAsync(presupuesto);
            return CreatedAtAction(nameof(GetPresupuesto), new { id = nuevoPresupuesto.Id }, nuevoPresupuesto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear presupuesto");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza un presupuesto existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Presupuesto>> UpdatePresupuesto(Guid id, [FromBody] Presupuesto presupuesto)
    {
        try
        {
            if (id != presupuesto.Id)
            {
                return BadRequest("El ID del presupuesto no coincide");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var presupuestoExistente = await _repository.ObtenerPorIdAsync(id);
            if (presupuestoExistente == null)
            {
                return NotFound($"Presupuesto con ID {id} no encontrado");
            }

            var presupuestoActualizado = await _repository.ActualizarAsync(presupuesto);
            return Ok(presupuestoActualizado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar presupuesto {PresupuestoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina un presupuesto.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePresupuesto(Guid id)
    {
        try
        {
            var presupuestoExistente = await _repository.ObtenerPorIdAsync(id);
            if (presupuestoExistente == null)
            {
                return NotFound($"Presupuesto con ID {id} no encontrado");
            }

            await _repository.EliminarAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar presupuesto {PresupuestoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Cambia el estado de un presupuesto.
    /// </summary>
    [HttpPost("{id}/cambiar-estado")]
    public async Task<ActionResult<Presupuesto>> CambiarEstado(Guid id, [FromBody] EstadoPresupuesto nuevoEstado)
    {
        try
        {
            var presupuesto = await _repository.ObtenerPorIdAsync(id);
            if (presupuesto == null)
            {
                return NotFound($"Presupuesto con ID {id} no encontrado");
            }

            _workflowService.CambiarEstado(presupuesto, nuevoEstado);
            var presupuestoActualizado = await _repository.ActualizarAsync(presupuesto);
            
            return Ok(presupuestoActualizado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Transición de estado inválida para presupuesto {PresupuestoId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del presupuesto {PresupuestoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
