using Cheetah.Core.DependencyInjection;

namespace Cheetah.Core;

public class ApplicationInitializationContext(IServiceProvider serviceProvider) : IServiceProviderAccessor
{
    public IServiceProvider ServiceProvider { get; set; } = serviceProvider;
}