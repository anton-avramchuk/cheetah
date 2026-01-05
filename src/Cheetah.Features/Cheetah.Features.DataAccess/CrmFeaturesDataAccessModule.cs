using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.Tenants;
using Cheetah.Core.Modularity;
using Cheetah.Features.Domain;
using Cheetah.Features.Events;
using Cheetah.Features.Shared;
using Cheetah.Tenants.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Features.DataAccess;

/// <summary>
/// Features Data Access Module - Configures database access for Features
/// </summary>
[DependsOn(typeof(CrmFeaturesDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
[DependsOn(typeof(CrmEntityFrameworkTenantsModule))]
[DependsOn(typeof(CrmFeaturesSharedModule))]
public partial class CrmFeaturesDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        // Register DbContext
        context.Services.AddDbContext<FeaturesDbContext>((sp, options) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("Features");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'Features' is not configured");
            }
            options.UseNpgsql(connectionString);
        });

        // Register as tenant-based DbContext for automatic migration
        context.Services.AddSingleton<ITenantBasedDbContext<TenantCreatedEvent>>(sp =>
            new FeaturesTenantDbContextProvider());
    }
}
