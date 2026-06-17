using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.NotesTimeline.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEventsCoreModule))]
public class CheetahNotesTimelineDomainEventsModule : CrmModule
{
}
