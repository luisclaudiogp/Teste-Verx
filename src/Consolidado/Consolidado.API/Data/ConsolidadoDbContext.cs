using Consolidado.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace Consolidado.API.Data;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class ConsolidadoDbContext : DbContext
{
    public ConsolidadoDbContext(DbContextOptions<ConsolidadoDbContext> options) : base(options)
    {
    }

    public DbSet<SaldoDiario> Saldos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SaldoDiario>(b =>
        {
            b.HasKey(x => x.Data);
            b.Property(x => x.TotalCreditos).HasPrecision(18, 2);
            b.Property(x => x.TotalDebitos).HasPrecision(18, 2);
        });
    }
}
