using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Cheetah.Contracts.Requests;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Grid;

/// <summary>
/// Перевод <see cref="FilterDescriptor"/> в выражение по сущности. Вынесено из
/// <see cref="EfGridRepository{TDbContext,TEntity,TKey}"/> отдельным классом, потому что это
/// единственное, что стоит между фильтром из строки запроса и SQL: ошибка здесь не падает, а тихо
/// возвращает не тот список — такое обязано проверяться тестом без базы.
///
/// Имена полей приходят из ViewModel («patientName»), а Where строится по сущности, поэтому путь
/// разворачивается через проекционную лямбду Mapster: <c>patientName</c> → <c>src.Patient.FullName</c>.
/// </summary>
public static class GridFilterExpressions
{
    /// <summary>
    /// Выражение фильтра или <c>null</c>, если описатель пустой либо ни одно поле не разрешилось.
    /// </summary>
    public static Expression<Func<TEntity, bool>>? Build<TEntity>(
        FilterDescriptor? filter,
        LambdaExpression? projection = null,
        ILogger? logger = null)
    {
        if (filter is null)
            return null;

        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var expression = BuildInternal(filter, parameter, projection, logger, typeof(TEntity));

        return expression is null ? null : Expression.Lambda<Func<TEntity, bool>>(expression, parameter);
    }

    /// <summary>
    /// Путь до члена сущности по имени поля ViewModel. Нужен и сортировке, поэтому публичный.
    /// </summary>
    public static Expression? Property(
        ParameterExpression entityParameter, string propertyPath, LambdaExpression? projection)
    {
        var parts = propertyPath.Split('.');
        Expression current = entityParameter;
        var startIndex = 0;

        // Сначала пробуем найти первый сегмент как destination-член проекции
        // (например, "patientName" → src.Direction.Patient.FullName).
        if (projection is { Body: var body } && projection.Parameters.Count == 1)
        {
            var projected = GetProjectedMember(body, parts[0]);
            if (projected is not null)
            {
                current = ReplaceParameter(projected, projection.Parameters[0], entityParameter);
                startIndex = 1;
            }
        }

        for (var i = startIndex; i < parts.Length; i++)
        {
            var property = current.Type.GetProperty(parts[i],
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is null)
                return null;

            current = Expression.Property(current, property);
        }

        return current;
    }

