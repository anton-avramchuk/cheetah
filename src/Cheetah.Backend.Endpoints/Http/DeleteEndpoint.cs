using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
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

/// <summary>
/// DELETE endpoint that returns a body: 200 OK with the result of the command.
///
/// Нужен там, где удаление отменяемо: ответ несёт состояние после удаления и токен отмены, иначе
/// вызывающему пришлось бы дочитывать его вторым запросом — а откатывать было бы нечем.
/// </summary>
public abstract class DeleteCommandWithResultEndpoint<TRequest, TCommand, TCommandResult, TResponse>
    : EndpointBase<TRequest, TResponse>
    where TRequest : ICrmRequest
    where TCommand : ICommand<TCommandResult>
    where TResponse : ICrmResponse
{
    public sealed override HttpMethod Method => HttpMethod.Delete;
}
