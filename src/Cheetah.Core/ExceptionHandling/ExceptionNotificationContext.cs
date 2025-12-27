using Cheetah.Core.Extensions.Common;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.ExceptionHandling;

public class ExceptionNotificationContext
{
    /// <summary>
    /// The exception object.
    /// </summary>
    
    public Exception Exception { get; }

    public LogLevel LogLevel { get; }

    /// <summary>
    /// True, if it is handled.
    /// </summary>
    public bool Handled { get; }

    public ExceptionNotificationContext(
        Exception exception,
        LogLevel? logLevel = null,
        bool handled = true)
    {
        Exception = exception;
        LogLevel = logLevel ?? exception.GetLogLevel();
        Handled = handled;
    }
}