using Lancamentos.Domain.Entities;
using Lancamentos.Domain.Repositories;
using Lancamentos.Infrastructure.Data;
using System.Threading.Tasks;

namespace Lancamentos.Infrastructure.Repositories;

public class LancamentoRepository : ILancamentoRepository
{
    private readonly LancamentosDbContext _context;

    public LancamentoRepository(LancamentosDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Lancamento lancamento)
    {
        await _context.Lancamentos.AddAsync(lancamento);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
