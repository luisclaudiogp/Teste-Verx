using Consolidado.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

namespace Consolidado.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ConsolidadoController : ControllerBase
{
    private readonly ISaldoService _service;

    public ConsolidadoController(ISaldoService service)
    {
        _service = service;
    }

    [HttpGet("{data}")]
    public async Task<IActionResult> GetSaldoDiario(DateTime data)
    {
        var saldo = await _service.GetSaldoDiarioAsync(data);

        if (saldo == null)
            return Ok(new { Data = data.Date, TotalCreditos = 0, TotalDebitos = 0, SaldoFinal = 0 });

        return Ok(saldo);
    }
}
