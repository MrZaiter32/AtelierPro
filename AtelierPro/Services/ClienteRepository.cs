using AtelierPro.Data;
using AtelierPro.Models;
using Microsoft.EntityFrameworkCore;

namespace AtelierPro.Services;

/// <summary>
/// Repositorio para gestionar clientes con persistencia en base de datos.
/// </summary>
public class ClienteRepository
{
    private readonly AtelierProDbContext _context;

    public ClienteRepository(AtelierProDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cliente>> ObtenerTodosAsync()
    {
        return await _context.Clientes
            .Include(c => c.Interacciones)
            .ToListAsync();
    }

    public async Task<Cliente?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Clientes
            .Include(c => c.Interacciones)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente> CrearAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<Cliente> ActualizarAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task EliminarAsync(Guid id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Cliente>> BuscarPorNombreAsync(string nombre)
    {
        return await _context.Clientes
            .Include(c => c.Interacciones)
            .Where(c => c.Nombre.Contains(nombre))
            .ToListAsync();
    }
}
