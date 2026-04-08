using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consolidado.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateToken([FromQuery] string username, [FromQuery] string password)
    {
        var client = _httpClientFactory.CreateClient();
        
        var keycloakUrl = _configuration["Keycloak:TokenEndpoint"]; 
        if (string.IsNullOrEmpty(keycloakUrl))
        {
            return StatusCode(500, "Configuração Keycloak:TokenEndpoint está faltando.");
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
            return Ok(content);
        }
        
        var errorContent = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, new 
        { 
            message = "Erro ao autenticar no Keycloak", 
            details = errorContent,
            keycloak_url = keycloakUrl 
        });
    }
}
