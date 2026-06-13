using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Specification;
using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Specifications;

/// <summary>
/// Default <see cref="IMongoFilterBuilder"/>. Prefers a native <see cref="MongoSpecification{T}.ToFilter"/>
/// when the specification provides one, otherwise renders the LINQ expression via
/// <see cref="FilterDefinitionBuilder{TDocument}.Where"/>.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMongoFilterBuilder))]
public class MongoFilterBuilder : IMongoFilterBuilder
{
    /// <inheritdoc />
    public FilterDefinition<T> Build<T>(ISpecification<T>? specification)
    {
        if (specification is null)
            return FilterDefinition<T>.Empty;

        if (specification is MongoSpecification<T> nativeSpec)
            return nativeSpec.ToFilter();

        return Builders<T>.Filter.Where(specification.ToExpression());
    }
}
