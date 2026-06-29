using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cheetah.AspNetCore.Middleware;

/// <summary>
/// Global exception handler that converts domain exceptions to appropriate HTTP responses
/// </summary>
public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Validation Error"),
            EntityNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            // Прикладные/бизнес-исключения фреймворка (валидация, нарушение инвариантов, IdentityException
            // и т.п.) несут безопасное для показа сообщение — отдаём 400 с ним, а не generic 500.
            CrmException => (StatusCodes.Status400BadRequest, "Validation Error"),
            _ => (0, null)
        };

        if (statusCode == 0)
            return false;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
