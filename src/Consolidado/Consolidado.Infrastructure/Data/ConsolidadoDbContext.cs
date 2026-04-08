using Consolidado.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Consolidado.Infrastructure.Data;

public class ConsolidadoDbContext : DbContext
{
    public ConsolidadoDbContext(DbContextOptions<ConsolidadoDbContext> options) : base(options) { }

    public DbSet<SaldoDiario> Saldos => Set<SaldoDiario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SaldoDiario>(entity =>
        {
            entity.HasKey(e => e.Data);
            entity.Property(e => e.TotalCreditos).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.TotalDebitos).IsRequired().HasPrecision(18, 2);
        });
    }
}
