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
    public async Task CriarLancamentoAsync_DeveProcessarCreditoCorretamente()
    {
        await _service.CriarLancamentoAsync(200, "Credito");

        _repositoryMock.Verify(r => r.AddAsync(It.Is<Lancamentos.Domain.Entities.Lancamento>(l => l.Tipo == "Credito" && l.Valor == 200)), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(It.Is<Shared.Contracts.LancamentoCriadoEvent>(e => e.Tipo == "Credito" && e.Valor == 200), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CriarLancamentoAsync_DeveProcessarDebitoCorretamente()
    {
        await _service.CriarLancamentoAsync(50, "Debito");

        _repositoryMock.Verify(r => r.AddAsync(It.Is<Lancamentos.Domain.Entities.Lancamento>(l => l.Tipo == "Debito" && l.Valor == 50)), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(It.Is<Shared.Contracts.LancamentoCriadoEvent>(e => e.Tipo == "Debito" && e.Valor == 50), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CriarLancamentoAsync_DeveLancarExcecao_QuandoRepositorioFalha()
    {
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Lancamentos.Domain.Entities.Lancamento>()))
            .ThrowsAsync(new Exception("Erro de banco"));

        // Act & Assert
    }
    [Theory]
    [InlineData("debito")]
    [InlineData("DEBITO")]
    [InlineData("dEbItO")]
    [InlineData("credito")]
    [InlineData("CREDITO")]
    public async Task CriarLancamentoAsync_DeveSerInsensivelACaso(string tipo)
    {
        await _service.CriarLancamentoAsync(100, tipo);

        var expectedTipo = tipo.ToLower() == "debito" ? "Debito" : "Credito";
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Lancamentos.Domain.Entities.Lancamento>(l => l.Tipo == expectedTipo)), Times.Once);
    }

    [Fact]
    public async Task Dominio_DeveLancarExcecao_ParaValoresInvalidos()
    {
        Assert.Throws<ArgumentException>(() => Lancamentos.Domain.Entities.Lancamento.CriarDebito(0));
        Assert.Throws<ArgumentException>(() => Lancamentos.Domain.Entities.Lancamento.CriarDebito(-1));
        Assert.Throws<ArgumentException>(() => Lancamentos.Domain.Entities.Lancamento.CriarCredito(0));
        Assert.Throws<ArgumentException>(() => Lancamentos.Domain.Entities.Lancamento.CriarCredito(-1));
    }
}
