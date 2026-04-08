using Consolidado.Application.Services;
using MassTransit;
using Shared.Contracts;
using System.Threading.Tasks;

namespace Consolidado.Application.Consumers;

public class LancamentoCriadoConsumer : IConsumer<LancamentoCriadoEvent>
{
    private readonly ISaldoService _saldoService;

    public LancamentoCriadoConsumer(ISaldoService saldoService)
    {
        _saldoService = saldoService;
    }

    public async Task Consume(ConsumeContext<LancamentoCriadoEvent> context)
    {
        var evento = context.Message;
        await _saldoService.AtualizarSaldoAsync(evento.Data, evento.Valor, evento.Tipo);
    }
}
