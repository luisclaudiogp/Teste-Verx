using System;

namespace Consolidado.API.Domain;

public class SaldoDiario
{
    public DateTime Data { get; set; }
    public decimal TotalCreditos { get; set; }
    public decimal TotalDebitos { get; set; }
    
    public decimal SaldoFinal => TotalCreditos - TotalDebitos;
}
