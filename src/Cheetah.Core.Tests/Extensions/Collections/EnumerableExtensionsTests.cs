using Cheetah.Core.Extensions.Collections;
using Shouldly;

namespace Cheetah.Core.Tests.Extensions.Collections;

public class EnumerableExtensionsTests
{
    [Fact]
    public void JoinAsString_WithStrings_ShouldJoinWithSeparator()
    {
        // Arrange
        var strings = new[] { "apple", "banana", "cherry" };

        // Act
        var result = strings.JoinAsString(", ");

        // Assert
        result.ShouldBe("apple, banana, cherry");
    }

    [Fact]
    public void JoinAsString_WithEmptyCollection_ShouldReturnEmptyString()
    {
        // Arrange
        var strings = Array.Empty<string>();

        // Act
        var result = strings.JoinAsString(", ");

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void JoinAsString_WithSingleItem_ShouldReturnItemWithoutSeparator()
    {
        // Arrange
        var strings = new[] { "single" };

        // Act
        var result = strings.JoinAsString(", ");

        // Assert
        result.ShouldBe("single");
    }

    [Fact]
    public void JoinAsString_Generic_WithIntegers_ShouldJoinWithSeparator()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = numbers.JoinAsString(" - ");

        // Assert
        result.ShouldBe("1 - 2 - 3 - 4 - 5");
    }

    [Fact]
    public void JoinAsString_Generic_WithCustomObjects_ShouldJoinWithToString()
    {
        // Arrange
        var objects = new[]
        {
            new TestObject { Name = "First" },
            new TestObject { Name = "Second" }
        };

        // Act
        var result = objects.JoinAsString(" | ");

        // Assert
        result.ShouldBe("First | Second");
    }

    [Fact]
    public void JoinAsString_WithEmptySeparator_ShouldConcatenateWithoutSeparator()
    {
        // Arrange
        var strings = new[] { "a", "b", "c" };

        // Act
        var result = strings.JoinAsString("");

        // Assert
        result.ShouldBe("abc");
    }

    [Fact]
    public void WhereIf_WhenConditionIsTrue_ShouldApplyFilter()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = numbers.WhereIf(true, x => x > 3);

        // Assert
        result.ShouldBe(new[] { 4, 5 });
    }

    [Fact]
    public void WhereIf_WhenConditionIsFalse_ShouldNotApplyFilter()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = numbers.WhereIf(false, x => x > 3);

        // Assert
        result.ShouldBe(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void WhereIf_WithIndexPredicate_WhenConditionIsTrue_ShouldApplyFilter()
    {
        // Arrange
        var numbers = new[] { 10, 20, 30, 40, 50 };

        // Act
        var result = numbers.WhereIf(true, (x, index) => index % 2 == 0);

        // Assert
        result.ShouldBe(new[] { 10, 30, 50 });
    }

    [Fact]
    public void WhereIf_WithIndexPredicate_WhenConditionIsFalse_ShouldNotApplyFilter()
    {
        // Arrange
        var numbers = new[] { 10, 20, 30, 40, 50 };

        // Act
        var result = numbers.WhereIf(false, (x, index) => index % 2 == 0);

        // Assert
        result.ShouldBe(new[] { 10, 20, 30, 40, 50 });
    }

    [Fact]
    public void WhereIf_ChainedMultipleTimes_ShouldWorkCorrectly()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // Act
        var result = numbers
            .WhereIf(true, x => x > 2)
            .WhereIf(false, x => x > 8)
            .WhereIf(true, x => x < 8);

        // Assert
        result.ShouldBe(new[] { 3, 4, 5, 6, 7 });
    }

    [Fact]
    public void WhereIf_WithEmptyCollection_ShouldReturnEmpty()
    {
        // Arrange
        var numbers = Array.Empty<int>();

        // Act
        var result = numbers.WhereIf(true, x => x > 0);

        // Assert
        result.ShouldBeEmpty();
    }

    // Helper class for testing
    private class TestObject
    {
        public string Name { get; set; } = string.Empty;

        public override string ToString() => Name;
    }
}
