using Cheetah.Core.Modularity;

namespace Cheetah.Admin.Modules.Clients.DataAccess.Mongo;

[DependsOn(typeof(Cheetah.Core.CoreModule))]

public partial class CrmAdminClientsDataAccessMongoModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}