using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Crm.MasterData.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.MasterData.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmMasterDataDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule)
)]
public partial class CrmMasterDataDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddApplicationDbContext<MasterDataDbContext>();
        context.Services.AddScoped<MasterDataDbContext>();
        context.Services.AddDatabaseMigrator<MasterDataDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<MasterDataDbContext>(); });
    }
}