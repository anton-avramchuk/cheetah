using Cheetah.Core.Extensions.Common;
using Shouldly;

namespace Cheetah.Core.Tests.Extensions.Common;

public class ComparableExtensionsTests
{
    [Fact]
    public void IsBetween_WithIntegerInRange_ShouldReturnTrue()
    {
        // Arrange
        int value = 5;

        // Act
        var result = value.IsBetween(1, 10);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBetween_WithIntegerAtMinimumBoundary_ShouldReturnTrue()
    {
        // Arrange
        int value = 1;

        // Act
        var result = value.IsBetween(1, 10);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBetween_WithIntegerAtMaximumBoundary_ShouldReturnTrue()
    {
        // Arrange
        int value = 10;

        // Act
        var result = value.IsBetween(1, 10);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBetween_WithIntegerBelowRange_ShouldReturnFalse()
    {
        // Arrange
        int value = 0;

        // Act
        var result = value.IsBetween(1, 10);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsBetween_WithIntegerAboveRange_ShouldReturnFalse()
    {
        // Arrange
        int value = 11;

        // Act
        var result = value.IsBetween(1, 10);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsBetween_WithDoubleInRange_ShouldReturnTrue()
    {
        // Arrange
        double value = 5.5;

        // Act
        var result = value.IsBetween(1.0, 10.0);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBetween_WithStringInRange_ShouldReturnTrue()
    {
        // Arrange
        string value = "cat";

        // Act
        var result = value.IsBetween("apple", "dog");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBetween_WithStringOutOfRange_ShouldReturnFalse()
    {
        // Arrange
        string value = "zebra";

        // Act
        var result = value.IsBetween("apple", "dog");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsBetween_WithDateTimeInRange_ShouldReturnTrue()
    {
        // Arrange
        var value = new DateTime(2023, 6, 15);
        var min = new DateTime(2023, 1, 1);
        var max = new DateTime(2023, 12, 31);

        // Act
        var result = value.IsBetween(min, max);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBetween_WithNegativeNumbers_ShouldWork()
    {
        // Arrange
        int value = -5;

        // Act
        var result = value.IsBetween(-10, 0);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBetween_WithSameMinAndMax_ShouldReturnTrueOnlyForExactMatch()
    {
        // Arrange
        int value1 = 5;
        int value2 = 4;

        // Act
        var result1 = value1.IsBetween(5, 5);
        var result2 = value2.IsBetween(5, 5);

        // Assert
        result1.ShouldBeTrue();
        result2.ShouldBeFalse();
    }
}
