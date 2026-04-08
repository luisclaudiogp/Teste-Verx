using System;
using Xunit;
using Consolidado.Domain.Entities;

namespace Consolidado.UnitTests.Domain;

public class SaldoDiarioTests
{
    [Fact]
    public void SaldoFinal_DeveRetornarDiferencaEntreCreditoEDebito()
    {
        // Arrange
        var saldoDiario = new SaldoDiario
        {
            Data = DateTime.UtcNow,
            TotalCreditos = 5000m,
            TotalDebitos = 1500m
        };

        // Act
        var saldoFinal = saldoDiario.SaldoFinal;

        // Assert
        Assert.Equal(3500m, saldoFinal);
    }
}
