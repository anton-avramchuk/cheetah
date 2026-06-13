using Cheetah.Core.Specification;

namespace Cheetah.Core.Dapper.Specifications;

/// <summary>
/// A specification that supplies its SQL predicate directly, for filters that
/// cannot be expressed as a translatable LINQ expression (functions, full-text, etc.).
/// <see cref="ExpressionToSqlParser"/> uses <see cref="ToSql"/> in preference to
/// <see cref="Specification{T}.ToExpression"/> when available.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class SqlSpecification<T> : Specification<T>, ISqlSpecification
{
    /// <summary>Returns the SQL predicate and its parameters.</summary>
    public abstract SqlWhere ToSql();
}

/// <summary>
/// Non-generic marker letting the parser detect specifications that carry raw SQL.
/// </summary>
public interface ISqlSpecification
{
    /// <summary>Returns the SQL predicate and its parameters.</summary>
    SqlWhere ToSql();
}
