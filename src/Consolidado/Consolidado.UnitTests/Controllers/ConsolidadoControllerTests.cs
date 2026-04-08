using System;
using System.Threading.Tasks;
using Consolidado.API.Controllers;
using Consolidado.Application.Services;
using Consolidado.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace Consolidado.UnitTests.Controllers;

public class ConsolidadoControllerTests
{
    private readonly Mock<ISaldoService> _serviceMock;
    private readonly ConsolidadoController _controller;

    public ConsolidadoControllerTests()
    {
        _serviceMock = new Mock<ISaldoService>();
        _controller = new ConsolidadoController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetSaldoDiario_DeveRetornarOk_ComSaldoExistente()
    {
        // Arrange
        var data = DateTime.UtcNow.Date;
        var saldo = new SaldoDiario { Data = data, TotalCreditos = 100 };
        _serviceMock.Setup(s => s.GetSaldoDiarioAsync(data))
            .ReturnsAsync(saldo);

        // Act
        var result = await _controller.GetSaldoDiario(data);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(saldo);
    }

    [Fact]
    public async Task GetSaldoDiario_DeveRetornarZerado_QuandoNaoExisteSaldo()
    {
        // Arrange
        var data = DateTime.UtcNow.Date;
        _serviceMock.Setup(s => s.GetSaldoDiarioAsync(data))
            .ReturnsAsync((SaldoDiario?)null);

        // Act
        var result = await _controller.GetSaldoDiario(data);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSaldoDiario_DeveLancarExcecao_QuandoServicoFalha()
    {
        // Arrange
        var data = DateTime.UtcNow.Date;
        _serviceMock.Setup(s => s.GetSaldoDiarioAsync(data))
            .ThrowsAsync(new System.Exception("Erro de banco"));

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _controller.GetSaldoDiario(data));
    }
}
