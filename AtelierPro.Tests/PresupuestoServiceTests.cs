using Xunit;
using AtelierPro.Models;
using AtelierPro.Services;

namespace AtelierPro.Tests;

public class PresupuestoServiceTests
{
    private readonly PresupuestoService _presupuestoService;
    private readonly ReglaService _reglaService;
    private readonly Tarifa _tarifa;

    public PresupuestoServiceTests()
    {
        _reglaService = new ReglaService();
        _presupuestoService = new PresupuestoService(_reglaService);
        _tarifa = new Tarifa
        {
            PrecioManoObraHora = 45m,
            PrecioPinturaHora = 38m,
            TasaIva = 0.16m
        };
    }

    [Fact]
    public void CalcularTotales_ConPresupuestoValido_CalculaCorrectamente()
    {
        // Arrange
        var vehiculo = new Vehiculo { AntiguedadAnios = 0, Vin = "TEST001" };
        var item1 = new ItemPresupuesto
        {
            Tipo = TipoItemPresupuesto.Pieza,
            Codigo = "PIEZA-01",
            PrecioUnitario = 100m,
            TiempoAsignadoHoras = 2,
            PorcentajeAjuste = 0m
        };
        var item2 = new ItemPresupuesto
        {
            Tipo = TipoItemPresupuesto.ManoObra,
            Codigo = "MO-01",
            PrecioUnitario = 45m,
            TiempoAsignadoHoras = 3,
            PorcentajeAjuste = 0m
        };
        var presupuesto = new Presupuesto
        {
            Vehiculo = vehiculo,
            Items = new List<ItemPresupuesto> { item1, item2 }
        };

        // Act
        var resultado = _presupuestoService.CalcularTotales(presupuesto, _tarifa);

        // Assert
        Assert.NotNull(resultado);
        
        // Subtotal: (100*2) + (45*3) = 200 + 135 = 335
        decimal subtotalEsperado = 335m;
        Assert.Equal(subtotalEsperado, presupuesto.Subtotal);
        
        // IVA: 335 * 0.16 = 53.6
        decimal ivaEsperado = 53.6m;
        Assert.Equal(ivaEsperado, presupuesto.IvaAplicado);
        
        // Total: 335 + 53.6 = 388.6
        decimal totalEsperado = 388.6m;
        Assert.Equal(totalEsperado, presupuesto.TotalFinal);
    }

    [Fact]
    public void CalcularTotales_ConIVAEnFormatoDecimal_CalculaCorrectamente()
    {
        // Arrange
        var vehiculo = new Vehiculo { AntiguedadAnios = 0, Vin = "TEST002" };
        var item = new ItemPresupuesto
        {
            Tipo = TipoItemPresupuesto.Pieza,
            Codigo = "PIEZA-02",
            PrecioUnitario = 100m,
            TiempoAsignadoHoras = 1,
            PorcentajeAjuste = 0m
        };
        var presupuesto = new Presupuesto
        {
            Vehiculo = vehiculo,
            Items = new List<ItemPresupuesto> { item }
        };

        // Act
        var resultado = _presupuestoService.CalcularTotales(presupuesto, _tarifa);

        // Assert
        // Subtotal: 100 * 1 = 100
        Assert.Equal(100m, presupuesto.Subtotal);
        
        // IVA debe ser 16 (100 * 0.16), NO 0.16 ni 1600
        Assert.Equal(16m, presupuesto.IvaAplicado);
        
        // Total: 100 + 16 = 116
        Assert.Equal(116m, presupuesto.TotalFinal);
    }

    [Fact]
    public void CalcularMargen_ConListaDePresupuestos_RetornaPromedioCorrectamente()
    {
        // Arrange
        var presupuesto1 = new Presupuesto
        {
            IvaAplicado = 50m,
            TotalFinal = 350m
        };
        var presupuesto2 = new Presupuesto
        {
            IvaAplicado = 100m,
            TotalFinal = 700m
        };
        var presupuestos = new List<Presupuesto> { presupuesto1, presupuesto2 };

        // Act
        var margen = _presupuestoService.CalcularMargen(presupuestos);

        // Assert
        // Margen promedio: (50 + 100) / 2 = 75
        Assert.Equal(75m, margen);
    }

    [Fact]
    public void CalcularMargen_ConListaVacia_RetornaCero()
    {
        // Arrange
        var presupuestos = new List<Presupuesto>();

        // Act
        var margen = _presupuestoService.CalcularMargen(presupuestos);

        // Assert
        Assert.Equal(0m, margen);
    }

    [Fact]
    public void AgregarItem_AgregaItemAlPresupuesto()
    {
        // Arrange
        var presupuesto = new Presupuesto();
        var item = new ItemPresupuesto
        {
            Tipo = TipoItemPresupuesto.Pieza,
            Codigo = "TEST-ITEM",
            PrecioUnitario = 50m,
            TiempoAsignadoHoras = 1
        };

        // Act
        _presupuestoService.AgregarItem(presupuesto, item, _tarifa);

        // Assert
        Assert.Single(presupuesto.Items);
        Assert.Contains(item, presupuesto.Items);
    }
}
