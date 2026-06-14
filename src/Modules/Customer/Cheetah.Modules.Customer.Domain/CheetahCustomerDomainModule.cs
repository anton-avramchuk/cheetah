using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.Customer.DomainEvents;
using Cheetah.Modules.Customer.Shared;

namespace Cheetah.Modules.Customer.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule))]
[DependsOn(typeof(CheetahCustomerSharedModule), typeof(CheetahCustomerDomainEventsModule))]
public partial class CheetahCustomerDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
