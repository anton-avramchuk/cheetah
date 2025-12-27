using Cheetah.Core.Extensions.Collections;

namespace Cheetah.Core.Modularity;

public interface IModuleLifecycleContributor
{
    Task InitializeAsync(ApplicationInitializationContext context, ICrmModule module);

    void Initialize(ApplicationInitializationContext context, ICrmModule module);
}