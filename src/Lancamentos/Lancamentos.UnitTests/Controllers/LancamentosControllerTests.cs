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
        var request = new CriarLancamentoRequest(100, "Credito");
        _serviceMock.Setup(s => s.CriarLancamentoAsync(It.IsAny<decimal>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Criar(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        _serviceMock.Verify(s => s.CriarLancamentoAsync(100, "Credito"), Times.Once);
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoServicoFalha()
    {
        var request = new CriarLancamentoRequest(100, "Credito");
        _serviceMock.Setup(s => s.CriarLancamentoAsync(It.IsAny<decimal>(), It.IsAny<string>()))
            .ThrowsAsync(new System.Exception("Erro interno"));

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _controller.Criar(request));
    }
}
