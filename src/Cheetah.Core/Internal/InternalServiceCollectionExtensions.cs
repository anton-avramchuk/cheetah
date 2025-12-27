using Cheetah.Core.Configuration;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Logging;
using Cheetah.Core.Modularity;
using Cheetah.Core.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MosaicCRM.Core.Reflection;

namespace Cheetah.Core.Internal;

internal static class InternalServiceCollectionExtensions
{
    internal static void AddCoreServices(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddLogging();
    }

    internal static void AddCoreCrmServices(this IServiceCollection services,
        ICrmApplication crmApplication,
        CrmApplicationCreationOptions applicationCreationOptions)
    {
        var moduleLoader = new ModuleLoader();
        var assemblyFinder = new AssemblyFinder(crmApplication);
        var typeFinder = new TypeFinder(assemblyFinder);

        if (!services.IsAdded<IConfiguration>())
        {
            services.ReplaceConfiguration(
                ConfigurationHelper.BuildConfiguration(
                    applicationCreationOptions.Configuration
                )
            );
        }

        services.TryAddSingleton<IModuleLoader>(moduleLoader);
        services.TryAddSingleton<IAssemblyFinder>(assemblyFinder);
        services.TryAddSingleton<ITypeFinder>(typeFinder);
        services.TryAddSingleton<IInitLoggerFactory>(new DefaultInitLoggerFactory());

       // services.AddAssemblyOf<ICrmApplication>();

        //services.AddTransient(typeof(ISimpleStateCheckerManager<>), typeof(SimpleStateCheckerManager<>));

        services.Configure<CrmModuleLifecycleOptions>(options =>
        {
            options.Contributors.Add<OnApplicationInitializationModuleLifecycleContributor>();
        });
    }
}