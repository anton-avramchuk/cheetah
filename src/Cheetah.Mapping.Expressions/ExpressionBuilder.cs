using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cheetah.Mapping.Core;

namespace Cheetah.Mapping.Expressions;

public static class ExpressionBuilder
{
    private static readonly ConcurrentDictionary<(Type, Type), Lazy<LambdaExpression>> _expressionCache = new();
    private static readonly ConcurrentDictionary<(Type, Type), Lazy<Delegate>> _delegateCache = new();
    private static readonly ConcurrentDictionary<(Type, Type), Lazy<Func<object, object?>>> _objectDelegateCache = new();
    private static readonly ConcurrentDictionary<(Type, Type), Lazy<Action<object, object>>> _inPlaceDelegateCache = new();

    public static Expression<Func<TSource, TDest>> GetMapExpression<TSource, TDest>()
        => (Expression<Func<TSource, TDest>>)GetMapExpression(typeof(TSource), typeof(TDest));

    public static LambdaExpression GetMapExpression(Type sourceType, Type destType)
        => _expressionCache.GetOrAdd((sourceType, destType),
            static key => new Lazy<LambdaExpression>(() => BuildExpression(key.Item1, key.Item2),
                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication)).Value;

    public static Func<TSource, TDest> GetMapDelegate<TSource, TDest>()
        => (Func<TSource, TDest>)_delegateCache.GetOrAdd((typeof(TSource), typeof(TDest)),
            static key => new Lazy<Delegate>(() => GetMapExpression(key.Item1, key.Item2).Compile(),
                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication)).Value;

    public static Func<object, object?> GetMapObjectDelegate(Type sourceType, Type destType)
        => _objectDelegateCache.GetOrAdd((sourceType, destType),
            static key => new Lazy<Func<object, object?>>(() => BuildObjectDelegate(key.Item1, key.Item2),
                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication)).Value;

    public static Action<object, object> GetInPlaceDelegate(Type sourceType, Type destType)
        => _inPlaceDelegateCache.GetOrAdd((sourceType, destType),
            static key => new Lazy<Action<object, object>>(() => BuildInPlaceDelegate(key.Item1, key.Item2),
                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication)).Value;

    private static Func<object, object?> BuildObjectDelegate(Type sourceType, Type destType)
    {
        // (object src) => (object)map((TSource)src)
        var lambda = GetMapExpression(sourceType, destType);
        var objParam = Expression.Parameter(typeof(object), "src");
        var invoke = Expression.Invoke(lambda, Expression.Convert(objParam, sourceType));
        var body = Expression.Convert(invoke, typeof(object));
        return Expression.Lambda<Func<object, object?>>(body, objParam).Compile();
    }

    private static LambdaExpression BuildExpression(Type sourceType, Type destType)
    {
        var sourceParameter = Expression.Parameter(sourceType, "source");
        var body = BuildMemberInit(sourceParameter, sourceType, destType);

        Expression finalBody;
        if (CanBeNull(sourceType))
        {
            // source == null ? default : body
            var nullCheck = Expression.Equal(sourceParameter, Expression.Constant(null, sourceType));
            var nullDest = Expression.Default(destType);
            finalBody = Expression.Condition(nullCheck, nullDest, body);
        }
        else
        {
            finalBody = body;
        }

        var funcType = typeof(Func<,>).MakeGenericType(sourceType, destType);
        return Expression.Lambda(funcType, finalBody, sourceParameter);
    }

    private static Action<object, object> BuildInPlaceDelegate(Type sourceType, Type destType)
    {
        // (object src, object dst) => { var s = (TSource)src; var d = (TDest)dst; d.P1 = s.P1; ... }
        var srcParam = Expression.Parameter(typeof(object), "src");
        var dstParam = Expression.Parameter(typeof(object), "dst");
        var s = Expression.Variable(sourceType, "s");
        var d = Expression.Variable(destType, "d");

        var assignments = new List<Expression>
        {
            Expression.Assign(s, Expression.Convert(srcParam, sourceType)),
            Expression.Assign(d, Expression.Convert(dstParam, destType)),
        };

        var pairs = GetMappablePairs(sourceType, destType, allowDestWriteOnly: true);
        foreach (var (destProp, sourceProp) in pairs)
        {
            if (!destProp.CanWrite) continue;
            var sourceAccess = Expression.Property(s, sourceProp);
            var value = ConvertValue(sourceAccess, sourceProp.PropertyType, destProp.PropertyType, destProp.Name, destType.Name, sourceType.Name);
            assignments.Add(Expression.Assign(Expression.Property(d, destProp), value));
        }

        var block = Expression.Block(new[] { s, d }, assignments);
        return Expression.Lambda<Action<object, object>>(block, srcParam, dstParam).Compile();
    }

