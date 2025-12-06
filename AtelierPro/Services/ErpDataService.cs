using System;
using System.Collections.Generic;
using System.Linq;
using AtelierPro.Models;

namespace AtelierPro.Services;

/// <summary>
/// Servicio legacy para datos de demostración en memoria.
/// Este servicio se mantiene solo para compatibilidad con el dashboard existente.
/// Las nuevas funcionalidades deben usar los repositorios y la base de datos.
/// </summary>
public sealed class ErpDataService
{
    private readonly Tarifa _tarifaBase = new()
    {
        PrecioManoObraHora = 45m,
        PrecioPinturaHora = 38m,
        TasaIva = 0.16m,
        FactorRecargo = 1.05m,
        FactorDescuento = 1m
    };

    private readonly List<Presupuesto> _presupuestos = new();
    private readonly List<Cliente> _clientes = new();
    private readonly List<Refaccion> _refacciones = new();
    private readonly List<OrdenCompra> _ordenesCompra = new();
    private readonly List<OrdenReparacion> _ordenesReparacion = new();
    private readonly List<Activo> _activos = new();
    private readonly List<PlanMantenimiento> _planes = new();
    private readonly List<Transaccion> _transacciones = new();
    private readonly List<FacturaCliente> _facturas = new();

    public ErpDataService()
    {
        SembrarDatos();
    }

    private void SembrarDatos()
    {
        var vehiculo = new Vehiculo
        {
            Vin = "3VWFE21C04M000001",
            Version = "Golf Highline",
            AntiguedadAnios = 6,
            ValorResidual = 8500m
        };

        var presupuesto = new Presupuesto { Vehiculo = vehiculo };

        var item1 = new ItemPresupuesto
        {
            Tipo = TipoItemPresupuesto.Pieza,
            Codigo = "PUERT-DEL",
            Descripcion = "Puerta delantera",
            TiempoAsignadoHoras = 2.5,
            PrecioUnitario = 120m,
            PorcentajeAjuste = 0m,
            RequierePintura = true,
            RequiereDesmontajeDoble = true,
            RequiereAlineacion = false
        };

        var item2 = new ItemPresupuesto
        {
            Tipo = TipoItemPresupuesto.ManoObra,
            Codigo = "MO-01",
            Descripcion = "Mano de obra general",
            TiempoAsignadoHoras = 3,
            PrecioUnitario = _tarifaBase.PrecioManoObraHora,
            PorcentajeAjuste = 0m
        };

        presupuesto.Items.Add(item1);
        presupuesto.Items.Add(item2);
        
        // Calcular totales manualmente sin usar PresupuestoService
        presupuesto.IvaAplicado = presupuesto.Subtotal * _tarifaBase.TasaIva;
        presupuesto.TotalFinal = presupuesto.Subtotal + presupuesto.IvaAplicado;
        presupuesto.Estado = EstadoPresupuesto.Aprobado;
        
        _presupuestos.Add(presupuesto);

        _clientes.Add(new Cliente
        {
            Nombre = "Ana Pérez",
            Historial = "Cliente VIP con 5 servicios.",
            Preferencias = "Comunicación por WhatsApp.",
            Nps = 75,
            TasaRetencion = 0.92,
            Interacciones = new List<Interaccion>
            {
                new() { Fecha = DateTime.UtcNow.AddDays(-2), Tipo = "Seguimiento", Resultado = "Acepta cotización" }
            }
        });

        var tecnico = new Tecnico
        {
            Nombre = "Luis",
            Apellido = "Gómez",
            Especialidad = "Mecánica General",
            Telefono = "555-1234",
            Email = "luis@atelierpro.com",
            Activo = true,
            CostoPorHora = 250m,
            HorasPorSemana = 40m
        };

        _refacciones.AddRange(new[]
        {
            new Refaccion 
            { 
                Sku = "PUERT-DEL", 
                Nombre = "Puerta Delantera",
                Descripcion = "Puerta delantera derecha",
                StockActual = 0, 
                StockMinimo = 1, 
                StockMaximo = 5,
                CostoPromedio = 95m,
                PrecioVenta = 150m,
                Categoria = "Carrocería",
                Ubicacion = "A1-01"
            },
            new Refaccion 
            { 
                Sku = "PARAG-GTI", 
                Nombre = "Parabrisas GTI",
                Descripcion = "Parabrisas para Golf GTI",
                StockActual = 3, 
                StockMinimo = 2, 
                StockMaximo = 10,
                CostoPromedio = 70m,
                PrecioVenta = 120m,
                Categoria = "Vidrios",
                Ubicacion = "A2-03"
            }
        });

        var proveedor = new Proveedor
        {
            RazonSocial = "Autopartes Express",
            Rfc = "AAE-100101-ABC",
            Telefono = "555-9999",
            Email = "ventas@autopartesexpress.com",
            Direccion = "Calle Principal 123, Ciudad",
            ContactoPrincipal = "Juan Pérez",
            CondicionesPago = "Crédito 30 días",
            Activo = true
        };

        _ordenesCompra.Add(new OrdenCompra
        {
            ProveedorId = proveedor.Id,
            Numero = "OC-2024-001",
            Estado = EstadoOrdenCompra.Enviada,
            FechaCreacion = DateTime.UtcNow,
            FechaEnvio = DateTime.UtcNow,
            Subtotal = 150m,
            Iva = 24m,
            Total = 174m
        });

        _ordenesReparacion.Add(new OrdenReparacion
        {
            PresupuestoId = presupuesto.Id,
            TecnicoId = tecnico.Id,
            Estado = EstadoOrdenReparacion.EnCurso,
            FechaCreacion = DateTime.UtcNow.AddHours(-5),
            FechaInicio = DateTime.UtcNow.AddHours(-5),
            HorasEstimadas = 6m,
            HorasReales = 4.2m,
            Prioridad = "Normal"
        });

        _activos.Add(new Activo
        {
            Nombre = "Cabina de pintura PPG",
            Tipo = "Cabina",
            UltimaCalibracion = DateTime.UtcNow.AddDays(-200),
            FrecuenciaCalibracion = TimeSpan.FromDays(180)
        });

        _planes.Add(new PlanMantenimiento
        {
            ActivoNombre = "Elevador 2 columnas",
            Frecuencia = "Trimestral",
            ProximaFecha = DateTime.UtcNow.AddDays(20)
        });

        _transacciones.Add(new Transaccion
        {
            Tipo = "Ingreso",
            Concepto = "Cobro siniestro",
            Fecha = DateTime.UtcNow.AddDays(-1),
            Monto = 5200m
        });

        _facturas.Add(new FacturaCliente
        {
            PresupuestoId = presupuesto.Id,
            Importe = presupuesto.TotalFinal,
            FechaEmision = DateTime.UtcNow,
            Pagada = false
        });
    }

