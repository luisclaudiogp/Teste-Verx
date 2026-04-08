using Consolidado.API.Consumers;
using Consolidado.API.Data;
using Consolidado.Domain.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Contracts;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Consolidado.UnitTests.Consumers;

public class LancamentoCriadoConsumerTests
{
    private readonly ConsolidadoDbContext _dbContext;

    public LancamentoCriadoConsumerTests()
    {
        var options = new DbContextOptionsBuilder<ConsolidadoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ConsolidadoDbContext(options);
    }

    [Fact]
    public async Task Consume_ComDebito_DeveDeduzirSaldoCorretamente()
    {
        var consumer = new LancamentoCriadoConsumer(_dbContext);
        var mockContext = new Mock<ConsumeContext<LancamentoCriadoEvent>>();

        var eventData = new LancamentoCriadoEvent
        {
            Id = Guid.NewGuid(),
            Valor = 150m,
            Tipo = "Debito",
            Data = DateTime.UtcNow
        };
        mockContext.Setup(c => c.Message).Returns(eventData);

        await consumer.Consume(mockContext.Object);

        var saldo = await _dbContext.Saldos.FirstOrDefaultAsync(x => x.Data == eventData.Data.Date);
        saldo.Should().NotBeNull();
        saldo!.TotalDebitos.Should().Be(150m);
        saldo.TotalCreditos.Should().Be(0m);
        saldo.SaldoFinal.Should().Be(-150m);
    }

    [Fact]
    public async Task Consume_VariosEventos_DeveAgruparNoMesmoDia()
    {
        var consumer = new LancamentoCriadoConsumer(_dbContext);
        var date = DateTime.UtcNow;

        var e1 = new LancamentoCriadoEvent { Valor = 200m, Tipo = "Credito", Data = date };
        var e2 = new LancamentoCriadoEvent { Valor = 50m, Tipo = "Debito", Data = date };

        var m1 = new Mock<ConsumeContext<LancamentoCriadoEvent>>();
        m1.Setup(c => c.Message).Returns(e1);
        var m2 = new Mock<ConsumeContext<LancamentoCriadoEvent>>();
        m2.Setup(c => c.Message).Returns(e2);

        await consumer.Consume(m1.Object);
        await consumer.Consume(m2.Object);

        var saldo = await _dbContext.Saldos.FirstOrDefaultAsync(x => x.Data == date.Date);
        saldo.Should().NotBeNull();
        saldo!.TotalCreditos.Should().Be(200m);
        saldo.TotalDebitos.Should().Be(50m);
        saldo.SaldoFinal.Should().Be(150m);
    }
}
