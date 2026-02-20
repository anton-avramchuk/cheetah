using Cheetah.Core.EntityFramework;
using Cheetah.Core.Identity.Domain;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.Identity.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEntityFrameworkModule), typeof(CrmIdentityCoreDomainModule))]
[DependsOn(typeof(Cheetah.Core.Security.CrmCoreSecurityModule))]
public partial class CrmIdentityCoreDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}