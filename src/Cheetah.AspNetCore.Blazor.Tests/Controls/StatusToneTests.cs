using Cheetah.AspNetCore.Blazor.Controls;

namespace Cheetah.AspNetCore.Blazor.Tests.Controls;

public class StatusToneTests
{
    [Theory]
    [InlineData("Активна", "success")]
    [InlineData("Активен", "success")]
    [InlineData("Открыта", "success")]
    [InlineData("Нанят", "success")]
    [InlineData("Оффер принят", "success")]
    [InlineData("Отказ", "danger")]
    [InlineData("Закрыта", "danger")]
    [InlineData("Отклонён", "danger")]
    [InlineData("Архив", "danger")]
    [InlineData("Приостановлена", "warning")]
    [InlineData("На паузе", "warning")]
    [InlineData("Новый", "info")]
    [InlineData("Скрининг", "info")]
    [InlineData("Интервью HR", "info")]
    [InlineData("Rejected", "danger")]
    [InlineData("Open", "success")]
    public void Classify_MapsKnownStatuses(string status, string expected)
        => StatusTone.Classify(status).ShouldBe(expected);

    [Fact]
    public void Classify_DangerWinsOverSuccess_ForNegatedActive()
        // «неактивна» содержит «актив» — danger должен проверяться первым.
        => StatusTone.Classify("Неактивна").ShouldBe("danger");

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("нечто неизвестное")]
    public void Classify_UnknownOrEmpty_IsSecondary(string? status)
        => StatusTone.Classify(status).ShouldBe("secondary");

    [Fact]
    public void ForBool_True_IsSuccessYes()
    {
        var (variant, label) = StatusTone.ForBool(true);
        variant.ShouldBe("success");
        label.ShouldBe("Да");
    }

    [Fact]
    public void ForBool_False_IsSecondaryNo()
    {
        var (variant, label) = StatusTone.ForBool(false);
        variant.ShouldBe("secondary");
        label.ShouldBe("Нет");
    }
}
