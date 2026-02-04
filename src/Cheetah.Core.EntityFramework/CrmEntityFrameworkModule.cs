using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.Providers;
using Cheetah.Core.EntityFramework.Seeding;
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

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<CrmEntityFrameworkModule>>();

        // Apply migrations
        var migrationManager = context.ServiceProvider.GetService<DatabaseMigrationManager>();
        if (migrationManager != null)
        {
            logger.LogInformation("Applying database migrations...");
            await migrationManager.MigrateAllAsync();
        }

        // Seed default data
        var seedManager = context.ServiceProvider.GetService<DatabaseSeedManager>();
        if (seedManager != null)
        {
            logger.LogInformation("Seeding database with default data...");
            await seedManager.SeedAllAsync();
        }
    }
}
