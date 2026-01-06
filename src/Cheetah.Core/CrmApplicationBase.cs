using System.Reflection;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Exceptions;
using Cheetah.Core.Extensions.Common;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Internal;
using Cheetah.Core.Logging;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core;

public abstract class CrmApplicationBase : ICrmApplication
{

    public Type StartupModuleType { get; }

    public IServiceProvider ServiceProvider { get; private set; } = default!;

    public IServiceCollection Services { get; }

    public IReadOnlyList<ICrmModuleDescriptor> Modules { get; }

    public string? ApplicationName { get; }

    public string InstanceId { get; } = Guid.NewGuid().ToString();

    private bool _configuredServices;

    internal CrmApplicationBase(
        Type startupModuleType,
        IServiceCollection services,
        Action<CrmApplicationCreationOptions>? optionsAction)
    {
        
        StartupModuleType = startupModuleType;
        Services = services;

        services.TryAddObjectAccessor<IServiceProvider>();

        var options = new CrmApplicationCreationOptions(services);
        optionsAction?.Invoke(options);

        ApplicationName = GetApplicationName(options);

        services.AddSingleton<ICrmApplication>(this);
        services.AddSingleton<IApplicationInfoAccessor>(this);
        services.AddSingleton<IModuleContainer>(this);
        services.AddSingleton<ICrmHostEnvironment>(new CrmHostEnvironment()
        {
            EnvironmentName = options.Environment
        });

        services.AddCoreServices();
        services.AddCoreCrmServices(this, options);

        Modules = LoadModules(services, options);

        if (!options.SkipConfigureServices)
        {
            ConfigureServices();
        }
    }


    public virtual void Dispose()
    {
        //TODO: Shutdown if not done before?
    }

    protected virtual void SetServiceProvider(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        ServiceProvider.GetRequiredService<ObjectAccessor<IServiceProvider>>().Value = ServiceProvider;
    }

