using System.Linq.Expressions;
using System.Reflection;
using Cheetah.Mapping.Core;

namespace Cheetah.Mapping.Expressions;

public class ExpressionObjectMapper : IObjectMapper
{
    public TDestination Map<TDestination>(object source)
    {
        if (source == null) return default!;
        var result = ExpressionBuilder.GetMapObjectDelegate(source.GetType(), typeof(TDestination))(source);
        return (TDestination)result!;
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source == null) return default!;
        return ExpressionBuilder.GetMapDelegate<TSource, TDestination>()(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        if (source == null) return destination;
        if (destination == null)
            throw new ArgumentNullException(nameof(destination),
                "In-place mapping requires a non-null destination instance.");

        ExpressionBuilder.GetInPlaceDelegate(typeof(TSource), typeof(TDestination))(source, destination);
        return destination;
    }

    public object Map(Type sourceType, Type destinationType, object source)
    {
        if (source == null) return null!;
        return ExpressionBuilder.GetMapObjectDelegate(sourceType, destinationType)(source)!;
    }

    public object Map(Type sourceType, Type destinationType, object source, object destination)
    {
        if (source == null) return destination;
        if (destination == null)
            throw new ArgumentNullException(nameof(destination),
                "In-place mapping requires a non-null destination instance.");

        ExpressionBuilder.GetInPlaceDelegate(sourceType, destinationType)(source, destination);
        return destination;
    }

    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
    {
        var sourceType = source.ElementType;
        var destType = typeof(TDestination);
        var lambda = ExpressionBuilder.GetMapExpression(sourceType, destType);

        var selectMethod = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(Queryable.Select)
                        && m.GetParameters().Length == 2
                        && m.GetParameters()[1].ParameterType.IsGenericType
                        && m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>))
            .MakeGenericMethod(sourceType, destType);

        return (IQueryable<TDestination>)selectMethod.Invoke(null, new object[] { source, lambda })!;
    }
}
