using System;

using System.Diagnostics.CodeAnalysis;

namespace Shared.Contracts;

[ExcludeFromCodeCoverage]
public class LancamentoCriadoEvent
{
    public Guid Id { get; set; }
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime Data { get; set; }
}
