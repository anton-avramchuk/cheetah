using System.Linq.Expressions;
using Cheetah.Contracts.Requests;
using Cheetah.Core.Grid;
using Shouldly;

namespace Cheetah.Core.Grid.Tests;

/// <summary>
/// Перевод фильтра из строки запроса в выражение по сущности. Проверяется без базы: выражение
/// прогоняется по обычному списку, поэтому тесты видят именно логику отбора, а не работу провайдера.
///
/// Это единственное, что стоит между тем, что нажал пользователь, и SQL: ошибка здесь не падает, а
/// возвращает не тот список — то есть замечают её позже всех.
/// </summary>
public class GridFilterExpressionsTests
{
    private sealed record Candidate(
        string FullName,
        string? City,
        int Rating,
        IReadOnlyList<string> Skills,
        string? Email = null,
        string? Phone = null);

    private static readonly Candidate[] People =
    [
        new("Илья Ремизов", "Москва", 5, ["C#", "SQL"], Email: "ilya@example.com"),
        new("Марина Стах", "Казань", 3, ["Go"], Phone: "+79000000000"),
        new("Пётр Гусев", null, 4, [], Email: null, Phone: null),
        new("Ольга Литвин", "Москва", 2, ["c#"]),
    ];

    private static IReadOnlyList<Candidate> Filter(FilterDescriptor filter, LambdaExpression? projection = null)
    {
        var expression = GridFilterExpressions.Build<Candidate>(filter, projection);

        return expression is null
            ? People
            : [.. People.AsQueryable().Where(expression)];
    }

    private static FilterDescriptor Leaf(string field, string op, object? value = null, bool ignoreCase = true)
        => new() { Field = field, Operator = op, Value = value, IgnoreCase = ignoreCase };

    // ── Коллекции ────────────────────────────────────────────────────────────────────────────
    //
    // Это и есть добавленное поведение. Раньше фильтр по списку молча пропадал: строковый Contains на
    // коллекции не собирался, выражение получалось null, и список возвращался целиком — то есть отбор
    // «по навыку» показывал всех.

    [Fact]
    public void Equality_OnCollection_MeansAnyElementEquals()
    {
        var found = Filter(Leaf("skills", "eq", "C#"));

        found.Select(p => p.FullName).ShouldBe(["Илья Ремизов", "Ольга Литвин"]);
    }

    [Fact]
    public void Equality_OnCollection_IgnoresCaseWhenAsked()
    {
        Filter(Leaf("skills", "eq", "c#")).Count.ShouldBe(2);
        Filter(Leaf("skills", "eq", "c#", ignoreCase: false)).ShouldHaveSingleItem()
            .FullName.ShouldBe("Ольга Литвин");
    }

    [Fact]
    public void Contains_OnCollection_MatchesElementSubstring()
        => Filter(Leaf("skills", "contains", "q")).ShouldHaveSingleItem()
            .FullName.ShouldBe("Илья Ремизов");

    /// <summary>
    /// «Не равно» для коллекции — «ни один элемент не равен», а не «есть хоть один другой»: второе
    /// отобрало бы почти всех и выглядело бы как сломанный фильтр.
    /// </summary>
    [Fact]
    public void Inequality_OnCollection_MeansNoElementEquals()
    {
        var found = Filter(Leaf("skills", "neq", "C#"));

        found.Select(p => p.FullName).ShouldBe(["Марина Стах", "Пётр Гусев"]);
    }

    [Fact]
    public void Emptiness_OnCollection_IsAboutTheCollectionItself()
    {
        Filter(Leaf("skills", "isempty")).ShouldHaveSingleItem().FullName.ShouldBe("Пётр Гусев");
        Filter(Leaf("skills", "isnotempty")).Count.ShouldBe(3);
    }

    [Fact]
    public void UnsupportedOperator_OnCollection_FiltersNothing()
        => Filter(Leaf("skills", "gte", "C#")).Count.ShouldBe(People.Length);

    // ── Прежнее поведение: оно тоже не было покрыто ──────────────────────────────────────────

    [Fact]
    public void Equality_OnString_IgnoresCaseByDefault()
        => Filter(Leaf("city", "eq", "москва")).Count.ShouldBe(2);

    [Fact]
    public void Contains_OnString_MatchesSubstring()
        => Filter(Leaf("fullName", "contains", "гусев")).ShouldHaveSingleItem()
            .FullName.ShouldBe("Пётр Гусев");

    [Fact]
    public void Comparison_OnNumber_Works()
        => Filter(Leaf("rating", "gte", 4)).Count.ShouldBe(2);

    [Fact]
    public void NullChecks_Work()
    {
        Filter(Leaf("city", "isnull")).ShouldHaveSingleItem().FullName.ShouldBe("Пётр Гусев");
        Filter(Leaf("city", "isnotnull")).Count.ShouldBe(3);
    }

    /// <summary>
    /// Составной фильтр — то, чем выражается «есть хоть какой-то контакт»: почта ИЛИ телефон. Ради
    /// этого случая отдельного оператора не нужно.
    /// </summary>
    [Fact]
    public void CompositeOr_ExpressesHasAnyContact()
    {
        var found = Filter(new FilterDescriptor
        {
            Logic = "or",
            Filters = [Leaf("email", "isnotnull"), Leaf("phone", "isnotnull")],
        });

        found.Select(p => p.FullName).ShouldBe(["Илья Ремизов", "Марина Стах"]);
    }

    [Fact]
    public void CompositeAnd_NarrowsTheSelection()
    {
        var found = Filter(new FilterDescriptor
        {
            Logic = "and",
            Filters = [Leaf("city", "eq", "Москва"), Leaf("skills", "eq", "C#")],
        });

        found.Count.ShouldBe(2);
    }

    [Fact]
    public void UnknownField_FiltersNothing()
        => Filter(Leaf("middleName", "eq", "Иванович")).Count.ShouldBe(People.Length);

    [Fact]
    public void EmptyDescriptor_FiltersNothing()
        => Filter(new FilterDescriptor()).Count.ShouldBe(People.Length);

    [Fact]
    public void NullDescriptor_GivesNoExpression()
        => GridFilterExpressions.Build<Candidate>(null).ShouldBeNull();

    /// <summary>
    /// Имя поля приходит из ViewModel, а выражение строится по сущности: путь разворачивается через
    /// проекционную лямбду. Здесь «town» — это City сущности.
    /// </summary>
    [Fact]
    public void FieldName_IsResolvedThroughTheProjection()
    {
        Expression<Func<Candidate, object>> projection = candidate => new { town = candidate.City };

        var found = Filter(Leaf("town", "eq", "Казань"), projection);

        found.ShouldHaveSingleItem().FullName.ShouldBe("Марина Стах");
    }
}
