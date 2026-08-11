using Cheetah.Core.Domain.ValueObjects;
using Shouldly;

namespace Cheetah.Core.Domain.Tests.ValueObjects;

/// <summary>
/// Telegram username. Главное здесь — приведение: одно и то же имя приходит четырьмя видами, и без
/// канона в базе оказались бы четыре разных контакта одного человека, а поиск по нику не нашёл бы ни
/// одного.
/// </summary>
public class TelegramTests
{
    [Theory]
    [InlineData("durov")]
    [InlineData("@durov")]
    [InlineData("t.me/durov")]
    [InlineData("https://t.me/durov")]
    [InlineData("http://telegram.me/durov")]
    [InlineData("  @Durov  ")]
    public void Create_AnyOfTheUsualForms_GivesTheSameUsername(string input)
    {
        var telegram = Telegram.Create(input);

        telegram.Value.ShouldBe("durov");
    }

    /// <summary>Регистр не значим: сам Telegram «User» и «user» не различает.</summary>
    [Fact]
    public void Create_IgnoresCase()
        => Telegram.Create("@Ivan_Petrov").Value.ShouldBe("ivan_petrov");

    [Theory]
    [InlineData("user_1")]
    [InlineData("abcde")]
    [InlineData("a234567890123456789012345678901")]
    public void Create_ValidUsername_ShouldSucceed(string input)
        => Telegram.Create(input).Value.ShouldBe(input.ToLowerInvariant());

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_ShouldThrowArgumentException(string input)
        => Should.Throw<ArgumentException>(() => Telegram.Create(input))
            .ParamName.ShouldBe("telegram");

    [Theory]
    [InlineData("abcd")]                                // короче пяти знаков
    [InlineData("a23456789012345678901234567890123")]   // длиннее тридцати двух
    [InlineData("1user")]                               // начинается с цифры
    [InlineData("_user")]                               // начинается с подчёркивания
    [InlineData("иванов")]                              // не латиница
    [InlineData("user name")]                           // пробел внутри
    [InlineData("user-name")]                           // дефис
    public void Create_InvalidUsername_ShouldThrowArgumentException(string input)
        => Should.Throw<ArgumentException>(() => Telegram.Create(input))
            .ParamName.ShouldBe("telegram");

    [Fact]
    public void Equality_IsByValue()
        => Telegram.Create("@durov").ShouldBe(Telegram.Create("https://t.me/DUROV"));

    [Fact]
    public void ImplicitConversion_GivesTheUsername()
    {
        string username = Telegram.Create("@durov");

        username.ShouldBe("durov");
    }
}
