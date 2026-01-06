using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Tests.Core;

public abstract class CrmIntegratedTest<TStartupModule> : CrmTestBaseWithServiceProvider, IDisposable
    where TStartupModule : ICrmModule
{
    protected ICrmApplication Application { get; }

    protected IServiceProvider RootServiceProvider { get; }

    protected IServiceScope TestServiceScope { get; }

    protected CrmIntegratedTest()
    {
        var services = CreateServiceCollection();

        BeforeAddApplication(services);

        var application = services.AddApplication<TStartupModule>(SetAbpApplicationCreationOptions);
        Application = application;

        AfterAddApplication(services);

        RootServiceProvider = CreateServiceProvider(services);
        TestServiceScope = RootServiceProvider.CreateScope();

        application.Initialize(TestServiceScope.ServiceProvider);
        ServiceProvider = Application.ServiceProvider;

        AfterInitialize();
    }

    protected virtual IServiceCollection CreateServiceCollection()
    {
        return new ServiceCollection();
    }

    protected virtual void BeforeAddApplication(IServiceCollection services)
    {

    }

    protected virtual void SetAbpApplicationCreationOptions(CrmApplicationCreationOptions options)
    {

    }

    protected virtual void AfterAddApplication(IServiceCollection services)
    {

    }

    protected virtual IServiceProvider CreateServiceProvider(IServiceCollection services)
    {
        return services.BuildServiceProviderFromFactory();
    }

    protected virtual void AfterInitialize()
    {

    }

    public virtual void Dispose()
    {
        Application.Shutdown();
        TestServiceScope.Dispose();
        if (RootServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        Application.Dispose();
    }
}