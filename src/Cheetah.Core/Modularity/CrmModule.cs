using System.Reflection;
using Cheetah.Core.Exceptions;
using Cheetah.Core.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Modularity;

public abstract class CrmModule : ICrmModule, IOnApplicationInitialization, IPreConfigureServices,
    IPostConfigureServices, IOnPreApplicationInitialization, IOnApplicationShutdown 
{
    internal static void CheckCrmModuleType(Type moduleType)
    {
        if (!IsCrmModule(moduleType))
        {
            throw new ArgumentException("Given type is not an CRM module: " + moduleType.AssemblyQualifiedName);
        }
    }

    public static bool IsCrmModule(Type type)
    {
        var typeInfo = type.GetTypeInfo();

        return
            typeInfo is { IsClass: true, IsAbstract: false, IsGenericType: false } &&
            typeof(ICrmModule).GetTypeInfo().IsAssignableFrom(type);
    }

    protected internal ServiceConfigurationContext ServiceConfigurationContext
    {
        get
        {
            if (_serviceConfigurationContext == null)
            {
                throw new CrmException(
                    $"{nameof(ServiceConfigurationContext)} is only available in the {nameof(ConfigureServices)}, {nameof(PreConfigureServices)} and {nameof(PostConfigureServices)} methods.");
            }

            return _serviceConfigurationContext;
        }
        internal set => _serviceConfigurationContext = value;
    }

    private ServiceConfigurationContext? _serviceConfigurationContext;

    public virtual void ConfigureServices(ServiceConfigurationContext context)
    {
    }

    public virtual Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        OnApplicationInitialization(context);
        return Task.CompletedTask;
    }

    public virtual void OnApplicationInitialization(ApplicationInitializationContext context)
    {
    }


    public virtual void PreConfigureServices(ServiceConfigurationContext context)
    {
    }

    public virtual void PostConfigureServices(ServiceConfigurationContext context)
    {
    }

    public virtual Task OnPreApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        OnPreApplicationInitialization(context);
        return Task.CompletedTask;
    }

    public virtual void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
    }

    protected void Configure<TOptions>(Action<TOptions> configureOptions)
        where TOptions : class
    {
        ServiceConfigurationContext.Services.Configure(configureOptions);
    }

    protected void Configure<TOptions>(string name, Action<TOptions> configureOptions)
        where TOptions : class
    {
        ServiceConfigurationContext.Services.Configure(name, configureOptions);
    }

    protected void Configure<TOptions>(IConfiguration configuration)
        where TOptions : class
    {
        ServiceConfigurationContext.Services.Configure<TOptions>(configuration);
    }

    protected void Configure<TOptions>(IConfiguration configuration, Action<BinderOptions> configureBinder)
        where TOptions : class
    {
        ServiceConfigurationContext.Services.Configure<TOptions>(configuration, configureBinder);
    }

    protected void Configure<TOptions>(string name, IConfiguration configuration)
        where TOptions : class
    {
        ServiceConfigurationContext.Services.Configure<TOptions>(name, configuration);
    }

    protected void PreConfigure<TOptions>(Action<TOptions> configureOptions)
        where TOptions : class
    {
        ServiceConfigurationContext.Services.PreConfigure(configureOptions);
    }

    protected void PostConfigure<TOptions>(Action<TOptions> configureOptions)
        where TOptions : class
    {
        ServiceConfigurationContext.Services.PostConfigure(configureOptions);
    }

    protected void PostConfigureAll<TOptions>(Action<TOptions> configureOptions)
        where TOptions : class
    {
        ServiceConfigurationContext.Services.PostConfigureAll(configureOptions);
    }

    public virtual Task OnApplicationShutdownAsync(ApplicationShutdownContext context)
    {
        OnApplicationShutdown(context);
        return Task.CompletedTask;
    }

    public virtual void OnApplicationShutdown(ApplicationShutdownContext context)
    {

    }
}