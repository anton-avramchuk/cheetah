using Cheetah.Core.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core;

internal class CrmApplicationWithInternalServiceProvider : CrmApplicationBase, ICrmApplicationWithInternalServiceProvider
{
    public IServiceScope? ServiceScope { get; private set; }

    public CrmApplicationWithInternalServiceProvider(
        Type startupModuleType,
        Action<CrmApplicationCreationOptions>? optionsAction
    ) : this(
        startupModuleType,
        new ServiceCollection(),
        optionsAction)
    {

    }

    private CrmApplicationWithInternalServiceProvider(
        Type startupModuleType,
        IServiceCollection services,
        Action<CrmApplicationCreationOptions>? optionsAction
    ) : base(
        startupModuleType,
        services,
        optionsAction)
    {
        Services.AddSingleton<ICrmApplicationWithInternalServiceProvider>(this);
    }

    public IServiceProvider CreateServiceProvider()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (ServiceProvider != null)
        {
            return ServiceProvider;
        }

        ServiceScope = Services.BuildServiceProviderFromFactory().CreateScope();
        SetServiceProvider(ServiceScope.ServiceProvider);

        return ServiceProvider!;
    }

    public async Task InitializeAsync()
    {
        CreateServiceProvider();
        await InitializeModulesAsync();
    }

    public void Initialize()
    {
        CreateServiceProvider();
        InitializeModules();
    }

    public override void Dispose()
    {
        base.Dispose();
        ServiceScope?.Dispose();
    }
}