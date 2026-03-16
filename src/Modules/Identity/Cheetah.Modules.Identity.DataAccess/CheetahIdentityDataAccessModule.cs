using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Grid;
using Cheetah.Core.Identity.DataAccess;
using Cheetah.Core.Identity.DataAccess.Extensions;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Identity.DataAccess;

[DependsOn(
    typeof(Cheetah.Core.CoreModule),
    typeof(CheetahIdentityDomainModule),
    typeof(CrmIdentityCoreDataAccessModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule)
)]
public partial class CheetahIdentityDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddIdentityContext<IdentityModuleDbContext, CrmIdentityUser, CrmIdentityRole>(_ => { });
        context.Services.AddScoped<IdentityModuleDbContext>();
        context.Services.AddDatabaseMigrator<IdentityModuleDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<IdentityModuleDbContext>(); });
    }
}
