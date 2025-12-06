using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AtelierPro.Models;
using AtelierPro.Services;

namespace AtelierPro.Controllers;

/// <summary>
/// API REST para la gestión del módulo Compras.
/// Endpoints para proveedores, órdenes de compra, y requisiciones.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComprasController : ControllerBase
{
    private readonly ComprasService _comprasService;
    private readonly ILogger<ComprasController> _logger;

    public ComprasController(ComprasService comprasService, ILogger<ComprasController> logger)
    {
        _comprasService = comprasService ?? throw new ArgumentNullException(nameof(comprasService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Proveedores

    /// <summary>
    /// Obtiene todos los proveedores activos.
    /// GET /api/compras/proveedores
    /// </summary>
    [HttpGet("proveedores")]
    public async Task<IActionResult> ObtenerProveedores()
    {
        try
        {
            var proveedores = await _comprasService.ObtenerProveedoresAsync();
            return Ok(new { success = true, data = proveedores, count = proveedores.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedores");
            return StatusCode(500, new { success = false, message = "Error al obtener proveedores" });
        }
    }

    /// <summary>
    /// Obtiene un proveedor por ID.
    /// GET /api/compras/proveedores/{id}
    /// </summary>
    [HttpGet("proveedores/{id}")]
    public async Task<IActionResult> ObtenerProveedor(Guid id)
    {
        try
        {
            var proveedor = await _comprasService.ObtenerProveedorAsync(id);
            if (proveedor == null)
                return NotFound(new { success = false, message = $"Proveedor {id} no encontrado" });

            return Ok(new { success = true, data = proveedor });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedor {id}", id);
            return StatusCode(500, new { success = false, message = "Error al obtener proveedor" });
        }
    }

    /// <summary>
    /// Crea un nuevo proveedor.
    /// POST /api/compras/proveedores
    /// </summary>
    [HttpPost("proveedores")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> CrearProveedor([FromBody] Proveedor proveedor)
    {
        try
        {
            if (proveedor == null || string.IsNullOrWhiteSpace(proveedor.RazonSocial))
                return BadRequest(new { success = false, message = "Datos del proveedor inválidos" });

            var nuevoProveedor = await _comprasService.CrearProveedorAsync(proveedor);
            return CreatedAtAction(nameof(ObtenerProveedor), new { id = nuevoProveedor.Id },
                new { success = true, data = nuevoProveedor });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear proveedor");
            return StatusCode(500, new { success = false, message = "Error al crear proveedor" });
        }
    }

    /// <summary>
    /// Actualiza un proveedor.
    /// PUT /api/compras/proveedores/{id}
    /// </summary>
    [HttpPut("proveedores/{id}")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> ActualizarProveedor(Guid id, [FromBody] Proveedor proveedor)
    {
        try
        {
            var proveedorActualizado = await _comprasService.ActualizarProveedorAsync(id, proveedor);
            return Ok(new { success = true, data = proveedorActualizado });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar proveedor {id}", id);
            return StatusCode(500, new { success = false, message = "Error al actualizar proveedor" });
        }
    }

    /// <summary>
    /// Desactiva un proveedor.
    /// DELETE /api/compras/proveedores/{id}
    /// </summary>
    [HttpDelete("proveedores/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DesactivarProveedor(Guid id)
    {
        try
        {
            var proveedor = await _comprasService.ObtenerProveedorAsync(id);
            if (proveedor == null)
                return NotFound(new { success = false, message = $"Proveedor {id} no encontrado" });

            proveedor.Activo = false;
            var actualizado = await _comprasService.ActualizarProveedorAsync(id, proveedor);
            return Ok(new { success = true, message = "Proveedor desactivado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar proveedor {id}", id);
            return StatusCode(500, new { success = false, message = "Error al desactivar proveedor" });
        }
    }

    #endregion

    #region Órdenes de Compra

    /// <summary>
    /// Obtiene órdenes de compra con filtros opcionales.
    /// GET /api/compras/ordenes?estado=Generada&desde={fecha}&hasta={fecha}
    /// </summary>
    [HttpGet("ordenes")]
    public async Task<IActionResult> ObtenerOrdenes(
        [FromQuery] string? estado = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        try
        {
            // Parsear estado si se proporciona
            EstadoOrdenCompra? estadoParsed = null;
            if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoOrdenCompra>(estado, true, out var est))
                estadoParsed = est;

            var ordenes = await _comprasService.ObtenerOrdenesCompraAsync(estadoParsed, null, desde, hasta);
            return Ok(new { success = true, data = ordenes, count = ordenes.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes de compra");
            return StatusCode(500, new { success = false, message = "Error al obtener órdenes" });
        }
    }

    /// <summary>
    /// Obtiene una orden de compra por ID.
    /// GET /api/compras/ordenes/{id}
    /// </summary>
    [HttpGet("ordenes/{id}")]
    public async Task<IActionResult> ObtenerOrden(Guid id)
    {
        try
        {
            var orden = await _comprasService.ObtenerOrdenCompraAsync(id);
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
    /// Crea una nueva orden de compra.
    /// POST /api/compras/ordenes
    /// Body: { proveedorId, items: [{refaccionId, cantidad, precioUnitario}] }
    /// </summary>
    [HttpPost("ordenes")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenCompraRequest request)
    {
        try
        {
            if (request?.ProveedorId == Guid.Empty || request.Items == null || !request.Items.Any())
                return BadRequest(new { success = false, message = "Datos de orden inválidos" });

            // Convertir DTOs a tuplas
            var items = request.Items
                .Select(i => (i.RefaccionId, i.Cantidad, i.PrecioUnitario))
                .ToList();

            var orden = await _comprasService.CrearOrdenCompraAsync(
                request.ProveedorId,
                items,
                request.Observaciones ?? "");

            return CreatedAtAction(nameof(ObtenerOrden), new { id = orden.Id },
                new { success = true, data = orden });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear orden");
            return StatusCode(500, new { success = false, message = "Error al crear orden" });
        }
    }

    /// <summary>
    /// Envía una orden de compra.
    /// PATCH /api/compras/ordenes/{id}/enviar
    /// </summary>
    [HttpPatch("ordenes/{id}/enviar")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> EnviarOrden(Guid id)
    {
        try
        {
            var orden = await _comprasService.EnviarOrdenCompraAsync(id);
            return Ok(new { success = true, data = orden, message = "Orden enviada" });
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
            _logger.LogError(ex, "Error al enviar orden");
            return StatusCode(500, new { success = false, message = "Error al enviar orden" });
        }
    }

    /// <summary>
    /// Confirma la recepción de una orden de compra.
    /// PATCH /api/compras/ordenes/{id}/recibir
    /// Body: { detallesRecepcion: [{itemId, cantidadRecibida}] }
    /// </summary>
    [HttpPatch("ordenes/{id}/recibir")]
    [Authorize(Roles = "Admin,Finanzas,Taller")]
    public async Task<IActionResult> ConfirmarRecepcion(Guid id, [FromBody] ConfirmarRecepcionRequest request)
    {
        try
        {
            // Convertir a diccionario esperado por el servicio
            var cantidadesRecibidas = request?.DetallesRecepcion?
                .ToDictionary(d => d.ItemId, d => d.CantidadRecibida)
                ?? new Dictionary<Guid, int>();

            var orden = await _comprasService.ConfirmarRecepcionAsync(id, cantidadesRecibidas);
            return Ok(new { success = true, data = orden, message = "Recepción confirmada" });
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
            _logger.LogError(ex, "Error al confirmar recepción");
            return StatusCode(500, new { success = false, message = "Error al confirmar recepción" });
        }
    }



    #endregion

    #region Requisiciones

    /// <summary>
    /// Obtiene requisiciones.
    /// GET /api/compras/requisiciones?estado=Pendiente
    /// </summary>
    [HttpGet("requisiciones")]
    public async Task<IActionResult> ObtenerRequisiciones([FromQuery] string? estado = null)
    {
        try
        {
            var requisiciones = await _comprasService.ObtenerRequisicionesAsync(estado);
            return Ok(new { success = true, data = requisiciones, count = requisiciones.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener requisiciones");
            return StatusCode(500, new { success = false, message = "Error al obtener requisiciones" });
        }
    }

    /// <summary>
    /// Crea una nueva requisición.
    /// POST /api/compras/requisiciones
    /// Body: { solicitanteUsuarioId, items: [{sku, descripcion, cantidad}], justificacion }
    /// </summary>
    [HttpPost("requisiciones")]
    [Authorize(Roles = "Admin,Finanzas,Taller")]
    public async Task<IActionResult> CrearRequisicion([FromBody] CrearRequisicionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.SolicitanteUsuarioId) || request.Items == null || !request.Items.Any())
                return BadRequest(new { success = false, message = "Datos de requisición inválidos" });

            // Convertir DTOs a tuplas para el servicio
            var items = request.Items
                .Select(i => (i.Sku ?? "", i.Descripcion ?? "", i.Cantidad, i.PrecioEstimado))
                .ToList();

            var requisicion = await _comprasService.CrearRequisicionAsync(
                request.SolicitanteUsuarioId,
                items,
                request.Justificacion ?? "");

            return CreatedAtAction(nameof(ObtenerRequisiciones), null,
                new { success = true, data = requisicion });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear requisición");
            return StatusCode(500, new { success = false, message = "Error al crear requisición" });
        }
    }

    /// <summary>
    /// Aprueba una requisición.
    /// PATCH /api/compras/requisiciones/{id}/aprobar
    /// Body: { aprobadorUsuarioId }
    /// </summary>
    [HttpPatch("requisiciones/{id}/aprobar")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> AprobarRequisicion(Guid id, [FromBody] AprobarRequisicionRequest request)
    {
        try
        {
            var requisicion = await _comprasService.AprobarRequisicionAsync(id, request.AprobadorUsuarioId);
            return Ok(new { success = true, data = requisicion, message = "Requisición aprobada" });
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
            _logger.LogError(ex, "Error al aprobar requisición");
            return StatusCode(500, new { success = false, message = "Error al aprobar requisición" });
        }
    }

    /// <summary>
    /// Rechaza una requisición.
    /// PATCH /api/compras/requisiciones/{id}/rechazar
    /// Body: { observaciones }
    /// </summary>
    [HttpPatch("requisiciones/{id}/rechazar")]
    [Authorize(Roles = "Admin,Finanzas")]
    public async Task<IActionResult> RechazarRequisicion(Guid id, [FromBody] RechazarRequisicionRequest request)
    {
        try
        {
            var requisicion = await _comprasService.RechazarRequisicionAsync(id, request.Observaciones ?? "");
            return Ok(new { success = true, data = requisicion, message = "Requisición rechazada" });
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
            _logger.LogError(ex, "Error al rechazar requisición");
            return StatusCode(500, new { success = false, message = "Error al rechazar requisición" });
        }
    }

    #endregion

    #region Reportes

    /// <summary>
    /// Obtiene estadísticas de compras.
    /// GET /api/compras/estadisticas
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<IActionResult> ObtenerEstadísticas()
    {
        try
        {
            var stats = await _comprasService.ObtenerEstadísticasAsync();
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

public class CrearOrdenCompraRequest
{
    public Guid ProveedorId { get; set; }
    public List<ItemOrdenCompraRequest>? Items { get; set; }
    public string? Observaciones { get; set; }
}

public class ItemOrdenCompraRequest
{
    public Guid RefaccionId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class ConfirmarRecepcionRequest
{
    public List<DetalleRecepcionRequest>? DetallesRecepcion { get; set; }
}

public class DetalleRecepcionRequest
{
    public Guid ItemId { get; set; }
    public int CantidadRecibida { get; set; }
}

public class CrearRequisicionRequest
{
    public string? SolicitanteUsuarioId { get; set; }
    public List<ItemRequisicionRequest>? Items { get; set; }
    public string? Justificacion { get; set; }
}

public class ItemRequisicionRequest
{
    public string? Sku { get; set; }
    public string? Descripcion { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioEstimado { get; set; }
}

public class AprobarRequisicionRequest
{
    public string AprobadorUsuarioId { get; set; } = string.Empty;
}

public class RechazarRequisicionRequest
{
    public string Observaciones { get; set; } = string.Empty;
}

#endregion
