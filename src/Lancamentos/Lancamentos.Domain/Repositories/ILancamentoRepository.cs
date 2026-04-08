using Lancamentos.Domain.Entities;
using System.Threading.Tasks;

namespace Lancamentos.Domain.Repositories;

public interface ILancamentoRepository
{
    Task AddAsync(Lancamento lancamento);
    Task SaveChangesAsync();
}
