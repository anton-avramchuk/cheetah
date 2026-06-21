using Cheetah.Mapping.Core;
using Mapster;

namespace Cheetah.Mapping.Mapster;

public class MapsterObjectMapper : IObjectMapper
{
    private readonly TypeAdapterConfig _config;

    public MapsterObjectMapper(TypeAdapterConfig config)
    {
        _config = config;
    }

    public TDestination Map<TDestination>(object source)
    {
        return source.Adapt<TDestination>(_config);
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        return source.Adapt<TSource, TDestination>(_config);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        return source.Adapt(destination, _config);
    }

    public object Map(Type sourceType, Type destinationType, object source)
    {
        return source.Adapt(sourceType, destinationType, _config)!;
    }

    public object Map(Type sourceType, Type destinationType, object source, object destination)
    {
        return source.Adapt(destination, _config);
    }

    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
    {
        return source.ProjectToType<TDestination>(_config);
    }
}