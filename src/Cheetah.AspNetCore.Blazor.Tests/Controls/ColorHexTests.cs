using Cheetah.AspNetCore.Blazor.Controls;

namespace Cheetah.AspNetCore.Blazor.Tests.Controls;

public class ColorHexTests
{
    [Theory]
    [InlineData("#6366f1")]
    [InlineData("#FFFFFF")]
    [InlineData("#000000")]
    [InlineData("#Ab12Cd")]
    public void IsValid_FullHex_True(string value)
        => ColorHex.IsValid(value).ShouldBeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("6366f1")]     // без #
    [InlineData("#6366f")]     // 5 цифр
    [InlineData("#6366f11")]   // 7 цифр
    [InlineData("#12345g")]    // не-hex символ
    [InlineData("#abc")]       // краткая форма невалидна как полный HEX
    public void IsValid_Invalid_False(string? value)
        => ColorHex.IsValid(value).ShouldBeFalse();

    [Theory]
    [InlineData("#6366F1", "#6366f1")]      // нижний регистр
    [InlineData("6366f1", "#6366f1")]       // добавляет #
    [InlineData("  #6366F1  ", "#6366f1")]  // тримминг
    [InlineData("#ABC", "#aabbcc")]         // #rgb → #rrggbb
    [InlineData("f00", "#ff0000")]          // краткая без # → полная
    public void Normalize_Canonicalizes(string input, string expected)
        => ColorHex.Normalize(input).ShouldBe(expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_Empty_Null(string? input)
        => ColorHex.Normalize(input).ShouldBeNull();

    [Fact]
    public void Normalize_Then_IsValid_ForFullHex()
        => ColorHex.IsValid(ColorHex.Normalize("6366F1")).ShouldBeTrue();
}
