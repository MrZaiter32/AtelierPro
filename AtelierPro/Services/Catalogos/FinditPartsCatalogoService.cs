// FinditPartsCatalogoService.cs
// Implementación del servicio de catálogo para FinditParts
// Ubicación sugerida: AtelierPRO.Almacen.Services.Catalogos

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AtelierPro.Services.Catalogos
{
    /// <summary>
    /// Servicio de consulta de catálogo de FinditParts
    /// Utiliza la API REST del scraper Python
    /// </summary>
    public class FinditPartsCatalogoService : ICatalogoProveedorService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public string NombreProveedor => "FinditParts";

        public FinditPartsCatalogoService(string apiBaseUrl = "http://localhost:5000")
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60) // Scraping puede tardar
            };
            _apiBaseUrl = apiBaseUrl;
        }

        public async Task<bool> VerificarDisponibilidadAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ProductoCatalogo>> BuscarPorPartNumberAsync(string partNumber, string manufacturer = null)
        {
            try
            {
                var request = new
                {
                    part_number = partNumber,
                    manufacturer = manufacturer
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/producto/part-number", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<SearchApiResponse>(responseString);
                    
                    // La API actual solo retorna la URL de búsqueda
                    // Aquí podrías implementar lógica adicional para parsear resultados
                    return new List<ProductoCatalogo>();
                }

                return new List<ProductoCatalogo>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error buscando en {NombreProveedor}: {ex.Message}", ex);
            }
        }

        public async Task<ProductoCatalogo> ObtenerProductoAsync(string url)
        {
            try
            {
                var request = new { url = url };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/producto", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ProductoApiResponse>(responseString);
                    
                    if (result?.Success == true && result.Data != null)
                    {
                        return ConvertirAProductoCatalogo(result.Data, url);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error obteniendo producto de {NombreProveedor}: {ex.Message}", ex);
            }
        }

        public async Task<List<ProductoCatalogo>> BuscarPorDescripcionAsync(string descripcion)
        {
            // FinditParts no soporta búsqueda por descripción directamente
            // Retornar lista vacía o implementar lógica personalizada
            return new List<ProductoCatalogo>();
        }

        /// <summary>
        /// Obtiene múltiples productos en lote (más eficiente)
        /// </summary>
        public async Task<List<ProductoCatalogo>> ObtenerProductosLoteAsync(List<string> urls)
        {
            try
            {
                var request = new { urls = urls };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/productos/lote", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<LoteApiResponse>(responseString);
                    
                    if (result?.Success == true && result.Productos != null)
                    {
                        return result.Productos
                            .Where(p => p.Success && p.Data != null)
                            .Select(p => ConvertirAProductoCatalogo(p.Data, p.Url))
                            .ToList();
                    }
                }

                return new List<ProductoCatalogo>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error obteniendo lote de {NombreProveedor}: {ex.Message}", ex);
            }
        }

        private ProductoCatalogo ConvertirAProductoCatalogo(ProductoData data, string url)
        {
            var producto = new ProductoCatalogo
            {
                Proveedor = NombreProveedor,
                Url = url,
                PartNumber = data.PartNumber,
                Manufacturer = data.Manufacturer,
                Description = data.Description,
                AdditionalInfo = data.AdditionalInfo,
                FechaConsulta = DateTime.Now
            };

            // Parsear cross references
            if (!string.IsNullOrEmpty(data.CrossReferences))
            {
                producto.CrossReferences = ParsearCrossReferences(data.CrossReferences);
            }

            return producto;
        }

        private List<CrossReference> ParsearCrossReferences(string crossRefsText)
        {
            var referencias = new List<CrossReference>();

            if (string.IsNullOrWhiteSpace(crossRefsText))
                return referencias;

            // Formato típico: "BWP-NSI M-1003" o "Manufacturer PartNumber"
            var items = crossRefsText.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in items)
            {
                var parts = item.Trim().Split(new[] { ' ' }, 2);
                
                if (parts.Length == 2)
                {
                    referencias.Add(new CrossReference
                    {
                        Manufacturer = parts[0].Trim(),
                        PartNumber = parts[1].Trim(),
                        Tipo = "Equivalente"
                    });
                }
                else if (parts.Length == 1)
                {
                    referencias.Add(new CrossReference
                    {
                        Manufacturer = "",
                        PartNumber = parts[0].Trim(),
                        Tipo = "Alternativo"
                    });
                }
            }

            return referencias;
        }

        #region Modelos de API Response

        private class ProductoApiResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("data")]
            public ProductoData Data { get; set; }
        }

        private class ProductoData
        {
            [JsonPropertyName("part_number")]
            public string PartNumber { get; set; }

            [JsonPropertyName("manufacturer")]
            public string Manufacturer { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("additional_info")]
            public string AdditionalInfo { get; set; }

            [JsonPropertyName("cross_references")]
            public string CrossReferences { get; set; }
        }

        private class SearchApiResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("search_url")]
            public string SearchUrl { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }
        }

        private class LoteApiResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("total")]
            public int Total { get; set; }

            [JsonPropertyName("exitosos")]
            public int Exitosos { get; set; }

            [JsonPropertyName("fallidos")]
            public int Fallidos { get; set; }

            [JsonPropertyName("productos")]
            public List<ProductoLote> Productos { get; set; }
        }

        private class ProductoLote
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("data")]
            public ProductoData Data { get; set; }

            [JsonPropertyName("error")]
            public string Error { get; set; }
        }

        #endregion
    }
}
