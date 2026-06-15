using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.Leads.DomainEvents;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Domain;

[DependsOn(typeof(CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule),
    typeof(CrmDataAccessModule))]
[DependsOn(typeof(CheetahLeadsSharedModule), typeof(CheetahLeadsDomainEventsModule))]
public partial class CheetahLeadsDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