    public IEnumerable<Presupuesto> ObtenerPresupuestos() => _presupuestos;
    public IEnumerable<Cliente> ObtenerClientes() => _clientes;
    public IEnumerable<Refaccion> ObtenerRefacciones() => _refacciones;
    public IEnumerable<OrdenCompra> ObtenerOrdenesCompra() => _ordenesCompra;
    public IEnumerable<OrdenReparacion> ObtenerOrdenesReparacion() => _ordenesReparacion;
    public IEnumerable<Activo> ObtenerActivos() => _activos;
    public IEnumerable<PlanMantenimiento> ObtenerPlanes() => _planes;
    public IEnumerable<Transaccion> ObtenerTransacciones() => _transacciones;
    public IEnumerable<FacturaCliente> ObtenerFacturas() => _facturas;
    public Tarifa ObtenerTarifaBase() => _tarifaBase;

    public DashboardKpi ConstruirKpis()
    {
        var cycleTime = _ordenesReparacion.Any()
            ? _ordenesReparacion.Average(o => ((o.FechaFinReal ?? DateTime.UtcNow) - o.FechaCreacion).TotalDays)
            : 0d;

        // Calcular margen manualmente sin PresupuestoService
        decimal margenPromedio = 0;
        if (_presupuestos.Any())
        {
            margenPromedio = _presupuestos.Average(p => p.IvaAplicado);
        }

        return new DashboardKpi
        {
            MargenPromedio = margenPromedio,
            Nps = _clientes.DefaultIfEmpty().Average(c => c?.Nps ?? 0),
            TasaRetencion = _clientes.DefaultIfEmpty().Average(c => c?.TasaRetencion ?? 0),
            TasaRetrabajo = 0.04, // Valor inventado para el demo.
            CycleTimePromedioDias = Math.Round(cycleTime, 1),
            OrdenesActivas = _ordenesReparacion.Count,
            FlujoCajaProyectado = _facturas.Where(f => !f.Pagada).Sum(f => f.Importe)
        };
    }

    public IEnumerable<Refaccion> ObtenerFaltantesInventario() =>
        _refacciones.Where(r => r.StockActual < r.StockMinimo);
}
