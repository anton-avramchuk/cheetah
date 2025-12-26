namespace Cheetah.Core.CQRS;

/// <summary>
/// Handler for queries
/// </summary>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    ValueTask<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}