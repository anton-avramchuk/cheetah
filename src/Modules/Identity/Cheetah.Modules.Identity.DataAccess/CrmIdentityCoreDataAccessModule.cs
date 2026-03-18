using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Identity.Domain;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Identity.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEntityFrameworkModule), typeof(CheetahIdentityDomainModule))]
[DependsOn(typeof(Cheetah.Core.Security.CrmCoreSecurityModule))]
public partial class CrmIdentityCoreDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
