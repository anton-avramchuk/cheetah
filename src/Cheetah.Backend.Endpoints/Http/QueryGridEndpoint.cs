using Cheetah.AspNetCore.Contracts.Requests;
using Cheetah.AspNetCore.Contracts.Responses;
using Cheetah.Contracts.Requests;
using Cheetah.Core.CQRS;

namespace Cheetah.Backend.Endpoints.Http;

/// <summary>
/// GET endpoint for grid queries that returns paginated, filtered, and sorted data
/// Maps TRequest -> TQuery -> GridResult{TViewModel}
/// Source generator will create: MapGet -> mapper.Map -> dispatcher.QueryAsync -> Results.Ok(GridResult)
/// </summary>
/// <typeparam name="TRequest">HTTP request DTO (should inherit from GridRequest)</typeparam>
/// <typeparam name="TQuery">CQRS Query returning GridResult{TViewModel}</typeparam>
/// <typeparam name="TViewModel">View model type in the grid result</typeparam>
/// <remarks>
/// TViewModel doesn't need ICrmResponse constraint because it's wrapped in GridResult
/// which already implements ICrmResponse
/// </remarks>
public abstract class QueryGridEndpoint<TRequest, TQuery, TViewModel>
    : EndpointBase<TRequest, GridResult<TViewModel>>
    where TRequest : GridRequest, ICrmRequest
    where TQuery : IQuery<GridResult<TViewModel>>
    where TViewModel : class
{
    public sealed override HttpMethod Method => HttpMethod.Get;
}
