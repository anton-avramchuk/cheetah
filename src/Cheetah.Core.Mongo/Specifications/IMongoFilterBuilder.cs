using Cheetah.Core.Specification;
using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Specifications;

/// <summary>
/// Translates a <see cref="ISpecification{T}"/> into a MongoDB <see cref="FilterDefinition{TDocument}"/>.
/// Unlike SQL, the MongoDB driver natively renders <c>Expression&lt;Func&lt;T,bool&gt;&gt;</c>, so the
/// translation is direct; native filters travel through <see cref="MongoSpecification{T}"/>.
/// </summary>
public interface IMongoFilterBuilder
{
    /// <summary>
    /// Builds a filter for the specification, or an empty (match-all) filter when
    /// <paramref name="specification"/> is <c>null</c>.
    /// </summary>
    FilterDefinition<T> Build<T>(ISpecification<T>? specification);
}
