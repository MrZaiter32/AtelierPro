using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AtelierPro.Models;
using Microsoft.Extensions.Logging;

namespace AtelierPro.Services
{
    public interface IFinditPartsService
    {
        Task<FinditPartsResponse> BuscarProductoAsync(string termino);
    }

    public class FinditPartsService : IFinditPartsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FinditPartsService> _logger;

        public FinditPartsService(HttpClient httpClient, ILogger<FinditPartsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<FinditPartsResponse> BuscarProductoAsync(string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    return new FinditPartsResponse
                    {
                        Success = false,
                        Error = "Debe ingresar un término de búsqueda"
                    };
                }

                _logger.LogInformation("Buscando producto: {Termino}", termino);

                var request = new { termino };
                var response = await _httpClient.PostAsJsonAsync("/buscar", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<FinditPartsResponse>(
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    return result ?? new FinditPartsResponse
                    {
                        Success = false,
                        Error = "Sin resultados"
                    };
                }

                _logger.LogError("Error HTTP: {StatusCode}", response.StatusCode);
                return new FinditPartsResponse
                {
                    Success = false,
                    Error = $"Error HTTP: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando producto");
                return new FinditPartsResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
}
