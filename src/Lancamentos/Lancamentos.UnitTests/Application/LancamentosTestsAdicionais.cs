using System;
using System.Threading.Tasks;
using Lancamentos.Application.Services;
using Lancamentos.Domain.Repositories;
using MassTransit;
using Moq;
using Xunit;
using FluentAssertions;

namespace Lancamentos.UnitTests.Application;

public class LancamentosTestsAdicionais
{
    private readonly Mock<ILancamentoRepository> _repositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly LancamentoService _service;

    public LancamentosTestsAdicionais()
    {
        _repositoryMock = new Mock<ILancamentoRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _service = new LancamentoService(_repositoryMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task CriarLancamentoAsync_DeveLancarExcecao_QuandoRepositorioFalha()
    {
        // Arrange
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Lancamentos.Domain.Entities.Lancamento>()))
            .ThrowsAsync(new Exception("Erro de banco"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.CriarLancamentoAsync(100, "Credito"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task CriarLancamentoAsync_DeveLancarExcecao_QuandoValorInvalido(decimal valor)
    {
        // Act & Assert
        // Nota: Atualmente o domínio talvez não lance, vamos ver se o serviço trata
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarLancamentoAsync(valor, "Credito"));
    }
}
