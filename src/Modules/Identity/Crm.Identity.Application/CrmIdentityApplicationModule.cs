using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Crm.Identity.Contracts;
using Crm.Identity.DataAccess;
using Crm.Identity.Domain;
using Crm.Identity.DomainEvents;

namespace Crm.Identity.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmGridModule),
    typeof(CrmIdentityDomainModule),
    typeof(CrmIdentityDataAccessModule),
    typeof(CrmIdentityContractsModule),
    typeof(CrmIdentityDomainEventsModule)
)]
public partial class CrmIdentityApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}