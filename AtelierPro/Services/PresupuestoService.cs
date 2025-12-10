using AtelierPro.Data;
using AtelierPro.Models;
using Microsoft.EntityFrameworkCore;

namespace AtelierPro.Services
{
    /// <summary>
    /// Servicio de dominio para la lógica de presupuestos.
    /// Gestiona la creación, actualización, aprobación y listar presupuestos.
    /// Implementa validación, transaccionalidad y trazabilidad per Clean Architecture.
    /// </summary>
    public class PresupuestoService
    {
        private readonly AtelierProDbContext _context;
        private readonly ILogger<PresupuestoService> _logger;

        public PresupuestoService(AtelierProDbContext context, ILogger<PresupuestoService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene presupuestos con filtro opcional por estado.
        /// </summary>
        public async Task<List<Presupuesto>> ObtenerPresupuestosAsync(EstadoPresupuesto? estado = null)
        {
            try
            {
                IQueryable<Presupuesto> query = _context.Presupuestos.AsNoTracking()
                    .Include(p => p.Cliente)
                    .Include(p => p.Items);

                if (estado.HasValue)
                {
                    query = query.Where(p => p.Estado == estado.Value);
                }

                query = query.OrderByDescending(p => p.FechaCreacion);

                var presupuestos = await query.ToListAsync();
                _logger.LogInformation($"Se obtuvieron {presupuestos.Count} presupuestos (Estado: {estado?.ToString() ?? "Todos"}).");
                return presupuestos;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener presupuestos: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un presupuesto por ID con todas sus relaciones cargadas.
        /// </summary>
        public async Task<Presupuesto?> ObtenerPresupuestoAsync(Guid id)
        {
            try
            {
                var presupuesto = await _context.Presupuestos
                    .Include(p => p.Cliente)
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (presupuesto == null)
                {
                    _logger.LogWarning($"Presupuesto con ID {id} no encontrado.");
                }
                else
                {
                    _logger.LogInformation($"Presupuesto {presupuesto.Numero} obtenido correctamente.");
                }

                return presupuesto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener presupuesto {id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo presupuesto con validación exhaustiva.
        /// Implementa patrón Unit of Work con transacción explícita.
        /// </summary>
        public async Task<Presupuesto> CrearPresupuestoAsync(
            Guid? clienteId,
            string? placaVehiculo,
            string? vinVehiculo,
            List<ItemPresupuestoVm> items,
            decimal tasaIva = 0.21m,
            string? observaciones = null,
            string? usuarioCreador = null)
        {
            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validar cliente si se proporciona
                Cliente? cliente = null;
                if (clienteId.HasValue && clienteId.Value != Guid.Empty)
                {
                    cliente = await _context.Clientes
                        .FirstOrDefaultAsync(c => c.Id == clienteId.Value);

                    if (cliente == null)
                    {
                        throw new InvalidOperationException(
                            $"Cliente con ID {clienteId} no existe o fue eliminado.");
                    }
                }

                // 2. Validar items no esté vacío
                if (items == null || items.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Un presupuesto debe contener al menos un item.");
                }

                // 3. Validar cada item
                foreach (var item in items)
                {
                    if (item.Cantidad <= 0)
                        throw new InvalidOperationException(
                            $"Item '{item.Descripcion}': cantidad debe ser > 0.");

                    if (item.PrecioUnitario < 0)
                        throw new InvalidOperationException(
                            $"Item '{item.Descripcion}': precio no puede ser negativo.");

                    if (item.TiempoAsignadoHoras < 0)
                        throw new InvalidOperationException(
                            $"Item '{item.Descripcion}': tiempo no puede ser negativo.");
                }

                // 4. Generar número único de presupuesto
                var numeroPresupuesto = await GenerarNumeroPresupuestoAsync();

                // 5. Crear entidades de item presupuesto
                var itemsEntidades = items.Select(i => new ItemPresupuesto
                {
                    Id = Guid.NewGuid(),
                    Tipo = i.Tipo,
                    Codigo = i.Codigo ?? string.Empty,
                    Descripcion = i.Descripcion,
                    Cantidad = i.Cantidad,
                    TiempoAsignadoHoras = i.TiempoAsignadoHoras,
                    PrecioUnitario = i.PrecioUnitario,
                    PorcentajeAjuste = i.PorcentajeAjuste,
                    RequierePintura = i.RequierePintura,
                    RequiereDesmontajeDoble = i.RequiereDesmontajeDoble,
                    RequiereAlineacion = i.RequiereAlineacion
                }).ToList();

                // 6. Calcular subtotal e IVA
                var subtotal = itemsEntidades.Sum(i => i.CostoAjustado);
                var iva = subtotal * tasaIva;
                var total = subtotal + iva;

                // 7. Crear presupuesto
                var presupuesto = new Presupuesto
                {
                    Id = Guid.NewGuid(),
                    Numero = numeroPresupuesto,
                    ClienteId = cliente?.Id,
                    Cliente = cliente,
                    PlacaVehiculo = placaVehiculo,
                    VinVehiculo = vinVehiculo,
                    Items = itemsEntidades,
                    IvaAplicado = iva,
                    Total = total,
                    Estado = EstadoPresupuesto.Borrador,
                    FechaCreacion = DateTime.UtcNow,
                    Observaciones = observaciones
                };

                _context.Presupuestos.Add(presupuesto);
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                _logger.LogInformation(
                    $"Presupuesto {numeroPresupuesto} creado exitosamente. " +
                    $"Cliente: {cliente?.Nombre ?? "N/A"}, Items: {itemsEntidades.Count}, " +
                    $"Total: {total:C}, Usuario: {usuarioCreador ?? "Sistema"}");

                return presupuesto;
            }
            catch (DbUpdateException ex)
            {
                await transaccion.RollbackAsync();
                _logger.LogError($"Error de BD al crear presupuesto: {ex.Message}");
                throw new InvalidOperationException(
                    "Ocurrió un error al guardar el presupuesto. Por favor, intente nuevamente.",
                    ex);
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                _logger.LogError($"Error inesperado al crear presupuesto: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Aprueba un presupuesto (cambio de estado).
        /// </summary>
        public async Task<Presupuesto> AprobarPresupuestoAsync(Guid presupuestoId, string? usuarioAprobador = null)
        {
            try
            {
                var presupuesto = await _context.Presupuestos
                    .FirstOrDefaultAsync(p => p.Id == presupuestoId);

                if (presupuesto == null)
                    throw new InvalidOperationException($"Presupuesto con ID {presupuestoId} no existe.");

                if (presupuesto.Estado != EstadoPresupuesto.Borrador)
                    throw new InvalidOperationException(
                        $"Solo presupuestos en Borrador pueden ser aprobados. Estado actual: {presupuesto.Estado}");

                presupuesto.Estado = EstadoPresupuesto.Aprobado;
                presupuesto.FechaAprobacion = DateTime.UtcNow;

                _context.Presupuestos.Update(presupuesto);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    $"Presupuesto {presupuesto.Numero} aprobado por {usuarioAprobador ?? "Sistema"}.");

                return presupuesto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al aprobar presupuesto {presupuestoId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Rechaza un presupuesto (cambio de estado).
        /// </summary>
        public async Task<Presupuesto> RechazarPresupuestoAsync(Guid presupuestoId, string? motivo = null)
        {
            try
            {
                var presupuesto = await _context.Presupuestos
                    .FirstOrDefaultAsync(p => p.Id == presupuestoId);

                if (presupuesto == null)
                    throw new InvalidOperationException($"Presupuesto con ID {presupuestoId} no existe.");

                if (presupuesto.Estado != EstadoPresupuesto.Borrador)
                    throw new InvalidOperationException(
                        $"Solo presupuestos en Borrador pueden ser rechazados. Estado actual: {presupuesto.Estado}");

                presupuesto.Estado = EstadoPresupuesto.Rechazado;
                presupuesto.Observaciones = $"Rechazado. Motivo: {motivo ?? "N/A"}";

                _context.Presupuestos.Update(presupuesto);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    $"Presupuesto {presupuesto.Numero} rechazado. Motivo: {motivo ?? "Sin especificar"}");

                return presupuesto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al rechazar presupuesto {presupuestoId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Genera un número único para presupuesto en formato "P{AÑO}-{SECUENCIAL}".
        /// </summary>
        private async Task<string> GenerarNumeroPresupuestoAsync()
        {
            try
            {
                var añoActual = DateTime.UtcNow.Year;
                var ultimoPresupuesto = await _context.Presupuestos
                    .AsNoTracking()
                    .Where(p => p.Numero.StartsWith($"P{añoActual}-"))
                    .OrderByDescending(p => p.Numero)
                    .FirstOrDefaultAsync();

                int secuencial = 1;
                if (ultimoPresupuesto != null)
                {
                    var numeroParte = ultimoPresupuesto.Numero.Split('-').LastOrDefault() ?? "0";
                    if (int.TryParse(numeroParte, out int ultimoSecuencial))
                    {
                        secuencial = ultimoSecuencial + 1;
                    }
                }

                return $"P{añoActual}-{secuencial:D5}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar número de presupuesto: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Calcula el total de un presupuesto (para preview antes de guardar).
        /// </summary>
        public decimal CalcularTotal(List<ItemPresupuestoVm> items, decimal tasaIva = 0.21m)
        {
            if (items == null || items.Count == 0)
                return 0;

            var subtotal = items.Sum(i =>
                i.PrecioUnitario * (decimal)i.TiempoAsignadoHoras * i.Cantidad *
                (1 + i.PorcentajeAjuste / 100m));

            var iva = subtotal * tasaIva;
            return subtotal + iva;
        }
    }

    /// <summary>
    /// ViewModel para transferencia de datos de items presupuesto desde UI.
    /// </summary>
    public class ItemPresupuestoVm
    {
        public TipoItemPresupuesto Tipo { get; set; }
        public string? Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; } = 1;
        public double TiempoAsignadoHoras { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PorcentajeAjuste { get; set; }
        public bool RequierePintura { get; set; }
        public bool RequiereDesmontajeDoble { get; set; }
        public bool RequiereAlineacion { get; set; }
    }
}