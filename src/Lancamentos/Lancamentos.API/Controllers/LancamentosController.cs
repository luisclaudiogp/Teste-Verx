using Lancamentos.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

namespace Lancamentos.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LancamentosController : ControllerBase
{
    private readonly ILancamentoService _service;

    public LancamentosController(ILancamentoService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarLancamentoRequest request)
    {
        await _service.CriarLancamentoAsync(request.Valor, request.Tipo);
        return Ok(new { Message = "Lançamento recebido com sucesso." });
    }
}

public record CriarLancamentoRequest(decimal Valor, string Tipo);
