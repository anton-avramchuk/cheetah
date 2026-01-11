using Cheetah.AspNetCore.Contracts.Requests;
using Cheetah.AspNetCore.Contracts.Responses;
using Cheetah.Backend.Endpoints.Responses;
using Cheetah.Core.CQRS;

namespace Cheetah.Backend.Endpoints.Http;

/// <summary>
/// DELETE endpoint for removing resources
/// Returns 204 NoContent on success
/// Source generator will create: MapDelete -> mapper.Map -> dispatcher.SendAsync -> Results.NoContent
/// </summary>
/// <typeparam name="TRequest">HTTP request DTO</typeparam>
/// <typeparam name="TCommand">CQRS Command</typeparam>
public abstract class DeleteCommandEndpoint<TRequest, TCommand>
    : EndpointBase<TRequest, EmptyResponse>
    where TRequest : ICrmRequest
    where TCommand : ICommand
{
    public sealed override HttpMethod Method => HttpMethod.Delete;
}
