using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Crm.Customer.Contracts;
using Crm.Customer.Domain;
using Crm.Customer.DomainEvents;

namespace Crm.Customer.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmGridModule),
    typeof(CrmCustomerDomainModule),
    typeof(CrmCustomerContractsModule),
    typeof(CrmCustomerDomainEventsModule)
)]
public partial class CrmCustomerApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}