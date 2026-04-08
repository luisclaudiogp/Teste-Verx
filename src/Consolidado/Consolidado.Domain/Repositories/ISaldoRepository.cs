using Consolidado.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Consolidado.Domain.Repositories;

public interface ISaldoRepository
{
    Task<SaldoDiario?> GetByDataAsync(DateTime data);
    Task AddAsync(SaldoDiario saldo);
    Task SaveChangesAsync();
}
