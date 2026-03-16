using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Domain;

namespace Crm.Identity.Domain;

[DependsOn(typeof(CheetahIdentityDomainModule))]
public partial class CrmIdentityDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
