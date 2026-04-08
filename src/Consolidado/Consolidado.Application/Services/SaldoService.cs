using Consolidado.Domain.Entities;
using Consolidado.Domain.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Threading.Tasks;

namespace Consolidado.Application.Services;

public interface ISaldoService
{
    Task<SaldoDiario?> GetSaldoDiarioAsync(DateTime data);
    Task AtualizarSaldoAsync(DateTime data, decimal valor, string tipo);
}

public class SaldoService : ISaldoService
{
    private readonly ISaldoRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<SaldoService> _logger;

    public SaldoService(ISaldoRepository repository, IDistributedCache cache, ILogger<SaldoService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<SaldoDiario?> GetSaldoDiarioAsync(DateTime data)
    {
        var dataUtc = DateTime.SpecifyKind(data.Date, DateTimeKind.Utc);
        string cacheKey = $"saldo_{dataUtc:yyyy-MM-dd}";

        // Tenta obter do Cache (Redis)
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Cache Hit para saldo de {Data}", dataUtc);
            return JsonSerializer.Deserialize<SaldoDiario>(cachedData);
        }

        // Se não houver no cache, busca no DB
        _logger.LogInformation("Cache Miss para saldo de {Data}, buscando no banco.", dataUtc);
        var saldo = await _repository.GetByDataAsync(dataUtc);

        if (saldo != null)
        {
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(saldo), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
        }

        return saldo;
    }

    public async Task AtualizarSaldoAsync(DateTime data, decimal valor, string tipo)
    {
        var dataUtc = DateTime.SpecifyKind(data.Date, DateTimeKind.Utc);
        var saldo = await _repository.GetByDataAsync(dataUtc);

        if (saldo == null)
        {
            saldo = new SaldoDiario { Data = dataUtc };
            await _repository.AddAsync(saldo);
        }

        if (tipo.ToLower() == "debito")
        {
            saldo.TotalDebitos += valor;
        }
        else
        {
            saldo.TotalCreditos += valor;
        }

        await _repository.SaveChangesAsync();

        // Invalida o Cache após atualização para garantir consistência
        string cacheKey = $"saldo_{dataUtc:yyyy-MM-dd}";
        await _cache.RemoveAsync(cacheKey);
        _logger.LogInformation("Cache invalidado para {Data} após atualização.", dataUtc);
    }
}
