using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.Calendar.DomainEvents;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule), typeof(CrmDataAccessModule))]
[DependsOn(typeof(CheetahCalendarSharedModule), typeof(CheetahCalendarDomainEventsModule))]
public partial class CheetahCalendarDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
