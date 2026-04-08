using System;
using System.Threading.Tasks;
using Lancamentos.API.Controllers;
using Lancamentos.Application.Services;
using Lancamentos.Infrastructure.Data;
using Lancamentos.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Moq;
using Xunit;
using FluentAssertions;

namespace Lancamentos.UnitTests.Integration;

public class LancamentosIntegrationTests
{
    private readonly LancamentosDbContext _context;
    private readonly LancamentoService _service;
    private readonly LancamentosController _controller;
    private readonly Mock<IPublishEndpoint> _publishMock;

    public LancamentosIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<LancamentosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new LancamentosDbContext(options);
        var repository = new LancamentoRepository(_context);
        _publishMock = new Mock<IPublishEndpoint>();
        
        _service = new LancamentoService(repository, _publishMock.Object);
        _controller = new LancamentosController(_service);
    }

    [Fact]
    public async Task CriarLancamento_FluxoIntegrado_DeveSalvarNoBanco()
    {
        // Arrange
        var request = new CriarLancamentoRequest(500m, "Credito");

        // Act
        var result = await _controller.Criar(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var lancamento = await _context.Lancamentos.AnyAsync(l => l.Valor == 500m);
        lancamento.Should().BeTrue();
        _publishMock.Verify(p => p.Publish(It.IsAny<Shared.Contracts.LancamentoCriadoEvent>(), default), Times.Once);
    }
}
