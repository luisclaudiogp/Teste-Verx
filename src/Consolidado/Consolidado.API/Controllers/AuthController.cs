using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Services;

namespace Consolidado.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateToken([FromQuery] string username, [FromQuery] string password)
    {
        var (success, content, errorMessage, statusCode) = await _authService.GenerateTokenAsync(username, password);
        
        if (success)
        {
            return Ok(content);
        }
        
        return StatusCode(statusCode, new 
        { 
            message = "Erro ao autenticar no Keycloak", 
            details = errorMessage
        });
    }
}
