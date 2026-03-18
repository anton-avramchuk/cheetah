using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Application;
using Crm.Identity.Domain;

namespace Crm.Identity.Application;

[DependsOn(typeof(CheetahIdentityApplicationModule), typeof(CrmIdentityDomainModule))]
public partial class CrmIdentityApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
