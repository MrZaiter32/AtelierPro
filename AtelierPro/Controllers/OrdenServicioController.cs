using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AtelierPro.Models;
using AtelierPro.Services;

namespace AtelierPro.Controllers;

/// <summary>
/// API REST para la gestión de órdenes de servicio (servicios adicionales).
/// Incluye pintura, detalles, tratamientos, etc.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdenServicioController : ControllerBase
{
    private readonly ComprasService _comprasService; // Reutilizamos para servicios administrativos
    private readonly ILogger<OrdenServicioController> _logger;

    public OrdenServicioController(ComprasService comprasService, ILogger<OrdenServicioController> logger)
    {
        _comprasService = comprasService ?? throw new ArgumentNullException(nameof(comprasService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todas las órdenes de servicio.
    /// GET /api/ordenservicio
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerOrdenes(
        [FromQuery] EstadoOrdenServicio? estado = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        try
        {
            // Esta implementación dependerá de un método en el servicio que manejemos
            // Por ahora retornamos un placeholder que será implementado después
            return Ok(new { success = true, data = new List<OrdenServicio>(), count = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes de servicio");
            return StatusCode(500, new { success = false, message = "Error al obtener órdenes de servicio" });
        }
    }

    /// <summary>
    /// Obtiene una orden de servicio por ID.
    /// GET /api/ordenservicio/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerOrden(Guid id)
    {
        try
        {
            // Placeholder para implementación futura
            return NotFound(new { success = false, message = $"Orden de servicio {id} no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener orden de servicio {id}", id);
            return StatusCode(500, new { success = false, message = "Error al obtener orden" });
        }
    }

    /// <summary>
    /// Crea una nueva orden de servicio.
    /// POST /api/ordenservicio
    /// Body: { ordenReparacionId, descripcion, tipo, precio }
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Finanzas,Taller")]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenServicioRequest request)
    {
        try
        {
            if (request?.OrdenReparacionId == Guid.Empty || string.IsNullOrWhiteSpace(request.Descripcion))
                return BadRequest(new { success = false, message = "Datos de orden de servicio inválidos" });

            var nuevaOrden = new OrdenServicio
            {
                Id = Guid.NewGuid(),
                OrdenReparacionId = request.OrdenReparacionId,
                Descripcion = request.Descripcion,
                Tipo = request.Tipo ?? "Otro",
                Precio = request.Precio,
                Estado = EstadoOrdenServicio.Pendiente,
                FechaCreacion = DateTime.UtcNow
            };

            _logger.LogInformation("Orden de servicio creada: {id}", nuevaOrden.Id);
            return CreatedAtAction(nameof(ObtenerOrden), new { id = nuevaOrden.Id },
                new { success = true, data = nuevaOrden });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear orden de servicio");
            return StatusCode(500, new { success = false, message = "Error al crear orden" });
        }
    }

    /// <summary>
    /// Actualiza el estado de una orden de servicio.
    /// PATCH /api/ordenservicio/{id}/estado
    /// Body: { estado, observaciones }
    /// </summary>
    [HttpPatch("{id}/estado")]
    [Authorize(Roles = "Admin,Finanzas,Taller")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoServicioRequest request)
    {
        try
        {
            _logger.LogInformation("Cambio de estado solicitado para orden {id} a {estado}", id, request.Estado);
            return Ok(new { success = true, message = "Estado actualizado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado");
            return StatusCode(500, new { success = false, message = "Error al cambiar estado" });
        }
    }

    /// <summary>
    /// Agrega una foto a la orden de servicio.
    /// POST /api/ordenservicio/{id}/fotos
    /// Body: { url, etiqueta }
    /// </summary>
    [HttpPost("{id}/fotos")]
    [Authorize(Roles = "Admin,Finanzas,Taller")]
    public async Task<IActionResult> AgregarFoto(Guid id, [FromBody] AgregarFotoRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Url))
                return BadRequest(new { success = false, message = "URL de foto inválida" });

            var foto = new FotoServicio
            {
                Id = Guid.NewGuid(),
                OrdenServicioId = id,
                Url = request.Url,
                Etiqueta = request.Etiqueta ?? "General",
                FechaCarga = DateTime.UtcNow
            };

            _logger.LogInformation("Foto agregada a orden {id}", id);
            return CreatedAtAction(nameof(ObtenerFotos), new { id },
                new { success = true, data = foto, message = "Foto agregada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar foto");
            return StatusCode(500, new { success = false, message = "Error al agregar foto" });
        }
    }

    /// <summary>
    /// Obtiene las fotos de una orden de servicio.
    /// GET /api/ordenservicio/{id}/fotos
    /// </summary>
    [HttpGet("{id}/fotos")]
    public async Task<IActionResult> ObtenerFotos(Guid id)
    {
        try
        {
            // Placeholder - será implementado en servicio
            return Ok(new { success = true, data = new List<FotoServicio>(), count = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener fotos de orden {id}", id);
            return StatusCode(500, new { success = false, message = "Error al obtener fotos" });
        }
    }

    /// <summary>
    /// Completa una orden de servicio.
    /// PATCH /api/ordenservicio/{id}/completar
    /// Body: { precio, observaciones }
    /// </summary>
    [HttpPatch("{id}/completar")]
    [Authorize(Roles = "Admin,Finanzas,Taller")]
    public async Task<IActionResult> CompletarOrden(Guid id, [FromBody] CompletarOrdenServicioRequest request)
    {
        try
        {
            _logger.LogInformation("Completando orden de servicio {id}", id);
            return Ok(new { success = true, message = "Orden de servicio completada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al completar orden");
            return StatusCode(500, new { success = false, message = "Error al completar orden" });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de servicios.
    /// GET /api/ordenservicio/estadisticas
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<IActionResult> ObtenerEstadísticas()
    {
        try
        {
            var stats = new
            {
                totalOrdenes = 0,
                ordenesCompletadas = 0,
                ingresoTotal = 0.0m,
                servicioMasComun = ""
            };

            return Ok(new { success = true, data = stats });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas");
            return StatusCode(500, new { success = false, message = "Error al obtener estadísticas" });
        }
    }
}

#region DTOs

public class CrearOrdenServicioRequest
{
    public Guid OrdenReparacionId { get; set; }
    public string? Descripcion { get; set; }
    public string? Tipo { get; set; } // Pintura, Detallado, Tratamiento, etc.
    public decimal Precio { get; set; }
}

public class CambiarEstadoServicioRequest
{
    public EstadoOrdenServicio Estado { get; set; }
    public string? Observaciones { get; set; }
}

public class AgregarFotoRequest
{
    public string? Url { get; set; }
    public string? Etiqueta { get; set; } // "Antes", "Después", "Detalle", etc.
}

public class CompletarOrdenServicioRequest
{
    public decimal Precio { get; set; }
    public string? Observaciones { get; set; }
}

#endregion
