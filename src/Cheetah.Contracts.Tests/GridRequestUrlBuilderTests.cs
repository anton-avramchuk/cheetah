using System.Collections.Specialized;
using System.Web;
using Cheetah.Contracts.Requests;
using Shouldly;

namespace Cheetah.Contracts.Tests;

/// <summary>
/// Сборка грид-запроса в строку запроса — обратная сторона разбора в
/// <c>Cheetah.AspNetCore.Contracts</c>: что здесь собрано, там должно разобраться обратно.
///
/// Проверяется въедливо потому, что поломка здесь беззвучна. Потерянный фильтр не даёт ни ошибки,
/// ни пустого ответа — список возвращает всё подряд, будто отбор и не задавали.
/// </summary>
public class GridRequestUrlBuilderTests
{
    [Fact]
    public void Keeps_the_path_when_there_is_no_request()
        => GridRequestUrlBuilder.BuildUrl("api/customers", null).ShouldBe("api/customers");

    [Fact]
    public void Passes_paging()
    {
        var query = Parse(new GridRequest { Page = 3, PageSize = 25 });

        query["page"].ShouldBe("3");
        query["pageSize"].ShouldBe("25");
    }

    [Fact]
    public void Passes_every_sort_in_order()
    {
        var query = Parse(new GridRequest
        {
            Sort =
            [
                new SortDescriptor { Field = "City", Dir = "desc" },
                new SortDescriptor { Field = "Name" },
            ],
        });

        query["sort[0][field]"].ShouldBe("City");
        query["sort[0][dir]"].ShouldBe("desc");
        query["sort[1][field]"].ShouldBe("Name");
        query["sort[1][dir]"].ShouldBe("asc");
    }

    [Fact]
    public void Passes_a_single_filter()
    {
        var query = Parse(new GridRequest
        {
            Filter = new FilterDescriptor { Field = "City", Operator = "eq", Value = "Москва" },
        });

        query["filter[field]"].ShouldBe("City");
        query["filter[operator]"].ShouldBe("eq");
        query["filter[value]"].ShouldBe("Москва");
    }

    /// <summary>
    /// Составной фильтр: у корневой группы нет поля, есть логика и вложенные условия. Раньше
    /// такой запрос уходил без фильтра вообще.
    /// </summary>
    [Fact]
    public void Passes_a_composite_filter_whole()
    {
        var query = Parse(new GridRequest
        {
            Filter = new FilterDescriptor
            {
                Logic = "and",
                Filters =
                [
                    new FilterDescriptor { Field = "Name", Operator = "contains", Value = "ромашка" },
                    new FilterDescriptor { Field = "IsActive", Operator = "eq", Value = true },
                ],
            },
        });

        query["filter[logic]"].ShouldBe("and");
        query["filter[filters][0][field]"].ShouldBe("Name");
        query["filter[filters][0][operator]"].ShouldBe("contains");
        query["filter[filters][0][value]"].ShouldBe("ромашка");
        query["filter[filters][1][field]"].ShouldBe("IsActive");
        query["filter[filters][1][value]"].ShouldBe("true");
    }

    [Fact]
    public void Passes_nested_groups()
    {
        var query = Parse(new GridRequest
        {
            Filter = new FilterDescriptor
            {
                Logic = "and",
                Filters =
                [
                    new FilterDescriptor { Field = "IsActive", Operator = "eq", Value = true },
                    new FilterDescriptor
                    {
                        Logic = "or",
                        Filters =
                        [
                            new FilterDescriptor { Field = "City", Operator = "eq", Value = "Москва" },
                            new FilterDescriptor { Field = "City", Operator = "eq", Value = "Казань" },
                        ],
                    },
                ],
            },
        });

        query["filter[filters][1][logic]"].ShouldBe("or");
        query["filter[filters][1][filters][0][value]"].ShouldBe("Москва");
        query["filter[filters][1][filters][1][value]"].ShouldBe("Казань");
    }

    /// <summary>
    /// Значения — в инвариантном виде. С локальной культурой дробное число уехало бы как «1,5»
    /// и на разборе стало бы строкой, а сравнение чисел — сравнением строк.
    /// </summary>
    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    [InlineData(1.5, "1.5")]
    [InlineData(42, "42")]
    public void Formats_values_invariantly(object value, string expected)
    {
        var query = Parse(new GridRequest
        {
            Filter = new FilterDescriptor { Field = "Amount", Operator = "eq", Value = value },
        });

        query["filter[value]"].ShouldBe(expected);
    }

    /// <summary>Пустая группа — это отсутствие отбора, а не отбор «ничего».</summary>
    [Fact]
    public void Skips_an_empty_group()
    {
        var query = Parse(new GridRequest { Filter = new FilterDescriptor { Logic = "and" } });

        query.AllKeys.ShouldNotContain(key => key!.StartsWith("filter", StringComparison.Ordinal));
    }

    private static NameValueCollection Parse(GridRequest request)
    {
        var url = GridRequestUrlBuilder.BuildUrl("api/items", request);

        url.ShouldStartWith("api/items?");

        return HttpUtility.ParseQueryString(url["api/items?".Length..]);
    }
}
