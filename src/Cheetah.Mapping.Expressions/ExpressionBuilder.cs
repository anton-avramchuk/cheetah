using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;

namespace Cheetah.Mapping.Expressions;

public static class ExpressionBuilder
{
    // Кэш для IQueryable выражений
    private static readonly ConcurrentDictionary<(Type, Type), LambdaExpression> _expressionCache = new();
    
    // Кэш для скомпилированных делегатов (Runtime маппинг)
    private static readonly ConcurrentDictionary<(Type, Type), Delegate> _delegateCache = new();

    public static Expression<Func<TSource, TDest>> GetMapExpression<TSource, TDest>()
    {
        var key = (typeof(TSource), typeof(TDest));
        
        if (_expressionCache.TryGetValue(key, out var cached))
        {
            return (Expression<Func<TSource, TDest>>)cached;
        }

        var expression = BuildExpression<TSource, TDest>();
        _expressionCache[key] = expression;
        return expression;
    }

    public static Func<TSource, TDest> GetMapDelegate<TSource, TDest>()
    {
        var key = (typeof(TSource), typeof(TDest));
        
        if (_delegateCache.TryGetValue(key, out var cached))
        {
            return (Func<TSource, TDest>)cached;
        }

        var expression = GetMapExpression<TSource, TDest>();
        var compiled = expression.Compile();
        _delegateCache[key] = compiled;
        return compiled;
    }

    private static Expression<Func<TSource, TDest>> BuildExpression<TSource, TDest>()
    {
        var sourceParameter = Expression.Parameter(typeof(TSource), "source");
        var body = BuildMemberInit(sourceParameter, typeof(TSource), typeof(TDest));
        
        // Добавляем проверку на null для корня: source == null ? null : new TDest { ... }
        var nullCheck = Expression.Equal(sourceParameter, Expression.Constant(null, typeof(TSource)));
        var nullDest = Expression.Constant(default(TDest), typeof(TDest));
        var condition = Expression.Condition(nullCheck, nullDest, body);

        return Expression.Lambda<Func<TSource, TDest>>(condition, sourceParameter);
    }

    private static Expression BuildMemberInit(Expression sourceExpression, Type sourceType, Type destType)
    {
        var destConstructor = destType.GetConstructor(Type.EmptyTypes);
        if (destConstructor == null)
            throw new InvalidOperationException($"Type {destType.Name} does not have a parameterless constructor. Strict mapping failed.");

        var newExpression = Expression.New(destConstructor);
        var bindings = new System.Collections.Generic.List<MemberBinding>();

        var destProperties = destType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        foreach (var destProp in destProperties)
        {
            if (sourceProperties.TryGetValue(destProp.Name, out var sourceProp))
            {
                var sourcePropAccess = Expression.Property(sourceExpression, sourceProp);
                var destPropType = destProp.PropertyType;
                var sourcePropType = sourceProp.PropertyType;

                Expression valueExpression;

                // Плоский маппинг типов (одинаковые типы)
                if (sourcePropType == destPropType)
                {
                    valueExpression = sourcePropAccess;
                }
                // Приведение типов
                else if (destPropType.IsAssignableFrom(sourcePropType))
                {
                    valueExpression = Expression.Convert(sourcePropAccess, destPropType);
                }
                // Nullable<T> -> T
                else if (Nullable.GetUnderlyingType(sourcePropType) == destPropType)
                {
                    valueExpression = Expression.Call(sourcePropAccess, "GetValueOrDefault", Type.EmptyTypes);
                }
                // T -> Nullable<T>
                else if (Nullable.GetUnderlyingType(destPropType) == sourcePropType)
                {
                    valueExpression = Expression.Convert(sourcePropAccess, destPropType);
                }
                // Вложенные объекты (рекурсия)
                else if (destPropType.IsClass && destPropType != typeof(string))
                {
                     var nestedInit = BuildMemberInit(sourcePropAccess, sourcePropType, destPropType);
                     var nullCheck = Expression.Equal(sourcePropAccess, Expression.Constant(null, sourcePropType));
                     var nullDest = Expression.Constant(null, destPropType);
                     valueExpression = Expression.Condition(nullCheck, nullDest, nestedInit);
                }
                else 
                {
                    throw new InvalidOperationException($"Cannot map property '{destProp.Name}' from '{sourcePropType.Name}' to '{destPropType.Name}'. No conversion found.");
                }

                bindings.Add(Expression.Bind(destProp, valueExpression));
            }
            else
            {
                // СТРОГАЯ ВАЛИДАЦИЯ: Если мы дошли сюда, значит свойства нет в Source.
                // В будущем здесь нужно проверять наличие атрибута [Ignore] или конфигурации MapFrom.
                throw new InvalidOperationException($"Strict Validation Failed: Unmapped property '{destType.Name}.{destProp.Name}'! Source type '{sourceType.Name}' does not have a corresponding property.");
            }
        }

        return Expression.MemberInit(newExpression, bindings);
    }
}
