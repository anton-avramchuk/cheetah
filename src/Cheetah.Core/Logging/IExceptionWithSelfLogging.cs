using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Logging;

public interface IExceptionWithSelfLogging
{
    void Log(ILogger logger);
}