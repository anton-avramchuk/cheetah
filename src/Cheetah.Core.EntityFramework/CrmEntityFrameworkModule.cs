using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.Providers;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.EntityFramework;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDomainModule),typeof(CrmDataAccessModule))]
public partial class CrmEntityFrameworkModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.TryAddTransient(typeof(IDbContextProvider<>), typeof(DbContextProvider<>));
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<CrmEntityFrameworkModule>>();

        logger.LogInformation("Applying database migrations...");

        var migrationManager = context.ServiceProvider.GetService<DatabaseMigrationManager>();

        if (migrationManager != null)
        {
            migrationManager.MigrateAllAsync().GetAwaiter().GetResult();
        }
        else
        {
            logger.LogInformation("No DatabaseMigrationManager registered. Skipping automatic migrations.");
        }
    }
}