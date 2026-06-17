namespace Cheetah.Modules.SalesDocuments.Shared;

/// <summary>
/// Денежная сумма с валютой (ISO-4217). Самодостаточный value object (равенство по значению через
/// record) — не наследует <c>Cheetah.Core.Domain.ValueObject</c>, чтобы Shared зависел только на Core.
/// Локальная копия (как в <c>Cheetah.Modules.Deals.Shared</c>); кандидат на вынос в общий
/// <c>Cheetah.Core.Domain</c> при унификации мультивалютности (см. план sales-documents.md §1.2).
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }

    /// <summary>Код валюты ISO-4217 (3 символа, верхний регистр).</summary>
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO-4217 code.", nameof(currency));

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public static Money Zero(string currency) => new(0m, currency);

    public override string ToString() => $"{Amount:0.##} {Currency}";
}
