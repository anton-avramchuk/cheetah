using Cheetah.Core.Domain.Exceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Cheetah.Backend.Grpc.Interceptors;

/// <summary>
/// Серверный интерсептор: транслирует доменные исключения в <see cref="RpcException"/>
/// с подходящим <see cref="StatusCode"/>. gRPC-аналог <c>ValidationExceptionHandler</c> для REST.
/// </summary>
public sealed class CrmExceptionInterceptor : Interceptor
{
    private readonly ILogger<CrmExceptionInterceptor> _logger;

    public CrmExceptionInterceptor(ILogger<CrmExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ToRpcException(ex);
        }
    }

    private RpcException ToRpcException(Exception exception)
    {
        var statusCode = exception switch
        {
            EntityNotFoundException => StatusCode.NotFound,
            ArgumentException => StatusCode.InvalidArgument,
            // невалидные данные клиента (например Guid.Parse кривого id) — это ошибка клиента,
            // а не сервера. FormatException/OverflowException — SystemException, не ArgumentException.
            FormatException => StatusCode.InvalidArgument,
            OverflowException => StatusCode.InvalidArgument,
            OperationCanceledException => StatusCode.Cancelled,
            _ => StatusCode.Internal
        };

        if (statusCode == StatusCode.Internal)
            _logger.LogError(exception, "Unhandled exception in gRPC handler");

        return new RpcException(new Status(statusCode, exception.Message));
    }
}
