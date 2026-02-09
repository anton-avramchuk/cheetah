using System.Linq.Expressions;
using System.Reflection;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Mapping.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Grid;

public class EfGridRepository<TDbContext, TEntity, TKey> : EfRepository<TDbContext, TEntity, TKey>, IGridRepository<TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : Entity<TKey>
{
    private static readonly MethodInfo OrderByMethod = typeof(Queryable)
        .GetMethods()
        .First(m => m.Name == nameof(Queryable.OrderBy) && m.GetParameters().Length == 2);

    private static readonly MethodInfo OrderByDescendingMethod = typeof(Queryable)
        .GetMethods()
        .First(m => m.Name == nameof(Queryable.OrderByDescending) && m.GetParameters().Length == 2);

    private static readonly MethodInfo ThenByMethod = typeof(Queryable)
        .GetMethods()
        .First(m => m.Name == nameof(Queryable.ThenBy) && m.GetParameters().Length == 2);

    private static readonly MethodInfo ThenByDescendingMethod = typeof(Queryable)
        .GetMethods()
        .First(m => m.Name == nameof(Queryable.ThenByDescending) && m.GetParameters().Length == 2);

    private readonly IObjectMapper _mapper;
    private readonly ILogger _logger;

    public EfGridRepository(TDbContext dbContext, IObjectMapper mapper, ILogger logger)
        : base(dbContext)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async ValueTask<GridResult<TViewModel>> GetGridAsync<TViewModel>(
        GridRequest request,
        CancellationToken ct = default)
        where TViewModel : class
    {
        var queryable = AsNoTrackingQueryable();

        // 1. Apply filtering
        if (request.Filter is not null)
        {
            var filterExpression = BuildFilterExpression(request.Filter);
            if (filterExpression is not null)
                queryable = queryable.Where(filterExpression);
        }

        // 2. Get total count (after filtering, before pagination)
        var total = await queryable.CountAsync(ct);

        // 3. Apply sorting
        if (request.Sort.Count > 0)
            queryable = ApplySorting(queryable, request.Sort);

        // 4. Apply pagination
        if (request.PageSize > 0)
        {
            var page = Math.Max(1, request.Page);
            var skip = (page - 1) * request.PageSize;
            queryable = queryable.Skip(skip).Take(request.PageSize);
        }

        // 5. Project to ViewModel and execute
        var projectedQuery = _mapper.ProjectTo<TViewModel>(queryable);
        var data = await projectedQuery.ToListAsync(ct);

        return new GridResult<TViewModel>
        {
            Data = data,
            Total = total
        };
    }

    private Expression<Func<TEntity, bool>>? BuildFilterExpression(FilterDescriptor filter)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var expression = BuildFilterExpressionInternal(filter, parameter);

        if (expression is null)
            return null;

