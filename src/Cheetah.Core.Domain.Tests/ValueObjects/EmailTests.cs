using Cheetah.Core.Domain.ValueObjects;
using Shouldly;

namespace Cheetah.Core.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("user.name+tag@sub.domain.org")]
    [InlineData("a@b.io")]
    [InlineData("123@456.ru")]
    public void Create_ValidEmail_ShouldSucceed(string input)
    {
        var email = Email.Create(input);

        email.ShouldNotBeNull();
        email.Value.ShouldBe(input.Trim().ToLowerInvariant());
    }

    [Fact]
    public void Create_TrimsAndLowercasesInput()
    {
        var email = Email.Create("  User@Example.COM  ");

        email.Value.ShouldBe("user@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_ShouldThrowArgumentException(string input)
    {
        Should.Throw<ArgumentException>(() => Email.Create(input))
            .ParamName.ShouldBe("email");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain.com")]
    [InlineData("noatsign")]
    [InlineData("user@")]
    [InlineData("user @example.com")]
    [InlineData("user@ example.com")]
    [InlineData("user@nodot")]
    public void Create_InvalidFormat_ShouldThrowArgumentException(string input)
    {
        Should.Throw<ArgumentException>(() => Email.Create(input))
            .ParamName.ShouldBe("email");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var email = Email.Create("user@example.com");

        email.ToString().ShouldBe("user@example.com");
    }

    [Fact]
    public void ImplicitStringConversion_ReturnsValue()
    {
        var email = Email.Create("user@example.com");

        string value = email;

        value.ShouldBe("user@example.com");
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("USER@EXAMPLE.COM");

        a.ShouldBe(b);
        (a == b).ShouldBeTrue();
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("other@example.com");

        a.ShouldNotBe(b);
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ShouldBeEqual()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("USER@EXAMPLE.COM");

        a.GetHashCode().ShouldBe(b.GetHashCode());
    }
}
