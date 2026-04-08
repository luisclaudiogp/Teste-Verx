using System;

namespace Lancamentos.API.Domain;

public class Lancamento
{
    public Guid Id { get; private set; }
    public decimal Valor { get; private set; }
    public string Tipo { get; private set; }
    public DateTime DataLancamento { get; private set; }

    private Lancamento() { Tipo = string.Empty; }

    private Lancamento(Guid id, decimal valor, string tipo, DateTime dataLancamento)
    {
        Id = id;
        Valor = valor;
        Tipo = tipo;
        DataLancamento = dataLancamento;
    }

    public static Lancamento CriarDebito(decimal valor)
    {
        if (valor <= 0) throw new ArgumentException("Valor deve ser positivo.", nameof(valor));
        return new Lancamento(Guid.NewGuid(), valor, "Debito", DateTime.UtcNow);
    }

    public static Lancamento CriarCredito(decimal valor)
    {
        if (valor <= 0) throw new ArgumentException("Valor deve ser positivo.", nameof(valor));
        return new Lancamento(Guid.NewGuid(), valor, "Credito", DateTime.UtcNow);
    }
}
