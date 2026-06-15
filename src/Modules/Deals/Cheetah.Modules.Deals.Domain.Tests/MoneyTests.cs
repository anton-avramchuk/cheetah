using Cheetah.Modules.Deals.Shared;
using Shouldly;

namespace Cheetah.Modules.Deals.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Ctor_NormalizesCurrencyToUpper()
        => new Money(100m, "usd").Currency.ShouldBe("USD");

    [Fact]
    public void Ctor_NegativeAmount_Throws()
        => Should.Throw<ArgumentOutOfRangeException>(() => new Money(-1m, "USD"));

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("")]
    [InlineData("  ")]
    public void Ctor_InvalidCurrency_Throws(string currency)
        => Should.Throw<ArgumentException>(() => new Money(1m, currency));

    [Fact]
    public void Equality_ByValue()
        => new Money(10m, "EUR").ShouldBe(new Money(10m, "eur"));
}
