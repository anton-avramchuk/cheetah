using System.Text;
using Cheetah.Core.Extensions.Common;
using FluentAssertions;

namespace Cheetah.Core.Tests.Extensions.Common;

public class StringExtensionsTests
{
    #region EnsureEndsWith / EnsureStartsWith

    [Fact]
    public void EnsureEndsWith_WhenStringDoesNotEndWithChar_ShouldAddChar()
    {
        // Arrange
        var str = "test";

        // Act
        var result = str.EnsureEndsWith('/');

        // Assert
        result.Should().Be("test/");
    }

    [Fact]
    public void EnsureEndsWith_WhenStringEndsWithChar_ShouldNotAddChar()
    {
        // Arrange
        var str = "test/";

        // Act
        var result = str.EnsureEndsWith('/');

        // Assert
        result.Should().Be("test/");
    }

    [Fact]
    public void EnsureStartsWith_WhenStringDoesNotStartWithChar_ShouldAddChar()
    {
        // Arrange
        var str = "test";

        // Act
        var result = str.EnsureStartsWith('/');

        // Assert
        result.Should().Be("/test");
    }

    [Fact]
    public void EnsureStartsWith_WhenStringStartsWithChar_ShouldNotAddChar()
    {
        // Arrange
        var str = "/test";

        // Act
        var result = str.EnsureStartsWith('/');

        // Assert
        result.Should().Be("/test");
    }

    #endregion

