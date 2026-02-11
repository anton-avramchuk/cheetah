using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Crm.Candidates.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEventsCoreModule))]
public class CrmCandidatesDomainEventsModule : CrmModule
{
}