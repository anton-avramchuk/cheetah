using System.Linq.Expressions;
using Cheetah.Core.Specification;
using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Specifications;

/// <summary>
/// Non-generic marker letting the filter builder detect specifications that carry a native
/// <see cref="FilterDefinition{TDocument}"/>.
/// </summary>
public interface IMongoSpecification
{
}

/// <summary>
/// A specification that supplies its MongoDB filter directly, for predicates that cannot be
/// expressed as a translatable LINQ expression (e.g. <c>$text</c>, geo, array operators).
/// <see cref="IMongoFilterBuilder"/> uses <see cref="ToFilter"/> in preference to
/// <see cref="Specification{T}.ToExpression"/> when available.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class MongoSpecification<T> : Specification<T>, IMongoSpecification
{
    /// <summary>Returns the native MongoDB filter.</summary>
    public abstract FilterDefinition<T> ToFilter();

    /// <summary>
    /// Native Mongo specifications are not evaluable in memory by default; override if an
    /// equivalent LINQ expression exists.
    /// </summary>
    public override Expression<Func<T, bool>> ToExpression()
        => throw new NotSupportedException(
            $"{GetType().Name} provides a native MongoDB filter and cannot be translated to a LINQ expression.");
}