    private static Expression BuildMemberInit(Expression sourceExpression, Type sourceType, Type destType)
    {
        var ctor = destType.GetConstructor(Type.EmptyTypes);
        if (ctor == null)
            throw new InvalidOperationException(
                $"Type {destType.Name} does not have a parameterless constructor. Strict mapping failed.");

        var newExpr = Expression.New(ctor);
        var bindings = new List<MemberBinding>();
        var pairs = GetMappablePairs(sourceType, destType, allowDestWriteOnly: true);

        foreach (var (destProp, sourceProp) in pairs)
        {
            var sourceAccess = Expression.Property(sourceExpression, sourceProp);
            var value = ConvertValue(sourceAccess, sourceProp.PropertyType, destProp.PropertyType,
                destProp.Name, destType.Name, sourceType.Name);
            bindings.Add(Expression.Bind(destProp, value));
        }

        return Expression.MemberInit(newExpr, bindings);
    }

    private static IEnumerable<(PropertyInfo Dest, PropertyInfo Source)> GetMappablePairs(
        Type sourceType, Type destType, bool allowDestWriteOnly)
    {
        var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p, StringComparer.Ordinal);

        var destProps = destType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var destProp in destProps)
        {
            if (destProp.GetCustomAttribute<MapIgnoreAttribute>() != null) continue;

            var sourceName = destProp.GetCustomAttribute<MapPropertyAttribute>()?.SourcePropertyName
                             ?? destProp.Name;

            if (!sourceProps.TryGetValue(sourceName, out var sourceProp))
            {
                throw new InvalidOperationException(
                    $"Strict Validation Failed: Unmapped property '{destType.Name}.{destProp.Name}'! " +
                    $"Source type '{sourceType.Name}' does not have a property '{sourceName}'. " +
                    $"Use [MapIgnore] or [MapProperty(\"...\")] to resolve.");
            }

            yield return (destProp, sourceProp);
        }
    }

    private static Expression ConvertValue(
        Expression sourceAccess, Type sourceType, Type destType,
        string destPropName, string destTypeName, string sourceTypeName)
    {
        // Идентичные типы — без cast.
        if (sourceType == destType) return sourceAccess;

        // Неявная ссылочная совместимость (Derived → Base, T → object, T → interface).
        // Для ссылочных типов используем Convert только если он действительно нужен.
        if (destType.IsAssignableFrom(sourceType))
        {
            return sourceType.IsValueType
                ? Expression.Convert(sourceAccess, destType) // boxing
                : Expression.TypeAs(sourceAccess, destType); // безопасный ref-cast
        }

        var srcUnderlying = Nullable.GetUnderlyingType(sourceType);
        var destUnderlying = Nullable.GetUnderlyingType(destType);

        // T? → T  (потеря null — GetValueOrDefault)
        if (srcUnderlying != null && srcUnderlying == destType)
        {
            return Expression.Call(sourceAccess, nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes);
        }

        // T → T?
        if (destUnderlying != null && destUnderlying == sourceType)
        {
            return Expression.Convert(sourceAccess, destType);
        }

        // T? → U? (через underlying, если совместимы)
        if (srcUnderlying != null && destUnderlying != null && destUnderlying.IsAssignableFrom(srcUnderlying))
        {
            return Expression.Convert(sourceAccess, destType);
        }

        // Коллекции
        if (TryBuildCollectionMap(sourceAccess, sourceType, destType, out var collectionExpr))
            return collectionExpr;

        // Вложенные классы — рекурсия.
        if (destType.IsClass && destType != typeof(string) && sourceType.IsClass && sourceType != typeof(string))
        {
            var nestedInit = BuildMemberInit(sourceAccess, sourceType, destType);
            if (CanBeNull(sourceType))
            {
                var nullCheck = Expression.Equal(sourceAccess, Expression.Constant(null, sourceType));
                var nullDest = Expression.Constant(null, destType);
                return Expression.Condition(nullCheck, nullDest, nestedInit);
            }
            return nestedInit;
        }

        throw new InvalidOperationException(
            $"Cannot map property '{destTypeName}.{destPropName}' from '{sourceTypeName}.{sourceType.Name}' " +
            $"to '{destType.Name}'. No conversion found.");
    }

    private static bool TryBuildCollectionMap(
        Expression sourceAccess, Type sourceType, Type destType, out Expression result)
    {
        result = null!;
        if (!TryGetEnumerableElement(sourceType, out var srcElem) ||
            !TryGetEnumerableElement(destType, out var destElem))
        {
            return false;
        }

        // Лямбда элемент-в-элемент.
        var elemParam = Expression.Parameter(srcElem, "x");
        Expression elemBody;
        if (srcElem == destElem)
        {
            elemBody = elemParam;
        }
        else if (destElem.IsAssignableFrom(srcElem))
        {
            elemBody = srcElem.IsValueType
                ? Expression.Convert(elemParam, destElem)
                : Expression.TypeAs(elemParam, destElem);
        }
        else if (destElem.IsClass && destElem != typeof(string) && srcElem.IsClass && srcElem != typeof(string))
        {
            // Рекурсивный member-init для каждого элемента.
            var init = BuildMemberInit(elemParam, srcElem, destElem);
            if (CanBeNull(srcElem))
            {
                var nullCheck = Expression.Equal(elemParam, Expression.Constant(null, srcElem));
                var nullElem = Expression.Constant(null, destElem);
                elemBody = Expression.Condition(nullCheck, nullElem, init);
            }
            else
            {
                elemBody = init;
            }
        }
        else
        {
            return false;
        }

        var selectorType = typeof(Func<,>).MakeGenericType(srcElem, destElem);
        var selector = Expression.Lambda(selectorType, elemBody, elemParam);

        // Enumerable.Select(source, selector) — выбираем перегрузку без index.
        var selectMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(Enumerable.Select)
                        && m.GetParameters().Length == 2
                        && m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>))
            .MakeGenericMethod(srcElem, destElem);

        Expression mapped = Expression.Call(selectMethod, sourceAccess, selector);

        // Материализация по типу назначения.
        if (destType.IsArray)
        {
            var toArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))!.MakeGenericMethod(destElem);
            mapped = Expression.Call(toArray, mapped);
        }
        else if (destType.IsGenericType)
        {
            var def = destType.GetGenericTypeDefinition();
            if (def == typeof(List<>) || def == typeof(IList<>) || def == typeof(ICollection<>)
                || def == typeof(IReadOnlyList<>) || def == typeof(IReadOnlyCollection<>))
            {
                var toList = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!.MakeGenericMethod(destElem);
                mapped = Expression.Call(toList, mapped);
            }
            // IEnumerable<T> — уже подходит.
        }

        // Null-check для источника-коллекции.
        if (CanBeNull(sourceType))
        {
            var nullCheck = Expression.Equal(sourceAccess, Expression.Constant(null, sourceType));
            var nullDest = Expression.Constant(null, destType);
            mapped = Expression.Condition(nullCheck, nullDest, Expression.Convert(mapped, destType));
        }
        else
        {
            mapped = Expression.Convert(mapped, destType);
        }

        result = mapped;
        return true;
    }

    private static bool TryGetEnumerableElement(Type type, out Type element)
    {
        if (type == typeof(string)) { element = null!; return false; }
        if (type.IsArray) { element = type.GetElementType()!; return true; }
        if (type.IsGenericType)
        {
            var ienum = typeof(IEnumerable<>);
            if (type.GetGenericTypeDefinition() == ienum)
            {
                element = type.GetGenericArguments()[0];
                return true;
            }
        }
        var implemented = type.GetInterfaces().FirstOrDefault(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (implemented != null)
        {
            element = implemented.GetGenericArguments()[0];
            return true;
        }
        element = null!;
        return false;
    }

    private static bool CanBeNull(Type t)
        => !t.IsValueType || Nullable.GetUnderlyingType(t) != null;
}