    protected virtual async Task InitializeModulesAsync()
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            WriteInitLogs(scope.ServiceProvider);
            await scope.ServiceProvider
                .GetRequiredService<IModuleManager>()
                .InitializeModulesAsync(new ApplicationInitializationContext(scope.ServiceProvider));
        }
    }

    protected virtual void InitializeModules()
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            WriteInitLogs(scope.ServiceProvider);
            scope.ServiceProvider
                .GetRequiredService<IModuleManager>()
                .InitializeModules(new ApplicationInitializationContext(scope.ServiceProvider));
        }
    }

    protected virtual void WriteInitLogs(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<CrmApplicationBase>>();
        if (logger == null)
        {
            return;
        }

        var initLogger = serviceProvider.GetRequiredService<IInitLoggerFactory>().Create<CrmApplicationBase>();

        foreach (var entry in initLogger.Entries)
        {
            logger.Log(entry.LogLevel, entry.EventId, entry.State, entry.Exception, entry.Formatter);
        }

        initLogger.Entries.Clear();
    }

    protected virtual IReadOnlyList<ICrmModuleDescriptor> LoadModules(IServiceCollection services, CrmApplicationCreationOptions options)
    {
        return services
            .GetSingletonInstance<IModuleLoader>()
            .LoadModules(
                services,
                StartupModuleType
            );
    }

    public virtual Task ConfigureServicesAsync()
    {
        CheckMultipleConfigureServices();

        var context = new ServiceConfigurationContext(Services);
        Services.AddSingleton(context);

        foreach (var module in Modules)
        {
            if (module.Instance is CrmModule crmModule)
            {
                crmModule.ServiceConfigurationContext = context;
            }
        }

        //PreConfigureServices
        foreach (var module in Modules.Where(m => m.Instance is IPreConfigureServices))
        {
            try
            {
                ((IPreConfigureServices)module.Instance).PreConfigureServices(context);
            }
            catch (Exception ex)
            {
                throw new CrmExceptionInitialization($"An error occurred during {nameof(IPreConfigureServices.PreConfigureServices)} phase of the module {module.Type.AssemblyQualifiedName}. See the inner exception for details.", ex);
            }
        }


        //ConfigureServices
        foreach (var module in Modules)
        {
            
            try
            {
                module.Instance.ConfigureServices(context);
            }
            catch (Exception ex)
            {
                throw new CrmExceptionInitialization($"An error occurred during {nameof(ICrmModule.ConfigureServices)} phase of the module {module.Type.AssemblyQualifiedName}. See the inner exception for details.", ex);
            }
        }
        
        //PostConfigureServices
        foreach (var module in Modules.Where(m => m.Instance is IPostConfigureServices))
        {
            try
            {
                ((IPostConfigureServices)module.Instance).PostConfigureServices(context);
            }
            catch (Exception ex)
            {
                throw new CrmExceptionInitialization($"An error occurred during {nameof(IPostConfigureServices.PostConfigureServices)} phase of the module {module.Type.AssemblyQualifiedName}. See the inner exception for details.", ex);
            }
        }

        foreach (var module in Modules)
        {
            if (module.Instance is CrmModule crmModule)
            {
                crmModule.ServiceConfigurationContext = null!;
            }
        }

        _configuredServices = true;

        TryToSetEnvironment(Services);
        
        return Task.CompletedTask;
    }

    private void CheckMultipleConfigureServices()
    {
        if (_configuredServices)
        {
            throw new CrmExceptionInitialization("Services have already been configured! If you call ConfigureServicesAsync method, you must have set CrmApplicationCreationOptions.SkipConfigureServices to true before.");
        }
    }

    //TODO: We can extract a new class for this
    public virtual void ConfigureServices()
    {
        CheckMultipleConfigureServices();

        var context = new ServiceConfigurationContext(Services);
        Services.AddSingleton(context);

        foreach (var module in Modules)
        {
            if (module.Instance is CrmModule crmModule)
            {
                crmModule.ServiceConfigurationContext = context;
            }
        }
        
        //PreConfigureServices
        foreach (var module in Modules.Where(m => m.Instance is IPreConfigureServices))
        {
            try
            {
                ((IPreConfigureServices)module.Instance).PreConfigureServices(context);
            }
            catch (Exception ex)
            {
                throw new CrmExceptionInitialization($"An error occurred during {nameof(IPreConfigureServices.PreConfigureServices)} phase of the module {module.Type.AssemblyQualifiedName}. See the inner exception for details.", ex);
            }
        }


        //ConfigureServices
        foreach (var module in Modules)
        {
            
            try
            {
                module.Instance.ConfigureServices(context);
            }
            catch (Exception ex)
            {
                throw new CrmExceptionInitialization($"An error occurred during {nameof(ICrmModule.ConfigureServices)} phase of the module {module.Type.AssemblyQualifiedName}. See the inner exception for details.", ex);
            }
        }

        //PostConfigureServices
        foreach (var module in Modules.Where(m => m.Instance is IPostConfigureServices))
        {
            try
            {
                ((IPostConfigureServices)module.Instance).PostConfigureServices(context);
            }
            catch (Exception ex)
            {
                throw new CrmExceptionInitialization($"An error occurred during {nameof(IPostConfigureServices.PostConfigureServices)} phase of the module {module.Type.AssemblyQualifiedName}. See the inner exception for details.", ex);
            }
        }

        foreach (var module in Modules)
        {
            if (module.Instance is CrmModule crmModule)
            {
                crmModule.ServiceConfigurationContext = null!;
            }
        }

        _configuredServices = true;

        TryToSetEnvironment(Services);
    }

    private static string? GetApplicationName(CrmApplicationCreationOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ApplicationName))
        {
            return options.ApplicationName!;
        }

        var configuration = options.Services.GetConfigurationOrNull();
        if (configuration != null)
        {
            var appNameConfig = configuration["ApplicationName"];
            if (!string.IsNullOrWhiteSpace(appNameConfig))
            {
                return appNameConfig!;
            }
        }

        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            return entryAssembly.GetName().Name;
        }

        return null;
    }

    private static void TryToSetEnvironment(IServiceCollection services)
    {
        var crmHostEnvironment = services.GetSingletonInstance<ICrmHostEnvironment>();
        if (crmHostEnvironment.EnvironmentName.IsNullOrWhiteSpace())
        {
            crmHostEnvironment.EnvironmentName = Environments.Production;
        }
    }
    
    public virtual void Shutdown()
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            scope.ServiceProvider
                .GetRequiredService<IModuleManager>()
                .ShutdownModules(new ApplicationShutdownContext(scope.ServiceProvider));
        }
    }
    
    public virtual async Task ShutdownAsync()
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            await scope.ServiceProvider
                .GetRequiredService<IModuleManager>()
                .ShutdownModulesAsync(new ApplicationShutdownContext(scope.ServiceProvider));
        }
    }
}