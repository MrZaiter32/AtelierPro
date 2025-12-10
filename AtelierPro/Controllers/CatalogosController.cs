// CatalogosController.cs
// Controller para el submódulo de Catálogos en el módulo de Almacén
// Integrado en AtelierPro

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AtelierPro.Data;
using AtelierPro.Models;
using AtelierPro.Services.Catalogos;
using AtelierPro.Services;

namespace AtelierPro.Controllers
{
    /// <summary>
    /// API REST para gestionar consultas de catálogos de proveedores
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CatalogosController : ControllerBase
    {
        private readonly CatalogosManager _catalogosManager;
        private readonly AlmacenService _almacenService;
        private readonly AtelierProDbContext _context;
        private readonly ILogger<CatalogosController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public CatalogosController(
            AlmacenService almacenService,
            AtelierProDbContext context,
            ILogger<CatalogosController> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _almacenService = almacenService ?? throw new ArgumentNullException(nameof(almacenService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            
            _catalogosManager = new CatalogosManager();

            // Registrar servicios de catálogos disponibles
            InicializarServicios();
        }

        private void InicializarServicios()
        {
            // FinditParts - usando HttpClient inyectado
            var finditPartsService = new FinditPartsCatalogoService(_httpClientFactory.CreateClient("FinditParts"));
            _catalogosManager.RegistrarServicio(finditPartsService);

            // Agregar más proveedores aquí cuando estén disponibles:
            // var fleetPrideService = new FleetPrideCatalogoService();
            // _catalogosManager.RegistrarServicio(fleetPrideService);
            
            // var meritorService = new MeritorCatalogoService();
            // _catalogosManager.RegistrarServicio(meritorService);
        }

        /// <summary>
        /// Busca un producto en todos los catálogos
        /// GET /api/catalogos/buscar?partNumber=ABC123&manufacturer=Meritor
        /// </summary>
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarProducto([FromQuery] string partNumber, [FromQuery] string manufacturer = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(partNumber))
                {
                    return BadRequest(new { success = false, message = "Part Number es requerido" });
                }

                var resultado = await _catalogosManager.BuscarEnTodosCatalogosAsync(partNumber, manufacturer);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar producto en catálogos: {PartNumber}", partNumber);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca en un catálogo específico
        /// GET /api/catalogos/buscar/{proveedor}?partNumber=ABC123&manufacturer=Meritor
        /// </summary>
        [HttpGet("buscar/{proveedor}")]
        public async Task<IActionResult> BuscarEnCatalogoEspecifico(
            string proveedor, 
            [FromQuery] string partNumber, 
            [FromQuery] string manufacturer = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(proveedor))
                {
                    return BadRequest(new { success = false, message = "Proveedor es requerido" });
                }

                if (string.IsNullOrWhiteSpace(partNumber))
                {
                    return BadRequest(new { success = false, message = "Part Number es requerido" });
                }

                var resultado = await _catalogosManager.BuscarEnCatalogoAsync(proveedor, partNumber, manufacturer);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar en catálogo específico: {Proveedor}, {PartNumber}", proveedor, partNumber);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene detalles completos de un producto desde una URL
        /// POST /api/catalogos/producto/detalles
        /// </summary>
        [HttpPost("producto/detalles")]
        public async Task<IActionResult> ObtenerDetallesProducto([FromBody] ProductoDetalleRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Url))
                {
                    return BadRequest(new { success = false, message = "URL es requerida" });
                }

                var producto = await _catalogosManager.ObtenerProductoPorUrlAsync(request.Url);
                return Ok(new { success = true, producto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalles del producto: {Url}", request?.Url);
                return StatusCode(500, new { success = false, message = "Error obteniendo detalles del producto" });
            }
        }

        /// <summary>
        /// Importa un producto desde un catálogo al inventario de AtelierPRO
        /// POST /api/catalogos/producto/importar
        /// </summary>
        [HttpPost("producto/importar")]
        public async Task<IActionResult> ImportarProductoAlInventario([FromBody] ImportarProductoRequest request)
        {
            try
            {
                if (request?.Producto == null)
                {
                    return BadRequest(new { success = false, message = "Datos del producto son requeridos" });
                }

                var importado = await _catalogosManager.ImportarProductoAlInventarioAsync(
                    request.Producto!,
                    async (p) => await GuardarProductoEnBDAsync(ConvertirProductoCatalogo(p))
                );

                if (importado)
                {
                    return Ok(new { success = true, message = "Producto importado exitosamente" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "No se pudo importar el producto" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importando producto: {PartNumber}", request?.Producto?.PartNumber);
                return StatusCode(500, new { success = false, message = "Error importando producto" });
            }
        }

        /// <summary>
        /// Verifica el estado de los servicios de catálogos
        /// GET /api/catalogos/servicios/estado
        /// </summary>
        [HttpGet("servicios/estado")]
        public async Task<IActionResult> VerificarServiciosAsync()
        {
            try
            {
                var estados = await _catalogosManager.VerificarDisponibilidadServiciosAsync();
                return Ok(new { success = true, servicios = estados });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando servicios de catálogos");
                return StatusCode(500, new { success = false, message = "Error verificando servicios" });
            }
        }

        /// <summary>
        /// Guarda el producto en la base de datos de AtelierPRO
        /// </summary>
        private async Task<bool> GuardarProductoEnBDAsync(AtelierPro.Models.ProductoCatalogo producto)
        {
            try
            {
                // Verificar si ya existe una refacción con ese SKU
                var refaccionExistente = await _context.Refacciones
                    .FirstOrDefaultAsync(r => r.Sku == producto.PartNumber);

                if (refaccionExistente != null)
                {
                    _logger.LogWarning("La refacción con SKU {Sku} ya existe", producto.PartNumber);
                    return false;
                }

                // Crear nueva refacción
                var nuevaRefaccion = new Refaccion
                {
                    Sku = producto.PartNumber,
                    Nombre = !string.IsNullOrEmpty(producto.Description) 
                        ? producto.Description.Substring(0, Math.Min(producto.Description.Length, 100))
                        : producto.PartNumber,
                    Descripcion = producto.Description + " " + producto.AdditionalInfo,
                    StockActual = 0,
                    StockMinimo = 1,
                    StockMaximo = 10,
                    CostoPromedio = 0,
                    PrecioVenta = 0,
                    Categoria = "Catálogo Importado",
                    Ubicacion = "",
                    Activa = true,
                    FechaActualizacion = DateTime.UtcNow
                };

                _context.Refacciones.Add(nuevaRefaccion);
                await _context.SaveChangesAsync();

                // Guardar referencias cruzadas si existen
                if (producto.CrossReferences != null && producto.CrossReferences.Any())
                {
                    var referencias = producto.CrossReferences.Select(crossRef => new ReferenciaAlternativa
                    {
                        RefaccionId = nuevaRefaccion.Id,
                        FabricanteRef = crossRef.Manufacturer,
                        PartNumberRef = crossRef.PartNumber,
                        Tipo = crossRef.Tipo,
                        ProveedorCatalogo = producto.Proveedor,
                        UrlCatalogo = producto.Url,
                        FechaActualizacion = DateTime.UtcNow
                    });

                    _context.ReferenciasAlternativas.AddRange(referencias);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Producto importado exitosamente: {Sku} - {Descripcion}", 
                    nuevaRefaccion.Sku, nuevaRefaccion.Descripcion);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando producto en BD: {PartNumber}", producto.PartNumber);
                throw;
            }
        }

        /// <summary>
        /// Convierte ProductoCatalogo del servicio al modelo
        /// </summary>
        private AtelierPro.Models.ProductoCatalogo ConvertirProductoCatalogo(AtelierPro.Services.Catalogos.ProductoCatalogo producto)
        {
            return new AtelierPro.Models.ProductoCatalogo
            {
                Proveedor = producto.Proveedor,
                PartNumber = producto.PartNumber,
                Manufacturer = producto.Manufacturer,
                Description = producto.Description,
                Url = producto.Url,
                CrossReferences = producto.CrossReferences?.Select(cr => new AtelierPro.Models.CrossReference
                {
                    Manufacturer = cr.Manufacturer,
                    PartNumber = cr.PartNumber,
                    Tipo = cr.Tipo
                }).ToList() ?? new List<AtelierPro.Models.CrossReference>(),
                AdditionalInfo = producto.AdditionalInfo
            };
        }
    }

    #region Request/Response Models

    /// <summary>
    /// Modelo para solicitar detalles de un producto
    /// </summary>
    public class ProductoDetalleRequest
    {
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para importar un producto
    /// </summary>
    public class ImportarProductoRequest
    {
        public AtelierPro.Services.Catalogos.ProductoCatalogo? Producto { get; set; }
        public string? Categoria { get; set; }
        public string? Ubicacion { get; set; }
    }

    #endregion
}
