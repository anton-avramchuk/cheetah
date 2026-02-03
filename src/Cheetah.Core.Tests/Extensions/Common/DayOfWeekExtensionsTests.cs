using Cheetah.Core.Extensions.Common;
using Shouldly;

namespace Cheetah.Core.Tests.Extensions.Common;

public class DayOfWeekExtensionsTests
{
    [Theory]
    [InlineData(DayOfWeek.Saturday, true)]
    [InlineData(DayOfWeek.Sunday, true)]
    [InlineData(DayOfWeek.Monday, false)]
    [InlineData(DayOfWeek.Tuesday, false)]
    [InlineData(DayOfWeek.Wednesday, false)]
    [InlineData(DayOfWeek.Thursday, false)]
    [InlineData(DayOfWeek.Friday, false)]
    public void IsWeekend_ShouldReturnCorrectValue(DayOfWeek dayOfWeek, bool expected)
    {
        // Act
        var result = dayOfWeek.IsWeekend();

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, true)]
    [InlineData(DayOfWeek.Tuesday, true)]
    [InlineData(DayOfWeek.Wednesday, true)]
    [InlineData(DayOfWeek.Thursday, true)]
    [InlineData(DayOfWeek.Friday, true)]
    [InlineData(DayOfWeek.Saturday, false)]
    [InlineData(DayOfWeek.Sunday, false)]
    public void IsWeekday_ShouldReturnCorrectValue(DayOfWeek dayOfWeek, bool expected)
    {
        // Act
        var result = dayOfWeek.IsWeekday();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void IsWeekend_AndIsWeekday_ShouldBeOpposite()
    {
        // Arrange
        var allDays = Enum.GetValues<DayOfWeek>();

        // Act & Assert
        foreach (var day in allDays)
        {
            day.IsWeekend().ShouldBe(!day.IsWeekday());
        }
    }
}
