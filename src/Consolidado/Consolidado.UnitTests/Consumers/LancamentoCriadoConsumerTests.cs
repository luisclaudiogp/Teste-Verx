using Consolidado.Application.Consumers;
using Consolidado.Application.Services;
using FluentAssertions;
using MassTransit;
using Moq;
using Shared.Contracts;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Consolidado.UnitTests.Consumers;

public class LancamentoCriadoConsumerTests
{
    private readonly Mock<ISaldoService> _saldoServiceMock;
    private readonly LancamentoCriadoConsumer _consumer;

    public LancamentoCriadoConsumerTests()
    {
        _saldoServiceMock = new Mock<ISaldoService>();
        _consumer = new LancamentoCriadoConsumer(_saldoServiceMock.Object);
    }

    [Fact]
    public async Task Consume_DeveChamarAtualizarSaldo_QuandoReceberEvento()
    {
        // Arrange
        var mockContext = new Mock<ConsumeContext<LancamentoCriadoEvent>>();
        var eventData = new LancamentoCriadoEvent
        {
            Id = Guid.NewGuid(),
            Valor = 150m,
            Tipo = "Debito",
            Data = DateTime.UtcNow
        };
        mockContext.Setup(c => c.Message).Returns(eventData);

        // Act
        await _consumer.Consume(mockContext.Object);

        // Assert
        _saldoServiceMock.Verify(s => s.AtualizarSaldoAsync(
            eventData.Data, 
            eventData.Valor, 
            eventData.Tipo), 
            Times.Once);
    }
}

