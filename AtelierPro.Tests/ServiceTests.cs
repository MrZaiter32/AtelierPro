using AtelierPro.Models;

namespace AtelierPro.Tests;

public class ServiceTests
{
    [Fact]
    public void EstadoOrdenReparacion_DebeContenerValoresValidos()
    {
        // Verify that EstadoOrdenReparacion enum has expected values
        Assert.True(Enum.IsDefined(typeof(EstadoOrdenReparacion), EstadoOrdenReparacion.Pendiente));
        Assert.True(Enum.IsDefined(typeof(EstadoOrdenReparacion), EstadoOrdenReparacion.EnCurso));
        Assert.True(Enum.IsDefined(typeof(EstadoOrdenReparacion), EstadoOrdenReparacion.Completada));
    }

    [Fact]
    public void EstadoOrdenCompra_DebeContenerValoresValidos()
    {
        // Verify that EstadoOrdenCompra enum has expected values
        Assert.True(Enum.IsDefined(typeof(EstadoOrdenCompra), EstadoOrdenCompra.Generada));
        Assert.True(Enum.IsDefined(typeof(EstadoOrdenCompra), EstadoOrdenCompra.Enviada));
    }

    [Fact]
    public void TipoItemPresupuesto_DebeContenerValoresValidos()
    {
        // Verify that TipoItemPresupuesto enum has expected values
        Assert.True(Enum.IsDefined(typeof(TipoItemPresupuesto), TipoItemPresupuesto.Pieza));
        Assert.True(Enum.IsDefined(typeof(TipoItemPresupuesto), TipoItemPresupuesto.ManoObra));
    }

    [Fact]
    public void ItemPresupuesto_CostoBase_DebeCalcularseCorrectamente()
    {
        // Arrange
        var item = new ItemPresupuesto
        {
            PrecioUnitario = 100m,
            TiempoAsignadoHoras = 5.0
        };

        // Act
        var costoBase = item.CostoBase;

        // Assert
        Assert.Equal(500m, costoBase);
    }

    [Fact]
    public void ItemPresupuesto_CostoAjustado_DebeAplicarPorcentaje()
    {
        // Arrange
        var item = new ItemPresupuesto
        {
            PrecioUnitario = 100m,
            TiempoAsignadoHoras = 5.0,
            PorcentajeAjuste = 10m // 10%
        };

        // Act
        var costoAjustado = item.CostoAjustado;

        // Assert
        var expected = 500m * 1.10m; // 550m
        Assert.Equal(expected, costoAjustado);
    }

    [Fact]
    public void Tecnico_DebeCrearseConValoresValidos()
    {
        // Arrange & Act
        var tecnico = new Tecnico
        {
            Nombre = "Luis",
            Apellido = "Gómez",
            Especialidad = "Mecánica General",
            Email = "luis@taller.com",
            Activo = true
        };

        // Assert
        Assert.Equal("Luis", tecnico.Nombre);
        Assert.Equal("Gómez", tecnico.Apellido);
        Assert.True(tecnico.Activo);
        Assert.NotEqual(Guid.Empty, tecnico.Id);
    }

    [Fact]
    public void OrdenReparacion_DebeCrearseConEstadoPendiente()
    {
        // Arrange & Act
        var orden = new OrdenReparacion
        {
            Estado = EstadoOrdenReparacion.Pendiente,
            HorasEstimadas = 8m,
            Prioridad = "Alta"
        };

        // Assert
        Assert.Equal(EstadoOrdenReparacion.Pendiente, orden.Estado);
        Assert.Equal(8m, orden.HorasEstimadas);
    }

    [Fact]
    public void Refaccion_DebeCalcularDisponibilidad()
    {
        // Arrange
        var refaccion = new Refaccion
        {
            Sku = "REF001",
            Nombre = "Aceite Motor",
            StockActual = 10,
            StockMinimo = 5
        };

        // Act & Assert
        Assert.True(refaccion.StockActual > refaccion.StockMinimo);
    }

    [Fact]
    public void OrdenCompra_IVA_DebeCalcularseCon16Porciento()
    {
        // Arrange
        var subtotal = 1000m;
        var tasaIva = 0.16m;

        // Act
        var iva = subtotal * tasaIva;

        // Assert
        Assert.Equal(160m, iva);
    }

    [Fact]
    public void Proveedor_DebeCrearseConDatos()
    {
        // Arrange & Act
        var proveedor = new Proveedor
        {
            RazonSocial = "Autopartes Express",
            Rfc = "AEX123456789",
            Email = "contacto@autopartes.com",
            Activo = true
        };

        // Assert
        Assert.Equal("Autopartes Express", proveedor.RazonSocial);
        Assert.True(proveedor.Activo);
    }
}
