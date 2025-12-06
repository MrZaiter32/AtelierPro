using AtelierPro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AtelierPro.Services
{
    /// <summary>
    /// Servicio para operaciones de negocio relacionadas con clientes.
    /// </summary>
    public class ClienteService
    {
        private readonly ClienteRepository _repository;

        public ClienteService(ClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Cliente>> GetClientesAsync()
        {
            var clientes = await _repository.ObtenerTodosAsync();
            return clientes.ToList();
        }

        public async Task<Cliente?> GetClienteByIdAsync(Guid id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task<Cliente> AddClienteAsync(Cliente cliente)
        {
            return await _repository.CrearAsync(cliente);
        }

        public async Task<Cliente> ActualizarClienteAsync(Cliente cliente)
        {
            return await _repository.ActualizarAsync(cliente);
        }

        public async Task EliminarClienteAsync(Guid id)
        {
            await _repository.EliminarAsync(id);
        }

        public async Task<double> CalcularNpsPromedioAsync()
        {
            var clientes = await _repository.ObtenerTodosAsync();
            return clientes.Any() ? clientes.Average(c => c.Nps) : 0;
        }

        public async Task<double> CalcularTasaRetencionPromedioAsync()
        {
            var clientes = await _repository.ObtenerTodosAsync();
            return clientes.Any() ? clientes.Average(c => c.TasaRetencion) : 0;
        }
    }
}
