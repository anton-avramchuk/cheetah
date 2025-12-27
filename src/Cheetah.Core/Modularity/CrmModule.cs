using System.Reflection;
using Cheetah.Core.Exceptions;

namespace Cheetah.Core.Modularity;

public abstract class CrmModule : ICrmModule, IOnApplicationInitialization
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
                //throw new CrmException($"{nameof(ServiceConfigurationContext)} is only available in the {nameof(ConfigureServices)}, {nameof(PreConfigureServices)} and {nameof(PostConfigureServices)} methods.");
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
}