    #region IsNullOrEmpty / IsNullOrWhiteSpace

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("test", false)]
    public void IsNullOrEmpty_ShouldReturnCorrectValue(string? str, bool expected)
    {
        // Act
        var result = str.IsNullOrEmpty();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("test", false)]
    public void IsNullOrWhiteSpace_ShouldReturnCorrectValue(string? str, bool expected)
    {
        // Act
        var result = str.IsNullOrWhiteSpace();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Left / Right

    [Fact]
    public void Left_ShouldReturnLeftPart()
    {
        // Arrange
        var str = "Hello World";

        // Act
        var result = str.Left(5);

        // Assert
        result.Should().Be("Hello");
    }

    [Fact]
    public void Left_WhenLengthExceedsStringLength_ShouldThrowException()
    {
        // Arrange
        var str = "test";

        // Act
        var act = () => str.Left(10);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Right_ShouldReturnRightPart()
    {
        // Arrange
        var str = "Hello World";

        // Act
        var result = str.Right(5);

        // Assert
        result.Should().Be("World");
    }

    [Fact]
    public void Right_WhenLengthExceedsStringLength_ShouldThrowException()
    {
        // Arrange
        var str = "test";

        // Act
        var act = () => str.Right(10);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region NormalizeLineEndings

    [Fact]
    public void NormalizeLineEndings_ShouldConvertAllLineEndingsToEnvironmentNewLine()
    {
        // Arrange
        var str = "Line1\r\nLine2\rLine3\nLine4";

        // Act
        var result = str.NormalizeLineEndings();

        // Assert
        var expected = $"Line1{Environment.NewLine}Line2{Environment.NewLine}Line3{Environment.NewLine}Line4";
        result.Should().Be(expected);
    }

    #endregion

    #region NthIndexOf

    [Fact]
    public void NthIndexOf_ShouldReturnIndexOfNthOccurrence()
    {
        // Arrange
        var str = "a,b,c,d,e";

        // Act
        var result = str.NthIndexOf(',', 3);

        // Assert
        result.Should().Be(5); // Position of third comma
    }

    [Fact]
    public void NthIndexOf_WhenOccurrenceNotFound_ShouldReturnMinusOne()
    {
        // Arrange
        var str = "a,b,c";

        // Act
        var result = str.NthIndexOf(',', 5);

        // Assert
        result.Should().Be(-1);
    }

    #endregion

    #region RemovePostFix / RemovePreFix

    [Fact]
    public void RemovePostFix_WhenEndsWithPostfix_ShouldRemoveIt()
    {
        // Arrange
        var str = "TestController";

        // Act
        var result = str.RemovePostFix("Controller");

        // Assert
        result.Should().Be("Test");
    }

    [Fact]
    public void RemovePostFix_WhenDoesNotEndWithPostfix_ShouldReturnOriginal()
    {
        // Arrange
        var str = "TestService";

        // Act
        var result = str.RemovePostFix("Controller");

        // Assert
        result.Should().Be("TestService");
    }

    [Fact]
    public void RemovePostFix_WithMultiplePostfixes_ShouldRemoveFirstMatch()
    {
        // Arrange
        var str = "TestAppService";

        // Act
        var result = str.RemovePostFix("Controller", "AppService", "Service");

        // Assert
        result.Should().Be("Test");
    }

    [Fact]
    public void RemovePreFix_WhenStartsWithPrefix_ShouldRemoveIt()
    {
        // Arrange
        var str = "ITestService";

        // Act
        var result = str.RemovePreFix("I");

        // Assert
        result.Should().Be("TestService");
    }

    [Fact]
    public void RemovePreFix_WhenDoesNotStartWithPrefix_ShouldReturnOriginal()
    {
        // Arrange
        var str = "TestService";

        // Act
        var result = str.RemovePreFix("I");

        // Assert
        result.Should().Be("TestService");
    }

    #endregion

    #region ReplaceFirst

    [Fact]
    public void ReplaceFirst_ShouldReplaceOnlyFirstOccurrence()
    {
        // Arrange
        var str = "hello world, hello universe";

        // Act
        var result = str.ReplaceFirst("hello", "hi");

        // Assert
        result.Should().Be("hi world, hello universe");
    }

    [Fact]
    public void ReplaceFirst_WhenSearchNotFound_ShouldReturnOriginal()
    {
        // Arrange
        var str = "hello world";

        // Act
        var result = str.ReplaceFirst("goodbye", "hi");

        // Assert
        result.Should().Be("hello world");
    }

    #endregion

    #region Split / SplitToLines

    [Fact]
    public void Split_WithStringSeparator_ShouldSplitCorrectly()
    {
        // Arrange
        var str = "one::two::three";

        // Act
        var result = str.Split("::");

        // Assert
        result.Should().Equal("one", "two", "three");
    }

    [Fact]
    public void SplitToLines_ShouldSplitByNewLine()
    {
        // Arrange
        var str = $"Line1{Environment.NewLine}Line2{Environment.NewLine}Line3";

        // Act
        var result = str.SplitToLines();

        // Assert
        result.Should().Equal("Line1", "Line2", "Line3");
    }

    #endregion

    #region ToCamelCase / ToPascalCase

    [Theory]
    [InlineData("PascalCase", "pascalCase")]
    [InlineData("SomeValue", "someValue")]
    [InlineData("A", "a")]
    public void ToCamelCase_ShouldConvertToCamelCase(string input, string expected)
    {
        // Act
        var result = input.ToCamelCase();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToCamelCase_WithAbbreviations_ShouldConvertCorrectly()
    {
        // Arrange
        var str = "XYZ";

        // Act
        var result = str.ToCamelCase(handleAbbreviations: true);

        // Assert
        result.Should().Be("xyz");
    }

    [Theory]
    [InlineData("camelCase", "CamelCase")]
    [InlineData("someValue", "SomeValue")]
    [InlineData("a", "A")]
    public void ToPascalCase_ShouldConvertToPascalCase(string input, string expected)
    {
        // Act
        var result = input.ToPascalCase();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region ToSentenceCase / ToKebabCase / ToSnakeCase

    [Fact]
    public void ToSentenceCase_ShouldConvertToSentenceCase()
    {
        // Arrange
        var str = "ThisIsSampleSentence";

        // Act
        var result = str.ToSentenceCase();

        // Assert
        result.Should().Be("This is sample sentence");
    }

    [Fact]
    public void ToKebabCase_ShouldConvertToKebabCase()
    {
        // Arrange
        var str = "ThisIsKebabCase";

        // Act
        var result = str.ToKebabCase();

        // Assert
        result.Should().Be("this-is-kebab-case");
    }

    [Fact]
    public void ToSnakeCase_ShouldConvertToSnakeCase()
    {
        // Arrange
        var str = "ThisIsSnakeCase";

        // Act
        var result = str.ToSnakeCase();

        // Assert
        result.Should().Be("this_is_snake_case");
    }

    [Fact]
    public void ToSnakeCase_WithNumbers_ShouldWork()
    {
        // Arrange
        var str = "Test123Value";

        // Act
        var result = str.ToSnakeCase();

        // Assert
        result.Should().Be("test123value");
    }

    #endregion

    #region ToEnum

    [Fact]
    public void ToEnum_ShouldConvertStringToEnum()
    {
        // Arrange
        var str = "Sunday";

        // Act
        var result = str.ToEnum<DayOfWeek>();

        // Assert
        result.Should().Be(DayOfWeek.Sunday);
    }

    [Fact]
    public void ToEnum_WithIgnoreCase_ShouldWork()
    {
        // Arrange
        var str = "sunday";

        // Act
        var result = str.ToEnum<DayOfWeek>(ignoreCase: true);

        // Assert
        result.Should().Be(DayOfWeek.Sunday);
    }

    #endregion

    #region ToMd5

    [Fact]
    public void ToMd5_ShouldReturnMd5Hash()
    {
        // Arrange
        var str = "test";

        // Act
        var result = str.ToMd5();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(32); // MD5 hash is 32 characters in hex
    }

    [Fact]
    public void ToMd5_SameString_ShouldReturnSameHash()
    {
        // Arrange
        var str = "test";

        // Act
        var hash1 = str.ToMd5();
        var hash2 = str.ToMd5();

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion

    #region Truncate

    [Fact]
    public void Truncate_WhenStringIsShorter_ShouldReturnOriginal()
    {
        // Arrange
        var str = "short";

        // Act
        var result = str.Truncate(10);

        // Assert
        result.Should().Be("short");
    }

    [Fact]
    public void Truncate_WhenStringIsLonger_ShouldTruncate()
    {
        // Arrange
        var str = "This is a long string";

        // Act
        var result = str.Truncate(10);

        // Assert
        result.Should().Be("This is a ");
    }

    [Fact]
    public void Truncate_WithNull_ShouldReturnNull()
    {
        // Arrange
        string? str = null;

        // Act
        var result = str!.Truncate(10);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TruncateFromBeginning_ShouldTruncateFromStart()
    {
        // Arrange
        var str = "This is a long string";

        // Act
        var result = str.TruncateFromBeginning(6);

        // Assert
        result.Should().Be("string");
    }

    [Fact]
    public void TruncateWithPostfix_ShouldAddPostfix()
    {
        // Arrange
        var str = "This is a long string";

        // Act
        var result = str.TruncateWithPostfix(10);

        // Assert
        result.Should().Be("This is...");
        result!.Length.Should().Be(10);
    }

    [Fact]
    public void TruncateWithPostfix_WithCustomPostfix_ShouldUseCustomPostfix()
    {
        // Arrange
        var str = "This is a long string";

        // Act
        var result = str.TruncateWithPostfix(10, "---");

        // Assert
        result.Should().Be("This is---");
    }

    #endregion

    #region GetBytes

    [Fact]
    public void GetBytes_ShouldConvertToByteArray()
    {
        // Arrange
        var str = "test";

        // Act
        var result = str.GetBytes();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("test"));
    }

    [Fact]
    public void GetBytes_WithEncoding_ShouldUseSpecifiedEncoding()
    {
        // Arrange
        var str = "test";

        // Act
        var result = str.GetBytes(Encoding.ASCII);

        // Assert
        result.Should().BeEquivalentTo(Encoding.ASCII.GetBytes("test"));
    }

    #endregion
}
