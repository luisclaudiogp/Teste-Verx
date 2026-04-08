using System.Net;
using System.Threading.Tasks;
using Lancamentos.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using Shared.Contracts.Services;

namespace Lancamentos.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task GenerateToken_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var mockResponse = new { access_token = "mock_token" };
        _authServiceMock.Setup(s => s.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((true, mockResponse, null, 200));

        // Act
        var result = await _controller.GenerateToken("user", "pass");

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(mockResponse);
    }

    [Fact]
    public async Task GenerateToken_DeveRetornarErro_QuandoFalhaNoServico()
    {
        // Arrange
        _authServiceMock.Setup(s => s.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((false, null, "Erro no Keycloak", 401));

        // Act
        var result = await _controller.GenerateToken("user", "pass");

        // Assert
        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(401);
    }
}
