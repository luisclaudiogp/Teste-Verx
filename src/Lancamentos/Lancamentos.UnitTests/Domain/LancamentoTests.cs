using System;
using Xunit;
using Lancamentos.Domain.Entities;

namespace Lancamentos.UnitTests.Domain;

public class LancamentoTests
{
    [Fact]
    public void CriarDebito_ComValorValido_DeveRetornarLancamento()
    {
        decimal valor = 100.50m;

        var lancamento = Lancamento.CriarDebito(valor);

        Assert.NotNull(lancamento);
        Assert.Equal(valor, lancamento.Valor);
        Assert.Equal("Debito", lancamento.Tipo);
        Assert.NotEqual(Guid.Empty, lancamento.Id);
    }

    [Fact]
    public void CriarCredito_ComValorValido_DeveRetornarLancamento()
    {
        decimal valor = 250m;

        var lancamento = Lancamento.CriarCredito(valor);

        Assert.NotNull(lancamento);
        Assert.Equal(valor, lancamento.Valor);
        Assert.Equal("Credito", lancamento.Tipo);
    }

    [Fact]
    public void CriarDebito_ComValorInvalido_DeveLancarExcecao()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => Lancamento.CriarDebito(-10));
    }
}
