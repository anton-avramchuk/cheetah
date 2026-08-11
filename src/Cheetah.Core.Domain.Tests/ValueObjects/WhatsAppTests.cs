using Cheetah.Core.Domain.ValueObjects;
using Shouldly;

namespace Cheetah.Core.Domain.Tests.ValueObjects;

/// <summary>
/// Номер WhatsApp. По форме телефон, поэтому и приводится как телефон; отдельный тип от
/// <see cref="Phone"/> — потому что это другой способ связи, а не другое написание того же.
/// </summary>
public class WhatsAppTests
{
    [Theory]
    [InlineData("+7 900 123-45-67")]
    [InlineData("+7 (900) 123 45 67")]
    [InlineData("  +79001234567  ")]
    [InlineData("wa.me/+79001234567")]
    [InlineData("https://wa.me/+79001234567")]
    [InlineData("https://api.whatsapp.com/send?phone=+79001234567")]
    public void Create_AnyOfTheUsualForms_GivesTheSameNumber(string input)
        => WhatsApp.Create(input).Value.ShouldBe("+79001234567");

    /// <summary>Номер без плюса остаётся без плюса: страну домысливать не наше дело.</summary>
    [Fact]
    public void Create_KeepsAbsenceOfThePlus()
        => WhatsApp.Create("79001234567").Value.ShouldBe("79001234567");

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_ShouldThrowArgumentException(string input)
        => Should.Throw<ArgumentException>(() => WhatsApp.Create(input))
            .ParamName.ShouldBe("whatsApp");

    [Theory]
    [InlineData("123456")]                 // меньше семи цифр
    [InlineData("+1234567890123456")]      // больше пятнадцати
    [InlineData("+0123456789")]            // не может начинаться с нуля
    [InlineData("написать в ватсап")]      // не номер вовсе
    public void Create_InvalidNumber_ShouldThrowArgumentException(string input)
        => Should.Throw<ArgumentException>(() => WhatsApp.Create(input))
            .ParamName.ShouldBe("whatsApp");

    [Fact]
    public void Equality_IsByValue()
        => WhatsApp.Create("+7 900 123-45-67").ShouldBe(WhatsApp.Create("wa.me/+79001234567"));

    /// <summary>
    /// Тот же номер в WhatsApp и в телефоне — разные значения: сравнивать их напрямую нельзя, иначе
    /// «есть телефон» начнёт означать «есть WhatsApp», чего никто не обещал.
    /// </summary>
    [Fact]
    public void WhatsApp_IsNotAPhone()
        => WhatsApp.Create("+79001234567").Equals(Phone.Create("+79001234567")).ShouldBeFalse();

    [Fact]
    public void ImplicitConversion_GivesTheNumber()
    {
        string number = WhatsApp.Create("+79001234567");

        number.ShouldBe("+79001234567");
    }
}
