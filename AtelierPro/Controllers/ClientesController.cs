using Microsoft.AspNetCore.Mvc;
using AtelierPro.Models;
using AtelierPro.Services;

namespace AtelierPro.Controllers;

/// <summary>
/// API Controller para gestionar clientes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ClienteService _clienteService;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(ClienteService clienteService, ILogger<ClientesController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los clientes.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
    {
        try
        {
            var clientes = await _clienteService.GetClientesAsync();
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un cliente por ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Cliente>> GetCliente(Guid id)
    {
        try
        {
            var cliente = await _clienteService.GetClienteByIdAsync(id);
            if (cliente == null)
            {
                return NotFound($"Cliente con ID {id} no encontrado");
            }
            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente {ClienteId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea un nuevo cliente.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Cliente>> CreateCliente([FromBody] Cliente cliente)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nuevoCliente = await _clienteService.AddClienteAsync(cliente);
            return CreatedAtAction(nameof(GetCliente), new { id = nuevoCliente.Id }, nuevoCliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cliente");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza un cliente existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Cliente>> UpdateCliente(Guid id, [FromBody] Cliente cliente)
    {
        try
        {
            if (id != cliente.Id)
            {
                return BadRequest("El ID del cliente no coincide");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clienteExistente = await _clienteService.GetClienteByIdAsync(id);
            if (clienteExistente == null)
            {
                return NotFound($"Cliente con ID {id} no encontrado");
            }

            var clienteActualizado = await _clienteService.ActualizarClienteAsync(cliente);
            return Ok(clienteActualizado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cliente {ClienteId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina un cliente.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCliente(Guid id)
    {
        try
        {
            var clienteExistente = await _clienteService.GetClienteByIdAsync(id);
            if (clienteExistente == null)
            {
                return NotFound($"Cliente con ID {id} no encontrado");
            }

            await _clienteService.EliminarClienteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cliente {ClienteId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene estadísticas de clientes.
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<ActionResult<object>> GetEstadisticas()
    {
        try
        {
            var npsPromedio = await _clienteService.CalcularNpsPromedioAsync();
            var tasaRetencion = await _clienteService.CalcularTasaRetencionPromedioAsync();
            
            return Ok(new
            {
                NpsPromedio = npsPromedio,
                TasaRetencionPromedio = tasaRetencion
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de clientes");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
