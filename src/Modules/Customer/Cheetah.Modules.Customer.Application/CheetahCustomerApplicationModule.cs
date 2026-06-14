using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain;
using Cheetah.Modules.Customer.DomainEvents;

namespace Cheetah.Modules.Customer.Application;

[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CheetahCustomerDomainModule),
    typeof(CheetahCustomerContractsModule),
    typeof(CheetahCustomerDomainEventsModule))]
public partial class CheetahCustomerApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