        return Expression.Lambda<Func<TEntity, bool>>(expression, parameter);
    }

    private Expression? BuildFilterExpressionInternal(
        FilterDescriptor filter,
        ParameterExpression parameter)
    {
        // Handle composite filters (AND/OR)
        if (filter.Filters.Count > 0)
        {
            var expressions = filter.Filters
                .Select(f => BuildFilterExpressionInternal(f, parameter))
                .Where(e => e is not null)
                .Cast<Expression>()
                .ToList();

            if (expressions.Count == 0)
                return null;

            var result = expressions[0];
            for (var i = 1; i < expressions.Count; i++)
            {
                result = string.Equals(filter.Logic, "or", StringComparison.OrdinalIgnoreCase)
                    ? Expression.OrElse(result, expressions[i])
                    : Expression.AndAlso(result, expressions[i]);
            }

            return result;
        }

        // Handle leaf filter
        if (string.IsNullOrEmpty(filter.Field) || string.IsNullOrEmpty(filter.Operator))
            return null;

        var propertyExpression = BuildPropertyExpression(parameter, filter.Field);
        if (propertyExpression is null)
        {
            _logger.LogWarning("Grid filter: Property '{Field}' not found on type {Type}",
                filter.Field, typeof(TEntity).Name);
            return null;
        }

        return BuildComparisonExpression(propertyExpression, filter.Operator, filter.Value, filter.IgnoreCase);
    }

    private static Expression? BuildPropertyExpression(Expression parameter, string propertyPath)
    {
        var parts = propertyPath.Split('.');
        Expression current = parameter;

        foreach (var part in parts)
        {
            var property = current.Type.GetProperty(part,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is null)
                return null;

            current = Expression.Property(current, property);
        }

        return current;
    }

    private static Expression? BuildComparisonExpression(
        Expression property,
        string op,
        object? value,
        bool ignoreCase)
    {
        var propertyType = property.Type;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        switch (op.ToLowerInvariant())
        {
            case "isnull":
                return BuildNullCheck(property, true);
            case "isnotnull":
                return BuildNullCheck(property, false);
            case "isempty":
                return BuildEmptyCheck(property, true);
            case "isnotempty":
                return BuildEmptyCheck(property, false);
        }

        var convertedValue = ConvertValue(value, underlyingType);
        if (convertedValue is null && value is not null)
            return null;

        var constant = Expression.Constant(convertedValue, propertyType);

        return op.ToLowerInvariant() switch
        {
            "eq" => BuildEqualityExpression(property, constant, ignoreCase),
            "neq" => Expression.Not(BuildEqualityExpression(property, constant, ignoreCase)),
            "contains" => BuildStringMethod(property, constant, "Contains", ignoreCase),
            "startswith" => BuildStringMethod(property, constant, "StartsWith", ignoreCase),
            "endswith" => BuildStringMethod(property, constant, "EndsWith", ignoreCase),
            "gt" => Expression.GreaterThan(property, constant),
            "gte" => Expression.GreaterThanOrEqual(property, constant),
            "lt" => Expression.LessThan(property, constant),
            "lte" => Expression.LessThanOrEqual(property, constant),
            _ => null
        };
    }

    private static Expression BuildNullCheck(Expression property, bool checkForNull)
    {
        var nullConstant = Expression.Constant(null, property.Type);
        return checkForNull
            ? Expression.Equal(property, nullConstant)
            : Expression.NotEqual(property, nullConstant);
    }

    private static Expression BuildEmptyCheck(Expression property, bool checkForEmpty)
    {
        if (property.Type != typeof(string))
            return BuildNullCheck(property, checkForEmpty);

        var emptyConstant = Expression.Constant(string.Empty, typeof(string));
        var isNullOrEmpty = Expression.OrElse(
            Expression.Equal(property, Expression.Constant(null, typeof(string))),
            Expression.Equal(property, emptyConstant));

        return checkForEmpty ? isNullOrEmpty : Expression.Not(isNullOrEmpty);
    }

    private static Expression BuildEqualityExpression(Expression property, Expression constant, bool ignoreCase)
    {
        if (property.Type == typeof(string) && ignoreCase)
        {
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            var nullCoalesce = Expression.Coalesce(property, Expression.Constant(string.Empty));
            var propertyLower = Expression.Call(nullCoalesce, toLowerMethod);

            var valueLower = constant.Type == typeof(string) && ((ConstantExpression)constant).Value is string s
                ? Expression.Constant(s.ToLower())
                : constant;

            return Expression.Equal(propertyLower, valueLower);
        }

        return Expression.Equal(property, constant);
    }

    private static Expression? BuildStringMethod(
        Expression property,
        Expression constant,
        string methodName,
        bool ignoreCase)
    {
        if (property.Type != typeof(string))
            return null;

        var stringComparisonType = typeof(StringComparison);
        var comparisonValue = ignoreCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        var method = typeof(string).GetMethod(methodName, [typeof(string), stringComparisonType]);
        if (method is null)
            return null;

        var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
        var methodCall = Expression.Call(property, method, constant, Expression.Constant(comparisonValue));

        return Expression.AndAlso(nullCheck, methodCall);
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value is null)
            return null;

        try
        {
            if (value is System.Text.Json.JsonElement jsonElement)
                return ConvertJsonElement(jsonElement, targetType);

            if (targetType == typeof(string))
                return value.ToString();

            if (targetType == typeof(Guid))
                return Guid.Parse(value.ToString()!);

            if (targetType == typeof(DateTime))
                return DateTime.Parse(value.ToString()!);

            if (targetType == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(value.ToString()!);

            if (targetType == typeof(DateOnly))
                return DateOnly.Parse(value.ToString()!);

            if (targetType == typeof(TimeOnly))
                return TimeOnly.Parse(value.ToString()!);

            if (targetType.IsEnum)
                return Enum.Parse(targetType, value.ToString()!, ignoreCase: true);

            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return null;
        }
    }

    private static object? ConvertJsonElement(System.Text.Json.JsonElement element, Type targetType)
    {
        return element.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String when targetType == typeof(string)
                => element.GetString(),
            System.Text.Json.JsonValueKind.String when targetType == typeof(Guid)
                => element.TryGetGuid(out var g) ? g : Guid.Parse(element.GetString()!),
            System.Text.Json.JsonValueKind.String when targetType == typeof(DateTime)
                => element.TryGetDateTime(out var dt) ? dt : DateTime.Parse(element.GetString()!),
            System.Text.Json.JsonValueKind.String when targetType == typeof(DateTimeOffset)
                => element.TryGetDateTimeOffset(out var dto) ? dto : DateTimeOffset.Parse(element.GetString()!),
            System.Text.Json.JsonValueKind.String when targetType.IsEnum
                => Enum.Parse(targetType, element.GetString()!, ignoreCase: true),
            System.Text.Json.JsonValueKind.String
                => element.GetString(),
            System.Text.Json.JsonValueKind.Number when targetType == typeof(int)
                => element.GetInt32(),
            System.Text.Json.JsonValueKind.Number when targetType == typeof(long)
                => element.GetInt64(),
            System.Text.Json.JsonValueKind.Number when targetType == typeof(decimal)
                => element.GetDecimal(),
            System.Text.Json.JsonValueKind.Number when targetType == typeof(double)
                => element.GetDouble(),
            System.Text.Json.JsonValueKind.Number when targetType == typeof(float)
                => element.GetSingle(),
            System.Text.Json.JsonValueKind.True => true,
            System.Text.Json.JsonValueKind.False => false,
            _ => Convert.ChangeType(element.ToString(), targetType)
        };
    }

    private static IQueryable<TEntity> ApplySorting(
        IQueryable<TEntity> queryable,
        List<SortDescriptor> sortDescriptors)
    {
        var isFirst = true;

        foreach (var sort in sortDescriptors)
        {
            if (string.IsNullOrEmpty(sort.Field))
                continue;

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = BuildPropertyExpression(parameter, sort.Field);

            if (property is null)
                continue;

            var lambda = Expression.Lambda(property, parameter);
            var isDescending = string.Equals(sort.Dir, "desc", StringComparison.OrdinalIgnoreCase);

            var method = isFirst
                ? (isDescending ? OrderByDescendingMethod : OrderByMethod)
                : (isDescending ? ThenByDescendingMethod : ThenByMethod);

            var genericMethod = method.MakeGenericMethod(typeof(TEntity), property.Type);

            queryable = (IQueryable<TEntity>)genericMethod.Invoke(null, [queryable, lambda])!;
            isFirst = false;
        }

        return queryable;
    }
}

public class EfGridRepository<TDbContext, TEntity> : EfGridRepository<TDbContext, TEntity, Guid>, IGridRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : Entity<Guid>
{
    public EfGridRepository(TDbContext dbContext, IObjectMapper mapper, ILogger logger)
        : base(dbContext, mapper, logger)
    {
    }
}
