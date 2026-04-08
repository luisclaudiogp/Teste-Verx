using Consolidado.API.Data;
using Consolidado.API.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;
using System;
using System.Threading.Tasks;

namespace Consolidado.API.Consumers;

public class LancamentoCriadoConsumer : IConsumer<LancamentoCriadoEvent>
{
    private readonly ConsolidadoDbContext _dbContext;

    public LancamentoCriadoConsumer(ConsolidadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<LancamentoCriadoEvent> context)
    {
        var evento = context.Message;
        var dataTruncada = DateTime.SpecifyKind(evento.Data.Date, DateTimeKind.Utc);

        var saldo = await _dbContext.Saldos.FirstOrDefaultAsync(x => x.Data == dataTruncada);
        
        if (saldo == null)
        {
            saldo = new SaldoDiario { Data = dataTruncada };
            _dbContext.Saldos.Add(saldo);
        }

        if (evento.Tipo.ToLower() == "debito")
        {
            saldo.TotalDebitos += evento.Valor;
        }
        else
        {
            saldo.TotalCreditos += evento.Valor;
        }

        await _dbContext.SaveChangesAsync();
    }
}
