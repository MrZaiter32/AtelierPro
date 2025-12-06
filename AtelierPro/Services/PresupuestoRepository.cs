using AtelierPro.Data;
using AtelierPro.Models;
using Microsoft.EntityFrameworkCore;

namespace AtelierPro.Services;

/// <summary>
/// Repositorio para gestionar presupuestos con persistencia en base de datos.
/// </summary>
public class PresupuestoRepository
{
    private readonly AtelierProDbContext _context;

    public PresupuestoRepository(AtelierProDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Presupuesto>> ObtenerTodosAsync()
    {
        return await _context.Presupuestos
            .Include(p => p.Vehiculo)
            .Include(p => p.Items)
            .ToListAsync();
    }

    public async Task<Presupuesto?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Presupuestos
            .Include(p => p.Vehiculo)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Presupuesto> CrearAsync(Presupuesto presupuesto)
    {
        _context.Presupuestos.Add(presupuesto);
        await _context.SaveChangesAsync();
        return presupuesto;
    }

    public async Task<Presupuesto> ActualizarAsync(Presupuesto presupuesto)
    {
        _context.Presupuestos.Update(presupuesto);
        await _context.SaveChangesAsync();
        return presupuesto;
    }

    public async Task EliminarAsync(Guid id)
    {
        var presupuesto = await _context.Presupuestos.FindAsync(id);
        if (presupuesto != null)
        {
            _context.Presupuestos.Remove(presupuesto);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Presupuesto>> ObtenerPorEstadoAsync(EstadoPresupuesto estado)
    {
        return await _context.Presupuestos
            .Include(p => p.Vehiculo)
            .Include(p => p.Items)
            .Where(p => p.Estado == estado)
            .ToListAsync();
    }
}
