using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.DataAccess;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Contracts;
using Cheetah.Modules.Identity.Domain;
using Cheetah.Modules.Identity.DomainEvents;
using Crm.Identity.Domain;

namespace Cheetah.Modules.Identity.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule),
    typeof(CrmIdentityCoreDataAccessModule),
    typeof(CheetahIdentityDomainModule),
    typeof(CrmIdentityDomainModule),
    typeof(CheetahIdentityContractsModule),
    typeof(CheetahIdentityDomainEventsModule)
)]
public partial class CheetahIdentityApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
