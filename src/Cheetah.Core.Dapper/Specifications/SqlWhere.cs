namespace Cheetah.Core.Dapper.Specifications;

/// <summary>
/// A SQL <c>WHERE</c> fragment together with its parameter values, produced from a
/// <see cref="Cheetah.Core.Specification.ISpecification{T}"/>.
/// </summary>
public sealed class SqlWhere
{
    /// <summary>An empty predicate (matches all rows).</summary>
    public static readonly SqlWhere Empty = new() { Sql = string.Empty, Parameters = new Dictionary<string, object?>() };

    /// <summary>The boolean SQL expression without the leading <c>WHERE</c> keyword.</summary>
    public required string Sql { get; init; }

    /// <summary>Parameter name/value pairs referenced by <see cref="Sql"/>.</summary>
    public required IReadOnlyDictionary<string, object?> Parameters { get; init; }

    /// <summary>True when the predicate is empty (no filtering).</summary>
    public bool IsEmpty => string.IsNullOrEmpty(Sql);
}
