using Cheetah.Backend.IdentityCore.Domain.ValueObjects;
using FluentAssertions;

namespace Cheetah.Backend.IdentityCore.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldReturnEmailValueObject()
    {
        // Arrange
        var validEmail = "test@example.com";

        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithValidEmail_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var validEmail = "Test@Example.COM";

        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act
        Action act = () => Email.Create(invalidEmail!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test")]
    [InlineData("test @example.com")]
    public void Create_WithInvalidEmailFormat_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act
        Action act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email format*");
    }

    [Fact]
    public void Create_WithEmailLongerThan256Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // 262 characters

        // Act
        Action act = () => Email.Create(longEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot exceed 256 characters*");
    }

    [Theory]
    [InlineData("user@domain.com")]
    [InlineData("user.name@domain.com")]
    [InlineData("user+tag@domain.co.uk")]
    [InlineData("123@456.com")]
    public void Create_WithVariousValidFormats_ShouldSucceed(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Fact]
    public void Normalize_ShouldReturnUpperCaseEmail()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        var normalized = email.Normalize();

        // Assert
        normalized.Should().Be("TEST@EXAMPLE.COM");
    }

    [Fact]
    public void TryCreate_WithValidEmail_ShouldReturnEmailValueObject()
    {
        // Arrange
        var validEmail = "test@example.com";

        // Act
        var email = Email.TryCreate(validEmail);

        // Assert
        email.Should().NotBeNull();
        email!.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData(null)]
    public void TryCreate_WithInvalidEmail_ShouldReturnNull(string? invalidEmail)
    {
        // Act
        var email = Email.TryCreate(invalidEmail);

        // Assert
        email.Should().BeNull();
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        string result = email;

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        (email1 != email2).Should().BeTrue();
    }
}
