using Cheetah.Core.Extensions.Common;
using FluentAssertions;

namespace Cheetah.Core.Tests.Extensions.Common;

public class DateTimeExtensionsTests
{
    [Fact]
    public void ClearTime_ShouldRemoveTimeComponent()
    {
        // Arrange
        var dateTime = new DateTime(2023, 6, 15, 14, 30, 45, 123);

        // Act
        var result = dateTime.ClearTime();

        // Assert
        result.Year.Should().Be(2023);
        result.Month.Should().Be(6);
        result.Day.Should().Be(15);
        result.Hour.Should().Be(0);
        result.Minute.Should().Be(0);
        result.Second.Should().Be(0);
        result.Millisecond.Should().Be(0);
    }

    [Fact]
    public void ClearTime_WithMidnight_ShouldReturnSameDate()
    {
        // Arrange
        var dateTime = new DateTime(2023, 12, 25, 0, 0, 0, 0);

        // Act
        var result = dateTime.ClearTime();

        // Assert
        result.Should().Be(dateTime);
    }

    [Fact]
    public void ClearTime_WithEndOfDay_ShouldReturnStartOfDay()
    {
        // Arrange
        var dateTime = new DateTime(2023, 1, 1, 23, 59, 59, 999);

        // Act
        var result = dateTime.ClearTime();

        // Assert
        result.Should().Be(new DateTime(2023, 1, 1, 0, 0, 0, 0));
    }

    [Fact]
    public void ClearTime_ShouldPreserveDateKind()
    {
        // Arrange
        var utcDateTime = new DateTime(2023, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var result = utcDateTime.ClearTime();

        // Assert
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ClearTime_WithLeapYear_ShouldWork()
    {
        // Arrange
        var dateTime = new DateTime(2020, 2, 29, 12, 30, 45);

        // Act
        var result = dateTime.ClearTime();

        // Assert
        result.Should().Be(new DateTime(2020, 2, 29, 0, 0, 0));
    }
}
