using Cheetah.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core;

internal class CrmApplicationWithExternalServiceProvider : CrmApplicationBase, ICrmApplicationWithExternalServiceProvider
{
    public CrmApplicationWithExternalServiceProvider(
        Type startupModuleType,
        IServiceCollection services,
        Action<CrmApplicationCreationOptions>? optionsAction
    ) : base(
        startupModuleType,
        services,
        optionsAction)
    {
        services.AddSingleton<ICrmApplicationWithExternalServiceProvider>(this);
    }

    void ICrmApplicationWithExternalServiceProvider.SetServiceProvider(IServiceProvider serviceProvider)
    {

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (ServiceProvider != null)
        {
            if (ServiceProvider != serviceProvider)
            {
                throw new CrmException("Service provider was already set before to another service provider instance.");
            }

            return;
        }

        SetServiceProvider(serviceProvider);
    }

    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        SetServiceProvider(serviceProvider);

        await InitializeModulesAsync();
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        SetServiceProvider(serviceProvider);

        InitializeModules();
    }

    public override void Dispose()
    {
        base.Dispose();

        if (ServiceProvider is IDisposable disposableServiceProvider)
        {
            disposableServiceProvider.Dispose();
        }
    }
}