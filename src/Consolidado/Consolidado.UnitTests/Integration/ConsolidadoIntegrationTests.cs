using System;
using System.Threading.Tasks;
using Consolidado.API.Controllers;
using Consolidado.Application.Services;
using Consolidado.Domain.Entities;
using Consolidado.Infrastructure.Data;
using Consolidado.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Consolidado.UnitTests.Integration;

public class ConsolidadoIntegrationTests
{
    private readonly ConsolidadoDbContext _context;
    private readonly SaldoService _service;
    private readonly ConsolidadoController _controller;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<SaldoService>> _loggerMock;

    public ConsolidadoIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ConsolidadoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ConsolidadoDbContext(options);
        var repository = new SaldoRepository(_context);
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<SaldoService>>();
        
        _service = new SaldoService(repository, _cacheMock.Object, _loggerMock.Object);
        _controller = new ConsolidadoController(_service);
    }

    [Fact]
    public async Task GetSaldo_FluxoIntegrado_DeveRetornarValoresDoBanco()
    {
        // Arrange
        var data = DateTime.UtcNow.Date;
        var saldo = new SaldoDiario { Data = DateTime.SpecifyKind(data, DateTimeKind.Utc), TotalCreditos = 1000, TotalDebitos = 200 };
        await _context.Saldos.AddAsync(saldo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetSaldoDiario(data);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resultValue = okResult.Value.Should().BeAssignableTo<SaldoDiario>().Subject;
        resultValue!.SaldoFinal.Should().Be(800);
    }
}
