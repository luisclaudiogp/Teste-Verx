using System;
using System.Threading.Tasks;

namespace Lancamentos.Application.Services;

public interface ILancamentoService
{
    Task CriarLancamentoAsync(decimal valor, string tipo);
}

public class LancamentoService : ILancamentoService
{
    private readonly Lancamentos.Domain.Repositories.ILancamentoRepository _repository;
    private readonly MassTransit.IPublishEndpoint _publishEndpoint;

    public LancamentoService(
        Lancamentos.Domain.Repositories.ILancamentoRepository repository,
        MassTransit.IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task CriarLancamentoAsync(decimal valor, string tipo)
    {
        var lancamento = tipo.ToLower() == "debito"
            ? Lancamentos.Domain.Entities.Lancamento.CriarDebito(valor)
            : Lancamentos.Domain.Entities.Lancamento.CriarCredito(valor);

        await _repository.AddAsync(lancamento);

        var evento = new Shared.Contracts.LancamentoCriadoEvent
        {
            Id = lancamento.Id,
            Valor = lancamento.Valor,
            Tipo = lancamento.Tipo,
            Data = lancamento.DataLancamento
        };

        await _publishEndpoint.Publish(evento);
        await _repository.SaveChangesAsync();
    }
}
