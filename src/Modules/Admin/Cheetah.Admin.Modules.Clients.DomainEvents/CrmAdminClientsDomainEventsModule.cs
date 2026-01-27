using Cheetah.Core.Modularity;

namespace Cheetah.Admin.Modules.Clients.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule))]

public partial class CrmAdminClientsDomainEventsModule:CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}