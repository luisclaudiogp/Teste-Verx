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
    private LancamentosDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LancamentosDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        
        return new LancamentosDbContext(options);
    }

    [Fact]
    public async Task AddAsync_DeveAdicionarLancamentoAoContexto()
    {
        // Arrange
        var context = CreateContext();
        var repository = new LancamentoRepository(context);
        var lancamento = Lancamento.CriarCredito(100);

        // Act
        await repository.AddAsync(lancamento);
        await repository.SaveChangesAsync();

        // Assert
        var saved = await context.Lancamentos.FirstOrDefaultAsync(l => l.Id == lancamento.Id);
        saved.Should().NotBeNull();
        saved!.Valor.Should().Be(100);
    }

    [Fact]
    public async Task SaveChangesAsync_DevePersistirAlteracoes()
    {
        // Arrange
        var context = CreateContext();
        var repository = new LancamentoRepository(context);
        var lancamento = Lancamento.CriarDebito(50);
        await context.Lancamentos.AddAsync(lancamento);

        // Act
        await repository.SaveChangesAsync();

        // Assert
        var count = await context.Lancamentos.CountAsync();
        count.Should().Be(1);
    }
}
