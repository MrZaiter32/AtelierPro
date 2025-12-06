using Xunit;
using AtelierPro.Models;
using AtelierPro.Services;

namespace AtelierPro.Tests;

public class WorkflowServiceTests
{
    private readonly WorkflowService _workflowService;

    public WorkflowServiceTests()
    {
        _workflowService = new WorkflowService();
    }

    [Fact]
    public void CambiarEstado_DeBorradorAAprobado_CambiaEstadoCorrectamente()
    {
        // Arrange
        var presupuesto = new Presupuesto
        {
            Estado = EstadoPresupuesto.Borrador
        };

        // Act
        _workflowService.CambiarEstado(presupuesto, EstadoPresupuesto.Aprobado);

        // Assert
        Assert.Equal(EstadoPresupuesto.Aprobado, presupuesto.Estado);
    }

    [Fact]
    public void CambiarEstado_DeAprobadoACerrado_CambiaEstadoCorrectamente()
    {
        // Arrange
        var presupuesto = new Presupuesto
        {
            Estado = EstadoPresupuesto.Aprobado
        };

        // Act
        _workflowService.CambiarEstado(presupuesto, EstadoPresupuesto.Cerrado);

        // Assert
        Assert.Equal(EstadoPresupuesto.Cerrado, presupuesto.Estado);
    }

    [Fact]
    public void CambiarEstado_DeCerradoAFacturado_CambiaEstadoCorrectamente()
    {
        // Arrange
        var presupuesto = new Presupuesto
        {
            Estado = EstadoPresupuesto.Cerrado
        };

        // Act
        _workflowService.CambiarEstado(presupuesto, EstadoPresupuesto.Facturado);

        // Assert
        Assert.Equal(EstadoPresupuesto.Facturado, presupuesto.Estado);
    }

    [Fact]
    public void CambiarEstado_TransicionInvalida_LanzaExcepcion()
    {
        // Arrange
        var presupuesto = new Presupuesto
        {
            Estado = EstadoPresupuesto.Borrador
        };

        // Act & Assert
        // Intentar pasar directamente de Borrador a Facturado (transición inválida)
        Assert.Throws<InvalidOperationException>(() =>
            _workflowService.CambiarEstado(presupuesto, EstadoPresupuesto.Facturado));
    }
}
