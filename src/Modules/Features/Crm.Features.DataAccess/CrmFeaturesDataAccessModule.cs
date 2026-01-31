using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Modularity;
using Crm.Features.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Features.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmFeaturesDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule)
)]
public partial class CrmFeaturesDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddApplicationDbContext<CrmFeatureDbContext>();
        context.Services.AddScoped<CrmFeatureDbContext>();
        context.Services.AddDatabaseMigrator<CrmFeatureDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options =>
        {
            options.UseNpgsql<CrmFeatureDbContext>();
        });
    }
}