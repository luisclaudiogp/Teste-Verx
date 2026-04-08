using Lancamentos.API.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Lancamentos.API.Data;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class LancamentosDbContext : DbContext
{
    public LancamentosDbContext(DbContextOptions<LancamentosDbContext> options) : base(options)
    {
    }

    public DbSet<Lancamento> Lancamentos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Lancamento>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Valor).HasPrecision(18, 2);
            b.Property(x => x.Tipo).HasMaxLength(20);
        });

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
