using System.Globalization;
using System.Text.RegularExpressions;
using Cheetah.Contracts.Requests;
using Microsoft.AspNetCore.Http;

namespace Cheetah.AspNetCore.Contracts.Extensions;

/// <summary>
/// Extension methods for binding GridRequest from HttpContext in Minimal API
/// </summary>
public static class GridRequestExtensions
{
    /// <summary>
    /// Binds GridRequest from query string.
    /// Supports Kendo UI format: ?page=1&amp;pageSize=10&amp;sort[0][field]=Name&amp;sort[0][dir]=asc
    /// </summary>
    public static TRequest BindGridRequest<TRequest>(this HttpContext context) where TRequest : GridRequest, new()
    {
        var request = new TRequest();
        var queryString = context.Request.Query;

        // Parse Page
        if (queryString.TryGetValue("page", out var pageValue) &&
            int.TryParse(pageValue.FirstOrDefault(), out var page))
        {
            request.Page = page;
        }

        // Parse PageSize
        if (queryString.TryGetValue("pageSize", out var pageSizeValue) &&
            int.TryParse(pageSizeValue.FirstOrDefault(), out var pageSize))
        {
            request.PageSize = pageSize;
        }

        // Parse Sort
        request.Sort = ParseSort(queryString);

        // Parse Filter
        request.Filter = ParseFilter(queryString, "filter");

        return request;
    }

    private static List<SortDescriptor> ParseSort(IQueryCollection queryString)
    {
        var sorts = new List<SortDescriptor>();
        var sortKeys = queryString.Keys.Where(k => k.StartsWith("sort[")).ToList();

        if (sortKeys.Count == 0)
            return sorts;

        var sortGroups = sortKeys
            .Select(k =>
            {
                var match = Regex.Match(k, @"sort\[(\d+)\](?:\.|\[)(\w+)\]?");
                if (match.Success)
                {
                    return new
                    {
                        Index = int.Parse(match.Groups[1].Value),
                        Property = match.Groups[2].Value,
                        Key = k
                    };
                }
                return null;
            })
            .Where(x => x != null)
            .GroupBy(x => x!.Index)
            .OrderBy(g => g.Key);

        foreach (var group in sortGroups)
        {
            var descriptor = new SortDescriptor();

            foreach (var item in group)
            {
                var value = queryString[item!.Key].FirstOrDefault();
                if (item.Property == "field")
                    descriptor.Field = value;
                else if (item.Property == "dir")
                    descriptor.Dir = value;
            }

            if (!string.IsNullOrEmpty(descriptor.Field))
                sorts.Add(descriptor);
        }

        return sorts;
    }

    private static FilterDescriptor? ParseFilter(IQueryCollection queryString, string prefix)
    {
        var filterKeys = queryString.Keys
            .Where(k => k.StartsWith(prefix + "[") || k.StartsWith(prefix + "."))
            .ToList();

        if (filterKeys.Count == 0)
            return null;

        var descriptor = new FilterDescriptor();

        // Parse logic
        var logicKey = $"{prefix}[logic]";
        var logicKeyDot = $"{prefix}.logic";
        if (queryString.TryGetValue(logicKey, out var logicValue) ||
            queryString.TryGetValue(logicKeyDot, out logicValue))
        {
            descriptor.Logic = logicValue.FirstOrDefault();
        }

        // Parse leaf filter (field, operator, value)
        var fieldKey = $"{prefix}[field]";
        var fieldKeyDot = $"{prefix}.field";
        if (queryString.TryGetValue(fieldKey, out var fieldValue) ||
            queryString.TryGetValue(fieldKeyDot, out fieldValue))
        {
            descriptor.Field = fieldValue.FirstOrDefault();

            var operatorKey = $"{prefix}[operator]";
            var operatorKeyDot = $"{prefix}.operator";
            if (queryString.TryGetValue(operatorKey, out var operatorValue) ||
                queryString.TryGetValue(operatorKeyDot, out operatorValue))
            {
                descriptor.Operator = operatorValue.FirstOrDefault();
            }

            var valueKey = $"{prefix}[value]";
            var valueKeyDot = $"{prefix}.value";
            if (queryString.TryGetValue(valueKey, out var value) ||
                queryString.TryGetValue(valueKeyDot, out value))
            {
                descriptor.Value = ParseValue(value.FirstOrDefault());
            }

            return descriptor;
        }

        // Parse nested filters
        var nestedFilterKeys = filterKeys
            .Where(k => k.Contains($"{prefix}[filters][") || k.Contains($"{prefix}.filters["))
            .ToList();

        if (nestedFilterKeys.Count > 0)
        {
            var indices = nestedFilterKeys
                .Select(k =>
                {
                    var matchBracket = Regex.Match(k, $@"{Regex.Escape(prefix)}\[filters\]\[(\d+)\]");
                    if (matchBracket.Success)
                        return int.Parse(matchBracket.Groups[1].Value);

                    var matchDot = Regex.Match(k, $@"{Regex.Escape(prefix)}\.filters\[(\d+)\]");
                    if (matchDot.Success)
                        return int.Parse(matchDot.Groups[1].Value);

                    return -1;
                })
                .Where(i => i >= 0)
                .Distinct()
                .OrderBy(i => i);

            foreach (var index in indices)
            {
                var nestedPrefixBracket = $"{prefix}[filters][{index}]";
                var nestedPrefixDot = $"{prefix}.filters[{index}]";
                var usesDotNotation = filterKeys.Any(k => k.StartsWith(nestedPrefixDot));
                var nestedPrefix = usesDotNotation ? nestedPrefixDot : nestedPrefixBracket;

                var nestedFilter = ParseFilter(queryString, nestedPrefix);
                if (nestedFilter != null)
                    descriptor.Filters.Add(nestedFilter);
            }
        }

        return descriptor.Filters.Count > 0 || !string.IsNullOrEmpty(descriptor.Field) ? descriptor : null;
    }

    private static object? ParseValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (int.TryParse(value, out var intValue))
            return intValue;

        if (long.TryParse(value, out var longValue))
            return longValue;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
            return decimalValue;

        if (bool.TryParse(value, out var boolValue))
            return boolValue;

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue))
            return dateValue;

        if (Guid.TryParse(value, out var guidValue))
            return guidValue;

        return value;
    }
}
