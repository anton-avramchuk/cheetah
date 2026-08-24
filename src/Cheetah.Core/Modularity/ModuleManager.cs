using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.Modularity;

[Export(LifetimeType.Singleton, typeof(IModuleManager))]
public class ModuleManager(
    IModuleContainer moduleContainer,
    ILogger<ModuleManager> logger,
    IOptions<CrmModuleLifecycleOptions> options,
    IServiceProvider serviceProvider)
    : IModuleManager
{
    private readonly IEnumerable<IModuleLifecycleContributor> _lifecycleContributors = options.Value
        .Contributors
        .Select(serviceProvider.GetRequiredService)
        .Cast<IModuleLifecycleContributor>()
        .ToArray();

    public virtual async Task InitializeModulesAsync(ApplicationInitializationContext context)
    {
        foreach (var contributor in _lifecycleContributors)
        {
            foreach (var module in moduleContainer.Modules)
            {
                try
                {
                    await contributor.InitializeAsync(context, module.Instance);
                }
                catch (Exception ex)
                {
                    throw new CrmExceptionInitialization($"An error occurred during the initialize {contributor.GetType().FullName} phase of the module {module.Type.AssemblyQualifiedName}: {ex.Message}. See the inner exception for details.", ex);
                }
            }
        }

        logger.LogInformation("Initialized all CRM modules.");
    }

    public void InitializeModules(ApplicationInitializationContext context)
    {
        foreach (var contributor in _lifecycleContributors)
        {
            foreach (var module in moduleContainer.Modules)
            {
                try
                {
                    contributor.Initialize(context, module.Instance);
                }
                catch (Exception ex)
                {
                    throw new CrmExceptionInitialization($"An error occurred during the initialize {contributor.GetType().FullName} phase of the module {module.Type.AssemblyQualifiedName}: {ex.Message}. See the inner exception for details.", ex);
                }
            }
        }

        logger.LogInformation("Initialized all CRM modules.");
    }

    public virtual async Task ShutdownModulesAsync(ApplicationShutdownContext context)
    {
        // Modules are shut down in reverse initialization order, so a module is torn down
        // before the modules it depends on.
        var modules = moduleContainer.Modules.Reverse().ToList();

        foreach (var contributor in _lifecycleContributors)
        {
            foreach (var module in modules)
            {
                try
                {
                    await contributor.ShutdownAsync(context, module.Instance);
                }
                catch (Exception ex)
                {
                    throw new CrmException($"An error occurred during the shutdown {contributor.GetType().FullName} phase of the module {module.Type.AssemblyQualifiedName}: {ex.Message}. See the inner exception for details.", ex);
                }
            }
        }

        logger.LogInformation("Shut down all CRM modules.");
    }

    public void ShutdownModules(ApplicationShutdownContext context)
    {
        var modules = moduleContainer.Modules.Reverse().ToList();

        foreach (var contributor in _lifecycleContributors)
        {
            foreach (var module in modules)
            {
                try
                {
                    contributor.Shutdown(context, module.Instance);
                }
                catch (Exception ex)
                {
                    throw new CrmException($"An error occurred during the shutdown {contributor.GetType().FullName} phase of the module {module.Type.AssemblyQualifiedName}: {ex.Message}. See the inner exception for details.", ex);
                }
            }
        }

        logger.LogInformation("Shut down all CRM modules.");
    }
}