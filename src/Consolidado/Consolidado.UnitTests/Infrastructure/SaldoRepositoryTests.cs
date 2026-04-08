using System;
using System.Threading.Tasks;
using Consolidado.Domain.Entities;
using Consolidado.Infrastructure.Data;
using Consolidado.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace Consolidado.UnitTests.Infrastructure;

public class SaldoRepositoryTests
{
    private readonly ConsolidadoDbContext _context;
    private readonly SaldoRepository _repository;

    public SaldoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ConsolidadoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ConsolidadoDbContext(options);
        _repository = new SaldoRepository(_context);
    }

    [Fact]
    public async Task GetByDataAsync_DeveRetornarSaldo_QuandoExiste()
    {
        // Arrange
        var data = DateTime.UtcNow.Date;
        var saldo = new SaldoDiario { Data = data, TotalCreditos = 500 };
        await _context.Saldos.AddAsync(saldo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDataAsync(data);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().Be(data);
        result.TotalCreditos.Should().Be(500);
    }

    [Fact]
    public async Task AddAsync_DeveAdicionarNovoSaldo()
    {
        // Arrange
        var saldo = new SaldoDiario { Data = DateTime.UtcNow.Date, TotalDebitos = 200 };

        // Act
        await _repository.AddAsync(saldo);
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _context.Saldos.FindAsync(saldo.Data);
        result.Should().NotBeNull();
        result!.TotalDebitos.Should().Be(200);
    }
}
