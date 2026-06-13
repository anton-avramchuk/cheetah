using Cheetah.Core.CQRS;
using Google.Protobuf;

namespace Cheetah.Backend.Grpc.Abstractions;

/// <summary>
/// Маркер: gRPC-метод, отображающий proto-запрос в команду без результата.
/// gRPC-генератор находит наследников по базовому типу и достаёт generic-параметры —
/// аналогично тому, как <c>EndpointRegistrationGenerator</c> работает с REST-эндпоинтами.
/// </summary>
/// <typeparam name="TProtoRequest">proto-сообщение запроса</typeparam>
/// <typeparam name="TCommand">CQRS-команда</typeparam>
public abstract class GrpcCommand<TProtoRequest, TCommand>
    where TProtoRequest : class, IMessage
    where TCommand : ICommand
{
}

/// <summary>
/// Маркер: gRPC-метод, отображающий proto-запрос в команду с результатом и обратно в proto-ответ.
/// </summary>
/// <typeparam name="TProtoRequest">proto-сообщение запроса</typeparam>
/// <typeparam name="TCommand">CQRS-команда</typeparam>
/// <typeparam name="TResult">тип результата команды</typeparam>
/// <typeparam name="TProtoResponse">proto-сообщение ответа</typeparam>
public abstract class GrpcCommandWithResult<TProtoRequest, TCommand, TResult, TProtoResponse>
    where TProtoRequest : class, IMessage
    where TCommand : ICommand<TResult>
    where TProtoResponse : class, IMessage
{
}

/// <summary>
/// Маркер: gRPC-метод, отображающий proto-запрос в запрос (query) и результат в proto-ответ.
/// </summary>
/// <typeparam name="TProtoRequest">proto-сообщение запроса</typeparam>
/// <typeparam name="TQuery">CQRS-запрос</typeparam>
/// <typeparam name="TResult">тип результата запроса</typeparam>
/// <typeparam name="TProtoResponse">proto-сообщение ответа</typeparam>
public abstract class GrpcQuery<TProtoRequest, TQuery, TResult, TProtoResponse>
    where TProtoRequest : class, IMessage
    where TQuery : IQuery<TResult>
    where TProtoResponse : class, IMessage
{
}

/// <summary>
/// Маркер: как <see cref="GrpcQuery{TProtoRequest, TQuery, TResult, TProtoResponse}"/>,
/// но при <c>null</c>-результате бросает <c>RpcException(StatusCode.NotFound)</c>.
/// </summary>
/// <typeparam name="TProtoRequest">proto-сообщение запроса</typeparam>
/// <typeparam name="TQuery">CQRS-запрос</typeparam>
/// <typeparam name="TResult">тип результата запроса (nullable)</typeparam>
/// <typeparam name="TProtoResponse">proto-сообщение ответа</typeparam>
public abstract class GrpcQueryOrNotFound<TProtoRequest, TQuery, TResult, TProtoResponse>
    where TProtoRequest : class, IMessage
    where TQuery : IQuery<TResult>
    where TProtoResponse : class, IMessage
{
}
