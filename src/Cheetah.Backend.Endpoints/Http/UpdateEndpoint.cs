using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Backend.Endpoints.Responses;
using Cheetah.Core.CQRS;

namespace Cheetah.Backend.Endpoints.Http;

/// <summary>
/// PUT endpoint for full updates (void command)
/// Returns 204 NoContent on success
/// Source generator will create: MapPut -> mapper.Map -> dispatcher.SendAsync -> Results.NoContent
/// </summary>
/// <typeparam name="TRequest">HTTP request DTO</typeparam>
/// <typeparam name="TCommand">CQRS Command</typeparam>
public abstract class UpdateCommandEndpoint<TRequest, TCommand>
    : EndpointBase<TRequest, EmptyResponse>
    where TRequest : ICrmRequest
    where TCommand : ICommand
{
    public sealed override HttpMethod Method => HttpMethod.Put;
}

/// <summary>
/// PUT endpoint that returns updated resource
/// Returns 200 OK with updated data
/// Source generator will create: MapPut -> mapper.Map -> dispatcher.SendAsync -> mapper.Map -> Results.Ok
/// </summary>
public abstract class UpdateCommandWithResultEndpoint<TRequest, TCommand, TCommandResult, TResponse>
    : EndpointBase<TRequest, TResponse>
    where TRequest : ICrmRequest
    where TCommand : ICommand<TCommandResult>
    where TResponse : ICrmResponse
{
    public sealed override HttpMethod Method => HttpMethod.Put;
}

/// <summary>
/// PATCH endpoint for partial updates
/// Returns 204 NoContent on success
/// Source generator will create: MapPatch -> mapper.Map -> dispatcher.SendAsync -> Results.NoContent
/// </summary>
public abstract class PatchCommandEndpoint<TRequest, TCommand>
    : EndpointBase<TRequest, EmptyResponse>
    where TRequest : ICrmRequest
    where TCommand : ICommand
{
    public sealed override HttpMethod Method => HttpMethod.Patch;
}
