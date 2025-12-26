using Cheetah.Core.DependencyInjection;

namespace Cheetah.Core;

public class ApplicationInitializationContext : IServiceProviderAccessor
{
    public IServiceProvider ServiceProvider { get; set; }

    public ApplicationInitializationContext(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}