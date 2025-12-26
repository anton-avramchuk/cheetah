namespace Cheetah.Mapping.Core;

public interface IObjectMapper
{
    TDestination Map<TDestination>(object source);
    TDestination Map<TSource, TDestination>(TSource source);
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
    object Map(Type sourceType, Type destinationType, object source);
    object Map(Type sourceType, Type destinationType, object source, object destination);
    IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source);
}