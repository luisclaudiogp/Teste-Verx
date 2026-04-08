using System;
using System.Threading.Tasks;
using Lancamentos.Application.Services;
using Lancamentos.Domain.Entities;
using Lancamentos.Domain.Repositories;
using MassTransit;
using Moq;
using Shared.Contracts;
using Xunit;
using FluentAssertions;

namespace Lancamentos.UnitTests.Application;

public class LancamentoServiceTests
{
    private readonly Mock<ILancamentoRepository> _repositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly LancamentoService _service;

    public LancamentoServiceTests()
    {
        _repositoryMock = new Mock<ILancamentoRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _service = new LancamentoService(_repositoryMock.Object, _publishEndpointMock.Object);
    }

    [Theory]
    [InlineData(100, "Debito")]
    [InlineData(50, "Credito")]
    public async Task CriarLancamentoAsync_DeveSalvarEPublicarEvento(decimal valor, string tipo)
    {

        await _service.CriarLancamentoAsync(valor, tipo);


        _repositoryMock.Verify(r => r.AddAsync(It.Is<Lancamento>(l => 
            l.Valor == valor && l.Tipo == tipo)), Times.Once);
        
        _publishEndpointMock.Verify(p => p.Publish(It.Is<LancamentoCriadoEvent>(e => 
            e.Valor == valor && e.Tipo == tipo), default), Times.Once);

        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
