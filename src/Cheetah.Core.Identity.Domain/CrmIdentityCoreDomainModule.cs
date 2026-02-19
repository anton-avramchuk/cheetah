using Cheetah.Core.Modularity;

namespace Cheetah.Core.Identity.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule))]
[DependsOn(typeof(Cheetah.Core.Domain.CrmDomainModule))]
public partial class CrmIdentityCoreDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
