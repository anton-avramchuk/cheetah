using Cheetah.Core.Domain.ValueObjects;
using Shouldly;

namespace Cheetah.Core.Domain.Tests.ValueObjects;

public class ColorTests
{
    [Theory]
    [InlineData("#aabbcc")]
    [InlineData("#AABBCC")]
    [InlineData("#aabbccdd")]
    [InlineData("#AABBCCDD")]
    [InlineData("#000000")]
    [InlineData("#ffffff")]
    [InlineData("#123456")]
    [InlineData("#12345678")]
    public void Create_ValidColor_ShouldSucceed(string input)
    {
        var color = Color.Create(input);

        color.ShouldNotBeNull();
        color.Value.ShouldBe(input.Trim().ToLowerInvariant());
    }

    [Fact]
    public void Create_TrimsAndLowercasesInput()
    {
        var color = Color.Create("  #AABBCC  ");

        color.Value.ShouldBe("#aabbcc");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_ShouldThrowArgumentException(string input)
    {
        Should.Throw<ArgumentException>(() => Color.Create(input))
            .ParamName.ShouldBe("color");
    }

    [Theory]
    [InlineData("aabbcc")]         // missing #
    [InlineData("#aabbc")]         // too short (5 hex digits)
    [InlineData("#aabbccd")]       // 7 hex digits
    [InlineData("#aabbccdde")]     // too long
    [InlineData("#gghhii")]        // invalid hex chars
    [InlineData("rgb(0,0,0)")]     // wrong format
    [InlineData("#")]
    public void Create_InvalidFormat_ShouldThrowArgumentException(string input)
    {
        Should.Throw<ArgumentException>(() => Color.Create(input))
            .ParamName.ShouldBe("color");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var color = Color.Create("#aabbcc");

        color.ToString().ShouldBe("#aabbcc");
    }

    [Fact]
    public void ImplicitStringConversion_ReturnsValue()
    {
        var color = Color.Create("#aabbcc");

        string value = color;

        value.ShouldBe("#aabbcc");
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var a = Color.Create("#aabbcc");
        var b = Color.Create("#AABBCC");

        a.ShouldBe(b);
        (a == b).ShouldBeTrue();
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var a = Color.Create("#aabbcc");
        var b = Color.Create("#112233");

        a.ShouldNotBe(b);
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ShouldBeEqual()
    {
        var a = Color.Create("#aabbcc");
        var b = Color.Create("#AABBCC");

        a.GetHashCode().ShouldBe(b.GetHashCode());
    }
}
