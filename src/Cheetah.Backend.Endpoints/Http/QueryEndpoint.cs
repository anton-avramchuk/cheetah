using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;

namespace Cheetah.Backend.Endpoints.Http;

/// <summary>
/// GET endpoint for queries that returns data
/// Maps TRequest -> TQuery -> TQueryResult -> TResponse
/// Source generator will create: MapGet -> mapper.Map -> dispatcher.QueryAsync -> mapper.Map -> Results.Ok
/// </summary>
/// <typeparam name="TRequest">HTTP request DTO</typeparam>
/// <typeparam name="TQuery">CQRS Query</typeparam>
/// <typeparam name="TQueryResult">Query result from handler</typeparam>
/// <typeparam name="TResponse">HTTP response DTO</typeparam>
public abstract class QueryEndpoint<TRequest, TQuery, TQueryResult, TResponse>
    : EndpointBase<TRequest, TResponse>
    where TRequest : ICrmRequest
    where TQuery : IQuery<TQueryResult>
    where TResponse : ICrmResponse
{
    public sealed override HttpMethod Method => HttpMethod.Get;
}

/// <summary>
/// GET endpoint that may return null (e.g., GetById)
/// Returns 404 if result is null
/// Source generator will create: MapGet -> check null -> Results.NotFound or Results.Ok
/// </summary>
public abstract class QueryOrNotFoundEndpoint<TRequest, TQuery, TQueryResult, TResponse>
    : EndpointBase<TRequest, TResponse>
    where TRequest : ICrmRequest
    where TQuery : IQuery<TQueryResult?>
    where TResponse : ICrmResponse
{
    public sealed override HttpMethod Method => HttpMethod.Get;
}

/// <summary>
/// GET endpoint for collections
/// Source generator will create: MapGet -> dispatcher.QueryAsync -> map collection -> Results.Ok
/// </summary>
public abstract class QueryCollectionEndpoint<TRequest, TQuery, TQueryResult, TResponse>
    : EndpointBase<TRequest, TResponse>
    where TRequest : ICrmRequest
    where TQuery : IQuery<IReadOnlyList<TQueryResult>>
    where TResponse : ICrmResponse
{
    public sealed override HttpMethod Method => HttpMethod.Get;
}
