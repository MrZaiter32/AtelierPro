using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AtelierPro.Models;
using AtelierPro.Services;

namespace AtelierPro.Controllers;

/// <summary>
/// API REST para la gestión del módulo Almacén.
/// Endpoints para refacciones, movimientos de inventario, y cuentos físicos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlmacenController : ControllerBase
{
    private readonly AlmacenService _almacenService;
    private readonly ILogger<AlmacenController> _logger;

    public AlmacenController(AlmacenService almacenService, ILogger<AlmacenController> logger)
    {
        _almacenService = almacenService ?? throw new ArgumentNullException(nameof(almacenService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Refacciones

    /// <summary>
    /// Obtiene todas las refacciones activas.
    /// GET /api/almacen/refacciones
    /// </summary>
    [HttpGet("refacciones")]
    public async Task<IActionResult> ObtenerRefacciones()
    {
        try
        {
            var refacciones = await _almacenService.ObtenerRefaccionesAsync();
            return Ok(new { success = true, data = refacciones, count = refacciones.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener refacciones");
            return StatusCode(500, new { success = false, message = "Error al obtener refacciones" });
        }
    }

    /// <summary>
    /// Obtiene una refacción por SKU.
    /// GET /api/almacen/refacciones/sku/{sku}
    /// </summary>
    [HttpGet("refacciones/sku/{sku}")]
    public async Task<IActionResult> ObtenerRefaccionPorSku(string sku)
    {
        try
        {
            var refaccion = await _almacenService.ObtenerRefaccionPorSkuAsync(sku);
            if (refaccion == null)
                return NotFound(new { success = false, message = $"Refacción {sku} no encontrada" });

            return Ok(new { success = true, data = refaccion });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener refacción por SKU {sku}", sku);
            return StatusCode(500, new { success = false, message = "Error al obtener refacción" });
        }
    }

    /// <summary>
    /// Obtiene una refacción por ID.
    /// GET /api/almacen/refacciones/{id}
    /// </summary>
    [HttpGet("refacciones/{id}")]
    public async Task<IActionResult> ObtenerRefaccion(Guid id)
    {
        try
        {
            var refaccion = await _almacenService.ObtenerRefaccionAsync(id);
            if (refaccion == null)
                return NotFound(new { success = false, message = $"Refacción {id} no encontrada" });

            return Ok(new { success = true, data = refaccion });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener refacción {id}", id);
            return StatusCode(500, new { success = false, message = "Error al obtener refacción" });
        }
    }

    /// <summary>
    /// Crea una nueva refacción.
    /// POST /api/almacen/refacciones
    /// </summary>
    [HttpPost("refacciones")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> CrearRefaccion([FromBody] Refaccion refaccion)
    {
        try
        {
            if (refaccion == null || string.IsNullOrWhiteSpace(refaccion.Sku))
                return BadRequest(new { success = false, message = "Datos de refacción inválidos" });

            var nuevaRefaccion = await _almacenService.CrearRefaccionAsync(refaccion);
            return CreatedAtAction(nameof(ObtenerRefaccion), new { id = nuevaRefaccion.Id },
                new { success = true, data = nuevaRefaccion });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear refacción");
            return StatusCode(500, new { success = false, message = "Error al crear refacción" });
        }
    }

    /// <summary>
    /// Actualiza una refacción.
    /// PUT /api/almacen/refacciones/{id}
    /// </summary>
    [HttpPut("refacciones/{id}")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> ActualizarRefaccion(Guid id, [FromBody] Refaccion refaccion)
    {
        try
        {
            var refaccionActualizada = await _almacenService.ActualizarRefaccionAsync(id, refaccion);
            return Ok(new { success = true, data = refaccionActualizada });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar refacción {id}", id);
            return StatusCode(500, new { success = false, message = "Error al actualizar refacción" });
        }
    }

    /// <summary>
    /// Desactiva una refacción.
    /// DELETE /api/almacen/refacciones/{id}
    /// </summary>
    [HttpDelete("refacciones/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DesactivarRefaccion(Guid id)
    {
        try
        {
            await _almacenService.DesactivarRefaccionAsync(id);
            return Ok(new { success = true, message = "Refacción desactivada" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar refacción {id}", id);
            return StatusCode(500, new { success = false, message = "Error al desactivar refacción" });
        }
    }

    #endregion

    #region Movimientos de Inventario

    /// <summary>
    /// Registra un movimiento de inventario.
    /// POST /api/almacen/movimientos
    /// Body: { refaccionId, tipo, cantidad, razon, responsableUsuarioId }
    /// </summary>
    [HttpPost("movimientos")]
    [Authorize(Roles = "Admin,Finanzas,Taller")]
    public async Task<IActionResult> RegistrarMovimiento([FromBody] RegistrarMovimientoRequest request)
    {
        try
        {
            if (request?.RefaccionId == Guid.Empty || request.Cantidad <= 0)
                return BadRequest(new { success = false, message = "Datos de movimiento inválidos" });

            var movimiento = await _almacenService.RegistrarMovimientoAsync(
                request.RefaccionId,
                request.Tipo,
                request.Cantidad,
                request.Razon,
                request.ResponsableUsuarioId,
                request.OrdenCompraId,
                request.OrdenReparacionId);

            return CreatedAtAction(nameof(ObtenerHistorialMovimientos), 
                new { refaccionId = request.RefaccionId },
                new { success = true, data = movimiento });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar movimiento");
            return StatusCode(500, new { success = false, message = "Error al registrar movimiento" });
        }
    }

    /// <summary>
    /// Obtiene el historial de movimientos de una refacción.
    /// GET /api/almacen/movimientos/{refaccionId}?desde={fecha}&hasta={fecha}
    /// </summary>
    [HttpGet("movimientos/{refaccionId}")]
    public async Task<IActionResult> ObtenerHistorialMovimientos(
        Guid refaccionId,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        try
        {
            var movimientos = await _almacenService.ObtenerHistorialMovimientosAsync(refaccionId, desde, hasta);
            return Ok(new { success = true, data = movimientos, count = movimientos.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de movimientos");
            return StatusCode(500, new { success = false, message = "Error al obtener historial" });
        }
    }

    #endregion

    #region Cuentos Físicos

    /// <summary>
    /// Crea un nuevo cuento físico.
    /// POST /api/almacen/cuentos
    /// Body: { responsableUsuarioId, observaciones }
    /// </summary>
    [HttpPost("cuentos")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> CrearCuentoFisico([FromBody] CrearCuentoFisicoRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.ResponsableUsuarioId))
                return BadRequest(new { success = false, message = "ResponsableUsuarioId es requerido" });

            var cuento = await _almacenService.CrearCuentoFisicoAsync(
                request.ResponsableUsuarioId,
                request.Observaciones ?? "");

            return CreatedAtAction(nameof(ObtenerCuentosFisicos), null,
                new { success = true, data = cuento });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cuento físico");
            return StatusCode(500, new { success = false, message = "Error al crear cuento físico" });
        }
    }

    /// <summary>
    /// Agrega un detalle a un cuento físico.
    /// POST /api/almacen/cuentos/{cuentoId}/detalles
    /// Body: { refaccionId, stockFisico, observaciones }
    /// </summary>
    [HttpPost("cuentos/{cuentoId}/detalles")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> AgregarDetalleCuento(Guid cuentoId, [FromBody] AgregarDetalleRequest request)
    {
        try
        {
            if (request?.RefaccionId == Guid.Empty || request.StockFisico < 0)
                return BadRequest(new { success = false, message = "Datos de detalle inválidos" });

            var detalle = await _almacenService.AgregarDetalleAsync(
                cuentoId,
                request.RefaccionId,
                request.StockFisico,
                request.Observaciones ?? "");

            return CreatedAtAction(nameof(ObtenerCuentosFisicos), null,
                new { success = true, data = detalle });
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
            _logger.LogError(ex, "Error al agregar detalle");
            return StatusCode(500, new { success = false, message = "Error al agregar detalle" });
        }
    }

    /// <summary>
    /// Completa un cuento físico y aplica ajustes.
    /// PATCH /api/almacen/cuentos/{cuentoId}/completar
    /// Body: { responsableUsuarioId }
    /// </summary>
    [HttpPatch("cuentos/{cuentoId}/completar")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> CompletarCuentoFisico(Guid cuentoId, [FromBody] CompletarCuentoRequest request)
    {
        try
        {
            var cuento = await _almacenService.CompletarCuentoFisicoAsync(cuentoId, request.ResponsableUsuarioId);
            return Ok(new { success = true, data = cuento, message = "Cuento físico completado" });
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
            _logger.LogError(ex, "Error al completar cuento físico");
            return StatusCode(500, new { success = false, message = "Error al completar cuento físico" });
        }
    }

    /// <summary>
    /// Obtiene cuentos físicos.
    /// GET /api/almacen/cuentos?estado=Completado
    /// </summary>
    [HttpGet("cuentos")]
    public async Task<IActionResult> ObtenerCuentosFisicos([FromQuery] string? estado = null)
    {
        try
        {
            var cuentos = await _almacenService.ObtenerCuentosFisicosAsync(estado);
            return Ok(new { success = true, data = cuentos, count = cuentos.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentos físicos");
            return StatusCode(500, new { success = false, message = "Error al obtener cuentos físicos" });
        }
    }

    #endregion

    #region Alertas y Análisis

    /// <summary>
    /// Obtiene refacciones con stock bajo.
    /// GET /api/almacen/stock-bajo
    /// </summary>
    [HttpGet("stock-bajo")]
    public async Task<IActionResult> ObtenerRefaccionesStockBajo()
    {
        try
        {
            var refacciones = await _almacenService.ObtenerRefaccionesStockBajoAsync();
            return Ok(new { success = true, data = refacciones, count = refacciones.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener refacciones con stock bajo");
            return StatusCode(500, new { success = false, message = "Error al obtener alertas" });
        }
    }

    /// <summary>
    /// Obtiene refacciones con stock crítico.
    /// GET /api/almacen/stock-critico
    /// </summary>
    [HttpGet("stock-critico")]
    public async Task<IActionResult> ObtenerRefaccionesCriticas()
    {
        try
        {
            var refacciones = await _almacenService.ObtenerRefaccionesCriticasAsync();
            return Ok(new { success = true, data = refacciones, count = refacciones.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener refacciones críticas");
            return StatusCode(500, new { success = false, message = "Error al obtener alertas críticas" });
        }
    }

    /// <summary>
    /// Obtiene estadísticas del almacén.
    /// GET /api/almacen/estadisticas
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<IActionResult> ObtenerEstadísticas()
    {
        try
        {
            var stats = await _almacenService.ObtenerEstadísticasAsync();
            return Ok(new { success = true, data = stats });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas");
            return StatusCode(500, new { success = false, message = "Error al obtener estadísticas" });
        }
    }

    /// <summary>
    /// Verifica disponibilidad de una refacción.
    /// GET /api/almacen/disponibilidad/{refaccionId}?cantidad=5
    /// </summary>
    [HttpGet("disponibilidad/{refaccionId}")]
    public async Task<IActionResult> VerificarDisponibilidad(Guid refaccionId, [FromQuery] int cantidad = 1)
    {
        try
        {
            var disponible = await _almacenService.VerificarDisponibilidadAsync(refaccionId, cantidad);
            return Ok(new { success = true, data = new { disponible, refaccionId, cantidadSolicitada = cantidad } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad");
            return StatusCode(500, new { success = false, message = "Error al verificar disponibilidad" });
        }
    }

    #endregion
}

#region DTOs

public class RegistrarMovimientoRequest
{
    public Guid RefaccionId { get; set; }
    public TipoMovimientoInventario Tipo { get; set; }
    public int Cantidad { get; set; }
    public string Razon { get; set; } = string.Empty;
    public string ResponsableUsuarioId { get; set; } = string.Empty;
    public Guid? OrdenCompraId { get; set; }
    public Guid? OrdenReparacionId { get; set; }
}

public class CrearCuentoFisicoRequest
{
    public string ResponsableUsuarioId { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

public class AgregarDetalleRequest
{
    public Guid RefaccionId { get; set; }
    public int StockFisico { get; set; }
    public string? Observaciones { get; set; }
}

public class CompletarCuentoRequest
{
    public string ResponsableUsuarioId { get; set; } = string.Empty;
}

#endregion
