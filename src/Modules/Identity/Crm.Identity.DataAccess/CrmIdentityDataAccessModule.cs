using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Grid;
using Cheetah.Core.Identity.DataAccess;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Core.Identity.DataAccess.Extensions;
using Crm.Identity.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Identity.DataAccess;

[DependsOn(
    typeof(Cheetah.Core.CoreModule),
    typeof(CrmIdentityDomainModule),
    typeof(CrmIdentityCoreDataAccessModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule)
)]
public partial class CrmIdentityDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddIdentityContext<IdentityDbContext, CrmUser, CrmRole>(_ => { });
        
        context.Services.AddDatabaseMigrator<IdentityDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<IdentityDbContext>(); });
    }
}
