using System;
using System.Threading.Tasks;
using Lancamentos.Domain.Entities;
using Lancamentos.Infrastructure.Data;
using Lancamentos.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace Lancamentos.UnitTests.Infrastructure;

public class LancamentoRepositoryTests
{
    private readonly LancamentosDbContext _context;
    private readonly LancamentoRepository _repository;

    public LancamentoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LancamentosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new LancamentosDbContext(options);
        _repository = new LancamentoRepository(_context);
    }

    [Fact]
    public async Task AddAsync_DevePersistirLancamento()
    {
        // Arrange
        var lancamento = Lancamento.CriarCredito(100);

        // Act
        await _repository.AddAsync(lancamento);
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _context.Lancamentos.FindAsync(lancamento.Id);
        result.Should().NotBeNull();
        result!.Valor.Should().Be(100);
        result.Tipo.Should().Be("Credito");
    }
}
