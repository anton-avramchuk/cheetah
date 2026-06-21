using System.Globalization;
using System.Text.RegularExpressions;
using Cheetah.Contracts.Requests;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Cheetah.AspNetCore.Contracts.ModelBinders
{
    /// <summary>
    /// Model binder для преобразования query string в GridRequest
    /// Поддерживает формат Kendo UI:
    /// ?page=1&amp;pageSize=10
    /// &amp;sort[0][field]=Name&amp;sort[0][dir]=asc
    /// &amp;filter[logic]=and&amp;filter[filters][0][field]=Name&amp;filter[filters][0][operator]=contains&amp;filter[filters][0][value]=test
    /// </summary>
    public class GridRequestModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var request = new GridRequest();
            var queryString = bindingContext.HttpContext.Request.Query;

            // Парсинг Page
            if (queryString.TryGetValue("page", out var pageValue))
            {
                if (int.TryParse(pageValue.FirstOrDefault(), out var page))
                    request.Page = page;
            }

            // Парсинг PageSize
            if (queryString.TryGetValue("pageSize", out var pageSizeValue))
            {
                if (int.TryParse(pageSizeValue.FirstOrDefault(), out var pageSize))
                    request.PageSize = pageSize;
            }

            // Парсинг Sort
            request.Sort = ParseSort(queryString);

            // Парсинг Filter
            request.Filter = ParseFilter(queryString, "filter");

            bindingContext.Result = ModelBindingResult.Success(request);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Парсит сортировку из query string
        /// Поддерживаемые форматы:
        /// - sort[0][field]=Name&amp;sort[0][dir]=asc (старый формат Kendo)
        /// - sort[0].field=Name&amp;sort[0].dir=asc (новый формат PrimeNG)
        /// </summary>
        private List<SortDescriptor> ParseSort(Microsoft.AspNetCore.Http.IQueryCollection queryString)
        {
            var sorts = new List<SortDescriptor>();
            var sortKeys = queryString.Keys.Where(k => k.StartsWith("sort[")).ToList();

            if (!sortKeys.Any())
                return sorts;

            // Группируем по индексу: sort[0][...], sort[1][...]
            var sortGroups = sortKeys
                .Select(k =>
                {
                    // Поддержка двух форматов: sort[0][field] и sort[0].field
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
                    if (item is null)
                        continue;

                    var value = queryString[item.Key].FirstOrDefault();
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

        /// <summary>
        /// Рекурсивно парсит фильтры из query string
        /// Формат: filter[logic]=and&amp;filter[filters][0][field]=Name&amp;filter[filters][0][operator]=contains&amp;filter[filters][0][value]=test
        /// Поддерживает точечную нотацию: filter.field=Name&amp;filter.operator=eq&amp;filter.value=test
        /// </summary>
        private FilterDescriptor? ParseFilter(Microsoft.AspNetCore.Http.IQueryCollection queryString, string prefix)
        {
            // Проверяем есть ли фильтры с данным префиксом (поддержка обоих форматов: filter[ и filter.)
            var filterKeys = queryString.Keys.Where(k => k.StartsWith(prefix + "[") || k.StartsWith(prefix + ".")).ToList();
            if (!filterKeys.Any())
                return null;

            var descriptor = new FilterDescriptor();

            // Парсинг logic (поддержка обоих форматов)
            var logicKey = $"{prefix}[logic]";
            var logicKeyDot = $"{prefix}.logic";
            if (queryString.TryGetValue(logicKey, out var logicValue) || queryString.TryGetValue(logicKeyDot, out logicValue))
            {
                descriptor.Logic = logicValue.FirstOrDefault();
            }

            // Парсинг конечного фильтра (field, operator, value) - поддержка обоих форматов
            var fieldKey = $"{prefix}[field]";
            var fieldKeyDot = $"{prefix}.field";
            if (queryString.TryGetValue(fieldKey, out var fieldValue) || queryString.TryGetValue(fieldKeyDot, out fieldValue))
            {
                descriptor.Field = fieldValue.FirstOrDefault();

                var operatorKey = $"{prefix}[operator]";
                var operatorKeyDot = $"{prefix}.operator";
                if (queryString.TryGetValue(operatorKey, out var operatorValue) || queryString.TryGetValue(operatorKeyDot, out operatorValue))
                    descriptor.Operator = operatorValue.FirstOrDefault();

                var valueKey = $"{prefix}[value]";
                var valueKeyDot = $"{prefix}.value";
                if (queryString.TryGetValue(valueKey, out var value) || queryString.TryGetValue(valueKeyDot, out value))
                    descriptor.Value = ParseValue(value.FirstOrDefault());

                return descriptor;
            }

            // Парсинг вложенных фильтров: filter[filters][0][...] или filter.filters[0][...]
            var nestedFilterKeys = filterKeys
                .Where(k => k.Contains($"{prefix}[filters][") || k.Contains($"{prefix}.filters["))
                .ToList();

            if (nestedFilterKeys.Any())
            {
                var indices = nestedFilterKeys
                    .Select(k =>
                    {
                        // Поддержка обоих форматов: filter[filters][0] и filter.filters[0]
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
                    // Пробуем оба формата для вложенных префиксов
                    var nestedPrefixBracket = $"{prefix}[filters][{index}]";
                    var nestedPrefixDot = $"{prefix}.filters[{index}]";

                    // Определяем, какой формат используется
                    var usesDotNotation = filterKeys.Any(k => k.StartsWith(nestedPrefixDot));
                    var nestedPrefix = usesDotNotation ? nestedPrefixDot : nestedPrefixBracket;

                    var nestedFilter = ParseFilter(queryString, nestedPrefix);
                    if (nestedFilter != null)
                        descriptor.Filters.Add(nestedFilter);
                }
            }

            return descriptor.Filters.Any() || !string.IsNullOrEmpty(descriptor.Field) ? descriptor : null;
        }

        /// <summary>
        /// Парсит значение из строки в соответствующий тип
        /// </summary>
        private object ParseValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return value ?? string.Empty;

            // Попытка распарсить как число
            if (int.TryParse(value, out var intValue))
                return intValue;

            if (long.TryParse(value, out var longValue))
                return longValue;

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
                return decimalValue;

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
                return doubleValue;

            // Попытка распарсить как bool
            if (bool.TryParse(value, out var boolValue))
                return boolValue;

            // Попытка распарсить как DateTime
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue))
                return dateValue;

            // Попытка распарсить как Guid
            if (Guid.TryParse(value, out var guidValue))
                return guidValue;

            // Иначе возвращаем как строку
            return value;
        }
    }
}
