using Microsoft.Extensions.Logging;

namespace Cheetah.Core.ExceptionHandling;

public static class ExceptionNotifierExtensions
{
    public static Task NotifyAsync(
        this IExceptionNotifier exceptionNotifier,
        Exception exception,
        LogLevel? logLevel = null,
        bool handled = true)
    {

        return exceptionNotifier.NotifyAsync(
            new ExceptionNotificationContext(
                exception,
                logLevel,
                handled
            )
        );
    }
}