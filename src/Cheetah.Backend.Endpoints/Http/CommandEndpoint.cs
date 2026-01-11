using Cheetah.AspNetCore.Contracts.Requests;
using Cheetah.AspNetCore.Contracts.Responses;
using Cheetah.Backend.Endpoints.Responses;
using Cheetah.Core.CQRS;
using ICommand = System.Windows.Input.ICommand;

namespace Cheetah.Backend.Endpoints.Http;

/// <summary>
/// POST endpoint for commands that don't return data (void)
/// Returns 204 NoContent on success
/// Source generator will create: MapPost -> mapper.Map -> dispatcher.SendAsync -> Results.NoContent
/// </summary>
/// <typeparam name="TRequest">HTTP request DTO</typeparam>
/// <typeparam name="TCommand">CQRS Command</typeparam>
public abstract class CommandEndpoint<TRequest, TCommand>
    : EndpointBase<TRequest, EmptyResponse>
    where TRequest : ICrmRequest
    where TCommand : ICommand
{
    public sealed override HttpMethod Method => HttpMethod.Post;
}

/// <summary>
/// POST endpoint for commands that return a result
/// Returns 200 OK with the result
/// Source generator will create: MapPost -> mapper.Map -> dispatcher.SendAsync -> mapper.Map -> Results.Ok
/// </summary>
/// <typeparam name="TRequest">HTTP request DTO</typeparam>
/// <typeparam name="TCommand">CQRS Command</typeparam>
/// <typeparam name="TCommandResult">Command result</typeparam>
/// <typeparam name="TResponse">HTTP response DTO</typeparam>
public abstract class CommandWithResultEndpoint<TRequest, TCommand, TCommandResult, TResponse>
    : EndpointBase<TRequest, TResponse>
    where TRequest : ICrmRequest
    where TCommand : ICommand<TCommandResult>
    where TResponse : ICrmResponse
{
    public sealed override HttpMethod Method => HttpMethod.Post;
}

/// <summary>
/// POST endpoint for creating resources
/// Returns 201 Created with resource ID and location header
/// Source generator will create: MapPost -> mapper.Map -> dispatcher.SendAsync -> Results.CreatedAtRoute
/// </summary>
/// <typeparam name="TRequest">HTTP request DTO</typeparam>
/// <typeparam name="TCommand">CQRS Command that returns Guid</typeparam>
public abstract class CreateCommandEndpoint<TRequest, TCommand>
    : EndpointBase<TRequest, GuidResponse>
    where TRequest : ICrmRequest
    where TCommand : ICommand<Guid>
{
    public sealed override HttpMethod Method => HttpMethod.Post;

    /// <summary>
    /// Route name for the GetById endpoint to generate Location header
    /// </summary>
    public abstract string GetByIdRouteName { get; }
}