    private static Expression? BuildInternal(
        FilterDescriptor filter,
        ParameterExpression parameter,
        LambdaExpression? projection,
        ILogger? logger,
        Type entityType)
    {
        // Композитный фильтр (AND/OR)
        if (filter.Filters.Count > 0)
        {
            var expressions = filter.Filters
                .Select(f => BuildInternal(f, parameter, projection, logger, entityType))
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

        if (string.IsNullOrEmpty(filter.Field) || string.IsNullOrEmpty(filter.Operator))
            return null;

        var propertyExpression = Property(parameter, filter.Field, projection);
        if (propertyExpression is null)
        {
            logger?.LogWarning("Grid filter: Property '{Field}' not found on type {Type}",
                filter.Field, entityType.Name);
            return null;
        }

        return BuildComparison(propertyExpression, filter.Operator, filter.Value, filter.IgnoreCase);
    }

    private static Expression? BuildComparison(
        Expression property,
        string op,
        object? value,
        bool ignoreCase)
    {
        // Коллекция — не сравниваемое значение, а множество: фильтр по ней означает «есть хоть один
        // подходящий элемент». Без этого отбор по навыку (список внутри JSONB) молча пропадал:
        // строковый Contains на коллекции не собирается, и фильтр возвращался пустым.
        if (ElementTypeOf(property.Type) is { } elementType)
            return BuildCollectionComparison(property, elementType, op, value, ignoreCase);

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

    /// <summary>
    /// Фильтр по коллекции: тот же оператор применяется к элементу, а условие оборачивается в
    /// <c>Any</c>. «Пусто» и «не пусто» — про саму коллекцию, а не про элемент.
    /// </summary>
    private static Expression? BuildCollectionComparison(
        Expression property,
        Type elementType,
        string op,
        object? value,
        bool ignoreCase)
    {
        var normalized = op.ToLowerInvariant();

        switch (normalized)
        {
            case "isnull":
                return BuildNullCheck(property, true);
            case "isnotnull":
                return BuildNullCheck(property, false);
            case "isempty":
                return Expression.Not(BuildAny(property, elementType, predicate: null));
            case "isnotempty":
                return BuildAny(property, elementType, predicate: null);
        }

        var underlyingElement = Nullable.GetUnderlyingType(elementType) ?? elementType;
        var convertedValue = ConvertValue(value, underlyingElement);
        if (convertedValue is null && value is not null)
            return null;

        var element = Expression.Parameter(elementType, "item");
        var constant = Expression.Constant(convertedValue, elementType);

        var comparison = normalized switch
        {
            "eq" or "neq" => BuildEqualityExpression(element, constant, ignoreCase),
            "contains" => BuildStringMethod(element, constant, "Contains", ignoreCase),
            "startswith" => BuildStringMethod(element, constant, "StartsWith", ignoreCase),
            "endswith" => BuildStringMethod(element, constant, "EndsWith", ignoreCase),
            _ => null,
        };

        if (comparison is null)
            return null;

        var any = BuildAny(property, elementType, Expression.Lambda(comparison, element));

        // «Не равно» для коллекции — «ни один элемент не равен»: иначе фильтр отбирал бы всех, у кого
        // есть хоть одно другое значение, то есть почти всех.
        return normalized == "neq" ? Expression.Not(any) : any;
    }

    private static Expression BuildAny(Expression property, Type elementType, LambdaExpression? predicate)
    {
        var method = predicate is null
            ? AnyMethod.MakeGenericMethod(elementType)
            : AnyWithPredicateMethod.MakeGenericMethod(elementType);

        var source = property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            ? property
            : Expression.Convert(property, typeof(IEnumerable<>).MakeGenericType(elementType));

        var call = predicate is null
            ? Expression.Call(method, source)
            : Expression.Call(method, source, predicate);

        // Коллекция может быть null — у сущности, собранной не из базы, или у необязательного JSONB.
        return Expression.AndAlso(
            Expression.NotEqual(property, Expression.Constant(null, property.Type)),
            call);
    }

    /// <summary>
    /// Тип элемента коллекции или <c>null</c>, если это не коллекция. Строка исключена намеренно:
    /// она перечисляемая, но фильтр по ней — обычное строковое сравнение.
    /// </summary>
    private static Type? ElementTypeOf(Type type)
    {
        if (type == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(type))
            return null;

        if (type.IsGenericType)
        {
            var candidate = type.GetGenericArguments();
            if (candidate.Length == 1)
                return candidate[0];
        }

        return type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            ?.GetGenericArguments()[0];
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

        // Однопараметрическая перегрузка: StringComparison провайдеры EF в SQL не переводят.
        var method = typeof(string).GetMethod(methodName, [typeof(string)]);
        if (method is null)
            return null;

        var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

        Expression effectiveProperty = property;
        Expression effectiveConstant = constant;

        if (ignoreCase)
        {
            var toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
            effectiveProperty = Expression.Call(property, toLower);
            if (constant is ConstantExpression { Value: string s })
                effectiveConstant = Expression.Constant(s.ToLowerInvariant());
        }

        var methodCall = Expression.Call(effectiveProperty, method, effectiveConstant);
        return Expression.AndAlso(nullCheck, methodCall);
    }

    private static Expression? GetProjectedMember(Expression projectionBody, string memberName)
    {
        switch (projectionBody)
        {
            case MemberInitExpression init:
                foreach (var binding in init.Bindings)
                {
                    if (binding is MemberAssignment ma &&
                        string.Equals(ma.Member.Name, memberName, StringComparison.OrdinalIgnoreCase))
                        return ma.Expression;
                }
                return GetFromNewExpression(init.NewExpression, memberName);
            case NewExpression ne:
                return GetFromNewExpression(ne, memberName);
            default:
                return null;
        }
    }

    private static Expression? GetFromNewExpression(NewExpression ne, string memberName)
    {
        if (ne.Members is not null)
        {
            for (var i = 0; i < ne.Members.Count; i++)
            {
                if (string.Equals(ne.Members[i].Name, memberName, StringComparison.OrdinalIgnoreCase))
                    return ne.Arguments[i];
            }
        }

        var ctorParams = ne.Constructor?.GetParameters();
        if (ctorParams is not null)
        {
            for (var i = 0; i < ctorParams.Length; i++)
            {
                if (string.Equals(ctorParams[i].Name, memberName, StringComparison.OrdinalIgnoreCase))
                    return ne.Arguments[i];
            }
        }

        return null;
    }

    private static Expression ReplaceParameter(Expression source, ParameterExpression from, ParameterExpression to)
        => new ParameterReplacer(from, to).Visit(source)!;

    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == from ? to : base.VisitParameter(node);
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

    private static readonly MethodInfo AnyMethod = typeof(Enumerable)
        .GetMethods()
        .First(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 1);

    private static readonly MethodInfo AnyWithPredicateMethod = typeof(Enumerable)
        .GetMethods()
        .First(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 2);
}
