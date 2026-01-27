using Cheetah.Admin.Modules.Clients.DomainEvents;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;

namespace Cheetah.Admin.Modules.Clients.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule), typeof(CrmAdminClientsDomainEventsModule))]
public partial class CrmAdminClientsDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}