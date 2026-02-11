using System.Web;

namespace Cheetah.Contracts.Requests;

/// <summary>
/// Builds query string URLs from GridRequest parameters
/// </summary>
public static class GridRequestUrlBuilder
{
    public static string BuildUrl(string basePath, GridRequest? request)
    {
        if (request is null)
            return basePath;

        var queryParams = HttpUtility.ParseQueryString(string.Empty);

        queryParams["page"] = request.Page.ToString();
        queryParams["pageSize"] = request.PageSize.ToString();

        for (var i = 0; i < request.Sort.Count; i++)
        {
            var sort = request.Sort[i];
            if (!string.IsNullOrEmpty(sort.Field))
            {
                queryParams[$"sort[{i}][field]"] = sort.Field;
                queryParams[$"sort[{i}][dir]"] = sort.Dir ?? "asc";
            }
        }

        if (request.Filter is not null && !string.IsNullOrEmpty(request.Filter.Field))
        {
            queryParams["filter[field]"] = request.Filter.Field;
            queryParams["filter[operator]"] = request.Filter.Operator ?? "eq";
            queryParams["filter[value]"] = request.Filter.Value?.ToString() ?? string.Empty;
        }

        var queryString = queryParams.ToString();
        return string.IsNullOrEmpty(queryString) ? basePath : $"{basePath}?{queryString}";
    }
}
