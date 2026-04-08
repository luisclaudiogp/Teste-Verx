using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Shared.Contracts.Services;

public interface IAuthService
{
    Task<(bool Success, object? Content, string? ErrorMessage, int StatusCode)> GenerateTokenAsync(string username, string password);
}

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<(bool Success, object? Content, string? ErrorMessage, int StatusCode)> GenerateTokenAsync(string username, string password)
    {
        var client = _httpClientFactory.CreateClient();
        
        var keycloakUrl = _configuration["Keycloak:TokenEndpoint"]; 
        if (string.IsNullOrEmpty(keycloakUrl))
        {
            return (false, null, "Configuração Keycloak:TokenEndpoint está faltando.", 500);
        }
        
        var request = new HttpRequestMessage(HttpMethod.Post, keycloakUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", _configuration["Keycloak:Audience"] ?? "verx-api" }, 
                { "grant_type", "password" },
                { "username", username },
                { "password", password }
            })
        };

        var response = await client.SendAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<object>();
            return (true, content, null, 200);
        }
        
        var errorContent = await response.Content.ReadAsStringAsync();
        return (false, null, errorContent, (int)response.StatusCode);
    }
}
