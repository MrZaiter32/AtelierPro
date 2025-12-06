using Xunit;
using AtelierPro.Models;
using AtelierPro.Services;

namespace AtelierPro.Tests;

public class ReglaServiceTests
{
    private readonly ReglaService _reglaService;
    private readonly Tarifa _tarifa;

    public ReglaServiceTests()
    {
        _reglaService = new ReglaService();
        _tarifa = new Tarifa
        {
            PrecioManoObraHora = 45m,
            PrecioPinturaHora = 38m,
            TasaIva = 0.16m
        };
    }

    [Fact]
    public void AplicarReglas_ConVehiculoAntiguo_AplicaDepreciacion()
    {
        // Arrange
        var vehiculo = new Vehiculo { AntiguedadAnios = 5, Vin = "TEST123" };
        var item = new ItemPresupuesto
        {
            Tipo = TipoItemPresupuesto.Pieza,
            Codigo = "TEST-01",
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
        var resultado = _reglaService.AplicarReglas(presupuesto, _tarifa);

        // Assert
        Assert.NotNull(resultado);
        // Con 5 años de antigüedad, esperamos 50% de depreciación (5 * 10% = 50%)
        Assert.Equal(-50m, item.PorcentajeAjuste);
    }

    [Fact]
    public void AplicarReglas_ConPiezaQueRequierePintura_AgregaItemPintura()
    {
        // Arrange
        var vehiculo = new Vehiculo { AntiguedadAnios = 0, Vin = "TEST456" };
        var item = new ItemPresupuesto
        {
            Tipo = TipoItemPresupuesto.Pieza,
            Codigo = "PUERTA-01",
            PrecioUnitario = 120m,
            TiempoAsignadoHoras = 2.5,
            RequierePintura = true
        };
        var presupuesto = new Presupuesto
        {
            Vehiculo = vehiculo,
            Items = new List<ItemPresupuesto> { item }
        };

        // Act
        var resultado = _reglaService.AplicarReglas(presupuesto, _tarifa);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, presupuesto.Items.Count);
        var itemPintura = presupuesto.Items.FirstOrDefault(i => i.Tipo == TipoItemPresupuesto.Pintura);
        Assert.NotNull(itemPintura);
        Assert.Contains("PUERTA-01", itemPintura.Codigo);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, -10)]
    [InlineData(3, -30)]
    [InlineData(5, -50)]
    [InlineData(10, -50)] // Máximo 50%
    public void AplicarDepreciacion_ConDiferentesAnos_AplicaPorcentajeCorrecto(int anos, decimal porcentajeEsperado)
    {
        // Arrange
        var vehiculo = new Vehiculo { AntiguedadAnios = anos, Vin = "TEST789" };
        var item = new ItemPresupuesto
        {
            Tipo = TipoItemPresupuesto.Pieza,
            Codigo = "TEST-PIECE",
            PrecioUnitario = 100m,
            TiempoAsignadoHoras = 1
        };
        var presupuesto = new Presupuesto
        {
            Vehiculo = vehiculo,
            Items = new List<ItemPresupuesto> { item }
        };

        // Act
        _reglaService.AplicarReglas(presupuesto, _tarifa);

        // Assert
        Assert.Equal(porcentajeEsperado, item.PorcentajeAjuste);
    }
}
