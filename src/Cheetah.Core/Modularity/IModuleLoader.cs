using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Modularity;

public interface IModuleLoader
{

    ICrmModuleDescriptor[] LoadModules(
        IServiceCollection services,
        Type startupModuleType
    );
}