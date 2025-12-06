// CatalogosManager.cs
// Gestor centralizado para consultar múltiples catálogos de proveedores
// Ubicación sugerida: AtelierPRO.Almacen.Services.Catalogos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AtelierPro.Services.Catalogos
{
    /// <summary>
    /// Gestor centralizado para consultar múltiples catálogos de proveedores
    /// </summary>
    public class CatalogosManager
    {
        private readonly List<ICatalogoProveedorService> _servicios;

        public CatalogosManager()
        {
            _servicios = new List<ICatalogoProveedorService>();
        }

        /// <summary>
        /// Registra un nuevo servicio de catálogo
        /// </summary>
        public void RegistrarServicio(ICatalogoProveedorService servicio)
        {
            if (!_servicios.Any(s => s.NombreProveedor == servicio.NombreProveedor))
            {
                _servicios.Add(servicio);
            }
        }

        /// <summary>
        /// Obtiene lista de proveedores disponibles
        /// </summary>
        public List<string> ObtenerProveedoresDisponibles()
        {
            return _servicios.Select(s => s.NombreProveedor).ToList();
        }

        /// <summary>
        /// Busca un producto en TODOS los catálogos registrados
        /// </summary>
        public async Task<ResultadoBusqueda> BuscarEnTodosCatalogosAsync(string partNumber, string manufacturer = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var resultado = new ResultadoBusqueda();

            var tareas = _servicios.Select(async servicio =>
            {
                try
                {
                    var disponible = await servicio.VerificarDisponibilidadAsync();
                    if (!disponible)
                        return new List<ProductoCatalogo>();

                    return await servicio.BuscarPorPartNumberAsync(partNumber, manufacturer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en {servicio.NombreProveedor}: {ex.Message}");
                    return new List<ProductoCatalogo>();
                }
            });

            var resultados = await Task.WhenAll(tareas);

            resultado.Productos = resultados.SelectMany(r => r).ToList();
            resultado.TotalResultados = resultado.Productos.Count;
            resultado.Success = resultado.TotalResultados > 0;
            resultado.Mensaje = resultado.Success 
                ? $"Se encontraron {resultado.TotalResultados} productos en {_servicios.Count} catálogos"
                : "No se encontraron productos en ningún catálogo";

            stopwatch.Stop();
            resultado.TiempoRespuesta = stopwatch.Elapsed;

            return resultado;
        }

        /// <summary>
        /// Busca un producto en un catálogo específico
        /// </summary>
        public async Task<ResultadoBusqueda> BuscarEnCatalogoAsync(string nombreProveedor, string partNumber, string manufacturer = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var resultado = new ResultadoBusqueda();

            var servicio = _servicios.FirstOrDefault(s => s.NombreProveedor.Equals(nombreProveedor, StringComparison.OrdinalIgnoreCase));

            if (servicio == null)
            {
                resultado.Success = false;
                resultado.Mensaje = $"Proveedor '{nombreProveedor}' no encontrado";
                return resultado;
            }

            try
            {
                var disponible = await servicio.VerificarDisponibilidadAsync();
                if (!disponible)
                {
                    resultado.Success = false;
                    resultado.Mensaje = $"El servicio de {nombreProveedor} no está disponible";
                    return resultado;
                }

                resultado.Productos = await servicio.BuscarPorPartNumberAsync(partNumber, manufacturer);
                resultado.TotalResultados = resultado.Productos.Count;
                resultado.Success = resultado.TotalResultados > 0;
                resultado.Mensaje = resultado.Success
                    ? $"Se encontraron {resultado.TotalResultados} productos"
                    : "No se encontraron productos";
            }
            catch (Exception ex)
            {
                resultado.Success = false;
                resultado.Mensaje = $"Error: {ex.Message}";
            }

            stopwatch.Stop();
            resultado.TiempoRespuesta = stopwatch.Elapsed;

            return resultado;
        }

        /// <summary>
        /// Obtiene producto específico por URL
        /// </summary>
        public async Task<ProductoCatalogo> ObtenerProductoPorUrlAsync(string url)
        {
            // Determinar proveedor por URL
            ICatalogoProveedorService servicio = null;

            if (url.Contains("finditparts.com"))
                servicio = _servicios.FirstOrDefault(s => s.NombreProveedor == "FinditParts");
            else if (url.Contains("fleetpride.com"))
                servicio = _servicios.FirstOrDefault(s => s.NombreProveedor == "FleetPride");
            // Agregar más proveedores aquí...

            if (servicio == null)
                throw new Exception("No se encontró servicio para esta URL");

            return await servicio.ObtenerProductoAsync(url);
        }

        /// <summary>
        /// Verifica disponibilidad de todos los servicios
        /// </summary>
        public async Task<Dictionary<string, bool>> VerificarDisponibilidadServiciosAsync()
        {
            var resultado = new Dictionary<string, bool>();

            var tareas = _servicios.Select(async servicio =>
            {
                var disponible = await servicio.VerificarDisponibilidadAsync();
                return new { Nombre = servicio.NombreProveedor, Disponible = disponible };
            });

            var resultados = await Task.WhenAll(tareas);

            foreach (var r in resultados)
            {
                resultado[r.Nombre] = r.Disponible;
            }

            return resultado;
        }

        /// <summary>
        /// Importa un producto desde un catálogo al inventario de AtelierPRO
        /// </summary>
        public async Task<bool> ImportarProductoAlInventarioAsync(ProductoCatalogo producto, Func<ProductoCatalogo, Task<bool>> funcionGuardar)
        {
            try
            {
                // Validar datos mínimos
                if (string.IsNullOrEmpty(producto.PartNumber))
                    throw new Exception("Part Number es requerido");

                if (string.IsNullOrEmpty(producto.Manufacturer))
                    throw new Exception("Manufacturer es requerido");

                // Llamar a función personalizada para guardar en BD
                return await funcionGuardar(producto);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error importando producto: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Compara precios de un producto en todos los catálogos
        /// </summary>
        public async Task<List<ComparacionPrecio>> CompararPreciosAsync(string partNumber, string manufacturer = null)
        {
            var resultado = await BuscarEnTodosCatalogosAsync(partNumber, manufacturer);
            
            return resultado.Productos
                .Where(p => p.Precio.HasValue)
                .OrderBy(p => p.Precio)
                .Select(p => new ComparacionPrecio
                {
                    Proveedor = p.Proveedor,
                    PartNumber = p.PartNumber,
                    Manufacturer = p.Manufacturer,
                    Precio = p.Precio.Value,
                    Disponibilidad = p.Disponibilidad,
                    Url = p.Url
                })
                .ToList();
        }
    }

    /// <summary>
    /// Modelo para comparación de precios
    /// </summary>
    public class ComparacionPrecio
    {
        public string Proveedor { get; set; }
        public string PartNumber { get; set; }
        public string Manufacturer { get; set; }
        public decimal Precio { get; set; }
        public string Disponibilidad { get; set; }
        public string Url { get; set; }
    }
}
