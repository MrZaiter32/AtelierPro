// ICatalogoProveedorService.cs
// Interfaz genérica para consultar catálogos de proveedores en línea
// Integrado en AtelierPro

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AtelierPro.Services.Catalogos
{
    /// <summary>
    /// Interfaz base para servicios de consulta de catálogos de proveedores
    /// </summary>
    public interface ICatalogoProveedorService
    {
        /// <summary>
        /// Nombre del proveedor (ej: "FinditParts", "FleetPride", "Meritor")
        /// </summary>
        string NombreProveedor { get; }

        /// <summary>
        /// Busca productos por part number
        /// </summary>
        Task<List<ProductoCatalogo>> BuscarPorPartNumberAsync(string partNumber, string manufacturer = null);

        /// <summary>
        /// Obtiene información detallada de un producto específico
        /// </summary>
        Task<ProductoCatalogo> ObtenerProductoAsync(string url);

        /// <summary>
        /// Busca productos por descripción o palabra clave
        /// </summary>
        Task<List<ProductoCatalogo>> BuscarPorDescripcionAsync(string descripcion);

        /// <summary>
        /// Verifica si el servicio está disponible
        /// </summary>
        Task<bool> VerificarDisponibilidadAsync();
    }

    /// <summary>
    /// Modelo de producto unificado para todos los proveedores
    /// </summary>
    public class ProductoCatalogo
    {
        /// <summary>
        /// Proveedor del catálogo (FinditParts, FleetPride, etc.)
        /// </summary>
        public string Proveedor { get; set; }

        /// <summary>
        /// URL del producto en el sitio del proveedor
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Part Number del producto
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Fabricante del producto
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Descripción del producto
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Información adicional (especificaciones, notas, etc.)
        /// </summary>
        public string AdditionalInfo { get; set; }

        /// <summary>
        /// Referencias cruzadas (números de parte alternativos)
        /// </summary>
        public List<CrossReference> CrossReferences { get; set; }

        /// <summary>
        /// Precio (si está disponible)
        /// </summary>
        public decimal? Precio { get; set; }

        /// <summary>
        /// Disponibilidad del producto
        /// </summary>
        public string Disponibilidad { get; set; }

        /// <summary>
        /// Imagen del producto (URL)
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Categoría del producto
        /// </summary>
        public string Categoria { get; set; }

        /// <summary>
        /// Fecha de consulta
        /// </summary>
        public DateTime FechaConsulta { get; set; }

        public ProductoCatalogo()
        {
            CrossReferences = new List<CrossReference>();
            FechaConsulta = DateTime.Now;
        }
    }

    /// <summary>
    /// Modelo de referencia cruzada
    /// </summary>
    public class CrossReference
    {
        /// <summary>
        /// Fabricante de la referencia alternativa
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Part Number alternativo
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Tipo de relación (Equivalente, Reemplazo, Alternativo, etc.)
        /// </summary>
        public string Tipo { get; set; }

        public override string ToString()
        {
            return $"{Manufacturer} {PartNumber}";
        }
    }

    /// <summary>
    /// Resultado de búsqueda con metadatos
    /// </summary>
    public class ResultadoBusqueda
    {
        public bool Success { get; set; }
        public string Mensaje { get; set; }
        public List<ProductoCatalogo> Productos { get; set; }
        public int TotalResultados { get; set; }
        public TimeSpan TiempoRespuesta { get; set; }

        public ResultadoBusqueda()
        {
            Productos = new List<ProductoCatalogo>();
        }
    }
}
