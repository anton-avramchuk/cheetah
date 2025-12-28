using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Domain;
using Cheetah.Core.Extensions.Collections;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.DataAccess;

[DependsOn(typeof(CrmDomainModule))]
public partial class CrmDataAccessModule : CrmModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        AutoAddDataSeedContributors(context.Services);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        
        var configuration = context.Services.GetConfiguration();

        Configure<CrmDbConnectionOptions>(configuration);

        context.Services.AddSingleton(typeof(IDataFilter<>), typeof(DataFilter<>));
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        Configure<CrmDbConnectionOptions>(options =>
        {
            options.Databases.RefreshIndexes();
        });
    }

    private static void AutoAddDataSeedContributors(IServiceCollection services)
    {
        var contributors = new List<Type>();

        services.OnRegistered(context =>
        {
            if (typeof(IDataSeedContributor).IsAssignableFrom(context.ImplementationType))
            {
                contributors.Add(context.ImplementationType);
            }
        });

        services.Configure<CrmDataSeedOptions>(options =>
        {
            options.Contributors.AddIfNotContains(contributors);
        });
    }
}