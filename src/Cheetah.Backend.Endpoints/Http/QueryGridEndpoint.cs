using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;

namespace Cheetah.Backend.Endpoints.Http;

/// <summary>
/// GET endpoint for grid queries that returns paginated, filtered, and sorted data
/// Maps TRequest -> TQuery -> GridResult{TQueryResult} -> GridResult{TResponse}
/// Source generator will create: MapGet -> mapper.Map -> dispatcher.QueryAsync -> map items -> Results.Ok(GridResult)
/// </summary>
/// <typeparam name="TRequest">HTTP request DTO (should inherit from GridRequest)</typeparam>
/// <typeparam name="TQuery">CQRS Query returning GridResult{TQueryResult}</typeparam>
/// <typeparam name="TQueryResult">Query result model type</typeparam>
/// <typeparam name="TResponse">Response ViewModel type</typeparam>
public abstract class QueryGridEndpoint<TRequest, TQuery, TQueryResult, TResponse>
    : EndpointBase<TRequest, GridResult<TResponse>>
    where TRequest : GridRequest, ICrmRequest
    where TQuery : IQuery<GridResult<TQueryResult>>
    where TResponse : class, ICrmResponse
{
    public sealed override HttpMethod Method => HttpMethod.Get;
}
