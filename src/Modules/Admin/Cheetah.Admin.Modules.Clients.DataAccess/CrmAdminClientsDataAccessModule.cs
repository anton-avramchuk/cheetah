using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Core;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Modularity;

namespace Cheetah.Admin.Modules.Clients.DataAccess;

[DependsOn(typeof(CoreModule),typeof(CrmEntityFrameworkPostgreSqlModule))]
[DependsOn(typeof(CrmEntityFrameworkModule),typeof(CrmAdminClientsDomainModule))]

public partial class CrmAdminClientsDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}