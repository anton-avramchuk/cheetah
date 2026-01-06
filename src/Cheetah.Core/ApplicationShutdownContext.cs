namespace Cheetah.Core;

public class ApplicationShutdownContext(IServiceProvider serviceProvider)
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
}