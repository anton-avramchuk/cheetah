using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Tests.Core;

public class AbpAsyncIntegratedTest<TStartupModule> : CrmTestBaseWithServiceProvider
    where TStartupModule : ICrmModule
{
    protected ICrmApplication Application { get; set; } = default!;

    protected IServiceProvider RootServiceProvider { get; set; } = default!;

    protected IServiceScope TestServiceScope { get; set; } = default!;

    public virtual async Task InitializeAsync()
    {
        var services = await CreateServiceCollectionAsync();

        await BeforeAddApplicationAsync(services);
        var application = await services.AddApplicationAsync<TStartupModule>(await SetCrmApplicationCreationOptionsAsync());
        await AfterAddApplicationAsync(services);

        RootServiceProvider = await CreateServiceProviderAsync(services);
        TestServiceScope = RootServiceProvider.CreateScope();
        await application.InitializeAsync(TestServiceScope.ServiceProvider);
        ServiceProvider = application.ServiceProvider;
        Application = application;

        await InitializeServicesAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await Application.ShutdownAsync();
        if (RootServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        TestServiceScope.Dispose();
        Application.Dispose();
    }

    protected virtual Task<IServiceCollection> CreateServiceCollectionAsync()
    {
        return Task.FromResult<IServiceCollection>(new ServiceCollection());
    }

    protected virtual Task BeforeAddApplicationAsync(IServiceCollection services)
    {
        return Task.CompletedTask;
    }

    protected virtual Task<Action<CrmApplicationCreationOptions>> SetCrmApplicationCreationOptionsAsync()
    {
        return Task.FromResult<Action<CrmApplicationCreationOptions>>(_ => { });
    }

    protected virtual Task AfterAddApplicationAsync(IServiceCollection services)
    {
        return Task.CompletedTask;
    }

    protected virtual Task<IServiceProvider> CreateServiceProviderAsync(IServiceCollection services)
    {
        return Task.FromResult(services.BuildServiceProviderFromFactory());
    }

    protected virtual Task InitializeServicesAsync()
    {
        return Task.CompletedTask;
    }
}