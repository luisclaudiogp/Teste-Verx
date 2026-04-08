using Consolidado.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using Shared.Contracts.Services;

namespace Consolidado.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
    }

    [Fact]
    public async Task GenerateToken_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var mockResponse = new { access_token = "mock_token" };
        _authServiceMock.Setup(s => s.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((true, mockResponse, null!, 200));

        var controller = new AuthController(_authServiceMock.Object);

        // Act
        var result = await controller.GenerateToken("user", "pass");

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(mockResponse);
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GenerateToken_DeveRetornarErro_QuandoFalha()
    {
        // Arrange
        _authServiceMock.Setup(s => s.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((false, null!, "Erro", 401));

        var controller = new AuthController(_authServiceMock.Object);

        // Act
        var result = await controller.GenerateToken("user", "pass");

        // Assert
        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(401);
    }
}
