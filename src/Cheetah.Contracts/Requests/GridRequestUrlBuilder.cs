using System.Globalization;
using System.Web;

namespace Cheetah.Contracts.Requests;

/// <summary>
/// Builds query string URLs from GridRequest parameters.
///
/// Формат — тот же, что понимает разбор на стороне сервиса (Kendo/PrimeNG):
/// <c>?page=1&amp;pageSize=10&amp;sort[0][field]=Name&amp;sort[0][dir]=asc</c>
/// <c>&amp;filter[logic]=and&amp;filter[filters][0][field]=Name&amp;filter[filters][0][operator]=contains</c>.
///
/// Фильтр обходится рекурсивно: у составного условия корневая группа не имеет поля, и запись
/// только листа означала бы, что весь отбор не уехал, — беззвучно, потому что список в ответ
/// вернул бы всё подряд, а не ошибку.
/// </summary>
public static class GridRequestUrlBuilder
{
    public static string BuildUrl(string basePath, GridRequest? request)
    {
        if (request is null)
            return basePath;

        var queryParams = HttpUtility.ParseQueryString(string.Empty);

        queryParams["page"] = request.Page.ToString(CultureInfo.InvariantCulture);
        queryParams["pageSize"] = request.PageSize.ToString(CultureInfo.InvariantCulture);

        for (var i = 0; i < request.Sort.Count; i++)
        {
            var sort = request.Sort[i];
            if (!string.IsNullOrEmpty(sort.Field))
            {
                queryParams[$"sort[{i}][field]"] = sort.Field;
                queryParams[$"sort[{i}][dir]"] = sort.Dir ?? "asc";
            }
        }

        if (request.Filter is not null)
            AppendFilter(queryParams, "filter", request.Filter);

        var queryString = queryParams.ToString();
        return string.IsNullOrEmpty(queryString) ? basePath : $"{basePath}?{queryString}";
    }

    private static void AppendFilter(
        System.Collections.Specialized.NameValueCollection queryParams, string prefix, FilterDescriptor filter)
    {
        // Лист: поле, оператор и значение. Вложенные условия у листа не смотрим — разбор на
        // сервере, встретив поле, тоже останавливается на нём.
        if (!string.IsNullOrEmpty(filter.Field))
        {
            queryParams[$"{prefix}[field]"] = filter.Field;
            queryParams[$"{prefix}[operator]"] = filter.Operator ?? "eq";
            queryParams[$"{prefix}[value]"] = FormatValue(filter.Value);
            return;
        }

        // Пустая группа — это отсутствие отбора, а не отбор «ничего»: записав одну логику без
        // условий, мы получили бы на разборе фильтр, который ничему не соответствует.
        if (filter.Filters.Count == 0)
            return;

        queryParams[$"{prefix}[logic]"] = filter.Logic ?? "and";

        for (var i = 0; i < filter.Filters.Count; i++)
            AppendFilter(queryParams, $"{prefix}[filters][{i}]", filter.Filters[i]);
    }

    /// <summary>
    /// Значение строкой в том виде, в каком разбор на сервере узнает его тип обратно: инвариантная
    /// культура для чисел, ISO для дат, нижний регистр для булева. С локальной культурой дробное
    /// число уехало бы как «1,5» и вернулось строкой, а сравнение чисел стало бы сравнением строк.
    /// </summary>
    private static string FormatValue(object? value) => value switch
    {
        null => string.Empty,
        bool flag => flag ? "true" : "false",
        DateTimeOffset moment => moment.ToString("O", CultureInfo.InvariantCulture),
        DateTime moment => moment.ToString("O", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };
}
