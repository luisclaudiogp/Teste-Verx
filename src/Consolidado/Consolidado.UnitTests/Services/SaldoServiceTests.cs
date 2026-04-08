using Consolidado.Application.Services;
using Consolidado.Domain.Entities;
using Consolidado.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Consolidado.UnitTests.Services;

public class SaldoServiceTests
{
    private readonly Mock<ISaldoRepository> _repositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<SaldoService>> _loggerMock;
    private readonly SaldoService _service;

    public SaldoServiceTests()
    {
        _repositoryMock = new Mock<ISaldoRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<SaldoService>>();
        _service = new SaldoService(_repositoryMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AtualizarSaldoAsync_DeveCriarNovoSaldo_QuandoNaoExistir()
    {
        // Arrange
        var data = DateTime.UtcNow.Date;
        var valor = 100m;
        var tipo = "Credito";
        _repositoryMock.Setup(r => r.GetByDataAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((SaldoDiario?)null);

        // Act
        await _service.AtualizarSaldoAsync(data, valor, tipo);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<SaldoDiario>(s => 
            s.Data == DateTime.SpecifyKind(data, DateTimeKind.Utc) &&
            s.TotalCreditos == valor)), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarSaldoAsync_DeveIncrementarDebito_QuandoJaExistir()
    {
        // Arrange
        var data = DateTime.UtcNow.Date;
        var dataUtc = DateTime.SpecifyKind(data, DateTimeKind.Utc);
        var valor = 50m;
        var tipo = "Debito";
        var saldoExistente = new SaldoDiario { Data = dataUtc, TotalCreditos = 200, TotalDebitos = 10 };
        
        _repositoryMock.Setup(r => r.GetByDataAsync(dataUtc))
            .ReturnsAsync(saldoExistente);

        // Act
        await _service.AtualizarSaldoAsync(data, valor, tipo);

        // Assert
        saldoExistente.TotalDebitos.Should().Be(60); // 10 + 50
        saldoExistente.SaldoFinal.Should().Be(140); // 200 - 60
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetSaldoDiarioAsync_DeveRetornarDoCache_QuandoExistir()
    {
        // Arrange
        var data = DateTime.UtcNow.Date;
        var dataUtc = DateTime.SpecifyKind(data, DateTimeKind.Utc);
        var saldo = new SaldoDiario { Data = dataUtc, TotalCreditos = 100 };
        var cachedJson = System.Text.Json.JsonSerializer.Serialize(saldo);
        
        _cacheMock.Setup(c => c.GetAsync($"saldo_{dataUtc:yyyy-MM-dd}", default))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(cachedJson));

        // Act
        var result = await _service.GetSaldoDiarioAsync(data);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCreditos.Should().Be(100);
        _repositoryMock.Verify(r => r.GetByDataAsync(It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task GetSaldoDiarioAsync_DeveBuscarNoDbESalvarCache_QuandoNaoExistirNoCache()
    {
        // Arrange
        var data = DateTime.UtcNow.Date;
        var dataUtc = DateTime.SpecifyKind(data, DateTimeKind.Utc);
        var saldo = new SaldoDiario { Data = dataUtc, TotalCreditos = 200 };
        
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[]?)null);
        _repositoryMock.Setup(r => r.GetByDataAsync(dataUtc))
            .ReturnsAsync(saldo);

        // Act
        var result = await _service.GetSaldoDiarioAsync(data);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCreditos.Should().Be(200);
        _cacheMock.Verify(c => c.SetAsync(
            $"saldo_{dataUtc:yyyy-MM-dd}", 
            It.IsAny<byte[]>(), 
            It.IsAny<DistributedCacheEntryOptions>(), 
            default), Times.Once);
    }

    [Fact]
    public async Task AtualizarSaldoAsync_DeveRemoverDoCache()
    {
        // Arrange
        var data = DateTime.UtcNow.Date;
        var dataUtc = DateTime.SpecifyKind(data, DateTimeKind.Utc);
        _repositoryMock.Setup(r => r.GetByDataAsync(dataUtc))
            .ReturnsAsync(new SaldoDiario { Data = dataUtc });

        // Act
        await _service.AtualizarSaldoAsync(data, 100, "Credito");

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync($"saldo_{dataUtc:yyyy-MM-dd}", default), Times.Once);
    }
}
