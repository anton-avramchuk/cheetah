using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Api;
using Crm.Identity.Application;
using Crm.Identity.DataAccess;
using Crm.Identity.Contracts;
using Crm.Identity.Domain;

namespace Crm.Identity.Api;

[DependsOn(typeof(CoreModule), typeof(CheetahIdentityApiModule), typeof(CrmIdentityApplicationModule), typeof(CrmIdentityDataAccessModule), typeof(CrmIdentityDomainModule), typeof(CrmIdentityContractsModule))]
[Bootstrapper]
public partial class CrmIdentityBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
