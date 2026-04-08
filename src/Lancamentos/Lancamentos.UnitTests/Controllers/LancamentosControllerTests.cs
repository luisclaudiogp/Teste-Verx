using System.Threading.Tasks;
using Lancamentos.API.Controllers;
using Lancamentos.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace Lancamentos.UnitTests.Controllers;

public class LancamentosControllerTests
{
    private readonly Mock<ILancamentoService> _serviceMock;
    private readonly LancamentosController _controller;

    public LancamentosControllerTests()
    {
        _serviceMock = new Mock<ILancamentoService>();
        _controller = new LancamentosController(_serviceMock.Object);
    }

    [Fact]
    public async Task Criar_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var request = new CriarLancamentoRequest(100m, "Credito");

        // Act
        var result = await _controller.Criar(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.CriarLancamentoAsync(request.Valor, request.Tipo), Times.Once);
    }
}
