using System;
using System.Linq;
using Cheetah.Mapping.Core;

namespace Cheetah.Mapping.Expressions;

public class ExpressionObjectMapper : IObjectMapper
{
    public TDestination Map<TDestination>(object source)
    {
        if (source == null) return default!;
        
        var sourceType = source.GetType();
        var destType = typeof(TDestination);
        
        // В идеале здесь нужен не generic-метод в ExpressionBuilder для вызова через рефлексию, 
        // но для MVP мы можем вызвать generic-метод GetMapDelegate через Reflection
        var method = typeof(ExpressionBuilder).GetMethod("GetMapDelegate")!
            .MakeGenericMethod(sourceType, destType);
            
        var del = method.Invoke(null, null) as Delegate;
        return (TDestination)del!.DynamicInvoke(source)!;
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source == null) return default!;
        var mapper = ExpressionBuilder.GetMapDelegate<TSource, TDestination>();
        return mapper(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        // MVP: Пока не поддерживаем маппинг в существующий объект
        // Для этого нужно генерировать Expression, который делает property = source.property
        throw new NotImplementedException("Mapping to existing instance is not supported yet.");
    }

    public object Map(Type sourceType, Type destinationType, object source)
    {
        if (source == null) return null!;
        
        var method = typeof(ExpressionBuilder).GetMethod("GetMapDelegate")!
            .MakeGenericMethod(sourceType, destinationType);
            
        var del = method.Invoke(null, null) as Delegate;
        return del!.DynamicInvoke(source)!;
    }

    public object Map(Type sourceType, Type destinationType, object source, object destination)
    {
        throw new NotImplementedException("Mapping to existing instance is not supported yet.");
    }

    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
    {
        var sourceType = source.ElementType;
        var destType = typeof(TDestination);
        
        var expressionMethod = typeof(ExpressionBuilder).GetMethod("GetMapExpression")!
            .MakeGenericMethod(sourceType, destType);
            
        var expression = expressionMethod.Invoke(null, null);
        
        // Queryable.Select(source, expression)
        var selectMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Select" && m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(System.Linq.Expressions.Expression<>))
            .MakeGenericMethod(sourceType, destType);
            
        return (IQueryable<TDestination>)selectMethod.Invoke(null, new[] { source, expression })!;
    }
}
