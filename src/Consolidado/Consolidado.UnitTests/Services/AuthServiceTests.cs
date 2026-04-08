using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Shared.Contracts.Services;
using Xunit;
using FluentAssertions;

namespace Consolidado.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public AuthServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Keycloak:TokenEndpoint"]).Returns("http://keycloak/token");
    }

    [Fact]
    public async Task GenerateTokenAsync_DeveRetornarSucesso_QuandoKeycloakRespondeOk()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { access_token = "mock_token" })
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new AuthService(_httpClientFactoryMock.Object, _configurationMock.Object);

        // Act
        var result = await service.GenerateTokenAsync("user", "pass");

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Content.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateTokenAsync_DeveRetornarErro_QuandoConfiguracaoFaltando()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:TokenEndpoint"]).Returns(string.Empty);
        var service = new AuthService(_httpClientFactoryMock.Object, _configurationMock.Object);

        // Act
        var result = await service.GenerateTokenAsync("user", "pass");

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.ErrorMessage.Should().Contain("faltando");
    }

    [Fact]
    public async Task GenerateTokenAsync_DeveRetornarErro_QuandoKeycloakRetornaErro()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Invalid credentials")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new AuthService(_httpClientFactoryMock.Object, _configurationMock.Object);

        // Act
        var result = await service.GenerateTokenAsync("user", "pass");

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.ErrorMessage.Should().Be("Invalid credentials");
    }
}
