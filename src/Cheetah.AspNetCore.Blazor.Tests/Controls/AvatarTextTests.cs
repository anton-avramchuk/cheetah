using Cheetah.AspNetCore.Blazor.Controls;

namespace Cheetah.AspNetCore.Blazor.Tests.Controls;

public class AvatarTextTests
{
    [Theory]
    [InlineData("Anna Smirnova", "AS")]
    [InlineData("Иван Петров", "ИП")]
    [InlineData("Иван Петров Сергеевич", "ИС")] // первое + последнее слово
    [InlineData("ООО «СеверЛес»", "О«")]
    [InlineData("Иван", "И")]
    [InlineData("anna", "A")]
    public void Initials_ReturnsUpperFirstAndLast(string name, string expected)
        => AvatarText.Initials(name).ShouldBe(expected);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Initials_Empty_ReturnsQuestionMark(string? name)
        => AvatarText.Initials(name).ShouldBe("?");

    [Fact]
    public void ColorIndex_IsStableForSameName()
        => AvatarText.ColorIndex("Anna Smirnova").ShouldBe(AvatarText.ColorIndex("Anna Smirnova"));

    [Fact]
    public void ColorIndex_IsWithinPalette()
    {
        foreach (var name in new[] { "Anna", "Ivan", "Oleg", "Пётр", "ООО Ромашка", "x" })
        {
            var i = AvatarText.ColorIndex(name);
            i.ShouldBeInRange(0, 7);
        }
    }

    [Fact]
    public void ColorIndex_SpreadsAcrossPalette()
    {
        // Хэш должен раскидывать имена по палитре, а не давать всем один цвет.
        var names = new[] { "Anna Smirnova", "Ivan Petrov", "Oleg Kozlov", "Пётр Сидоров",
                            "Мария Дронова", "Дмитрий Л.", "ООО Ромашка", "АО НеваТелеком" };
        var distinct = names.Select(AvatarText.ColorIndex).Distinct().Count();
        distinct.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void Background_IsGradient_AndStable()
    {
        var bg = AvatarText.Background("Anna Smirnova");
        bg.ShouldContain("linear-gradient");
        bg.ShouldBe(AvatarText.Background("Anna Smirnova"));
    }
}
