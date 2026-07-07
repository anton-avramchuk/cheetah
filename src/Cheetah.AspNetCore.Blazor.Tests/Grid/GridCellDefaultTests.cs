using Cheetah.AspNetCore.Blazor.Grid;

namespace Cheetah.AspNetCore.Blazor.Tests.Grid;

public class GridCellDefaultTests
{
    [Fact]
    public void Null_IsMutedDash()
        => GridCellDefault.Describe(null).ShouldBe((GridCellKind.Muted, "—"));

    [Fact]
    public void True_IsBoolTrueDa()
        => GridCellDefault.Describe(true).ShouldBe((GridCellKind.BoolTrue, "Да"));

    [Fact]
    public void False_IsBoolFalseNet()
        => GridCellDefault.Describe(false).ShouldBe((GridCellKind.BoolFalse, "Нет"));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyString_IsMutedDash(string s)
        => GridCellDefault.Describe(s).ShouldBe((GridCellKind.Muted, "—"));

    [Fact]
    public void NonEmptyString_IsText()
        => GridCellDefault.Describe("Anna").ShouldBe((GridCellKind.Text, "Anna"));

    [Fact]
    public void DateOnlyMidnight_IsDateWithoutTime()
        => GridCellDefault.Describe(new DateTime(2026, 7, 7)).ShouldBe((GridCellKind.Date, "07.07.2026"));

    [Fact]
    public void DateWithTime_IncludesTime()
        => GridCellDefault.Describe(new DateTime(2026, 7, 7, 14, 8, 0)).ShouldBe((GridCellKind.Date, "07.07.2026 14:08"));

    [Fact]
    public void DateOnly_IsFormatted()
        => GridCellDefault.Describe(new DateOnly(2026, 7, 7)).ShouldBe((GridCellKind.Date, "07.07.2026"));

    [Fact]
    public void Number_IsText()
        => GridCellDefault.Describe(42).ShouldBe((GridCellKind.Text, "42"));
}
