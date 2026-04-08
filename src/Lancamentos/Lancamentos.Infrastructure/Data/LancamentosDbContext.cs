using Lancamentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace Lancamentos.Infrastructure.Data;

public class LancamentosDbContext : DbContext
{
    public LancamentosDbContext(DbContextOptions<LancamentosDbContext> options) : base(options) { }

    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lancamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valor).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DataLancamento).IsRequired();
        });

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
