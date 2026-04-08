using Consolidado.Domain.Entities;
using Consolidado.Domain.Repositories;
using Consolidado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Consolidado.Infrastructure.Repositories;

public class SaldoRepository : ISaldoRepository
{
    private readonly ConsolidadoDbContext _context;

    public SaldoRepository(ConsolidadoDbContext context)
    {
        _context = context;
    }

    public async Task<SaldoDiario?> GetByDataAsync(DateTime data)
    {
        return await _context.Saldos.FirstOrDefaultAsync(x => x.Data == data);
    }

    public async Task AddAsync(SaldoDiario saldo)
    {
        await _context.Saldos.AddAsync(saldo);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
