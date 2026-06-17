using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.NotesTimeline.DomainEvents;
using Cheetah.Modules.NotesTimeline.Shared;

namespace Cheetah.Modules.NotesTimeline.Domain;

[DependsOn(typeof(CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule),
    typeof(CrmDataAccessModule), typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CheetahNotesTimelineSharedModule), typeof(CheetahNotesTimelineDomainEventsModule))]
public partial class CheetahNotesTimelineDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
