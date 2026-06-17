using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.SalesDocuments.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEventsCoreModule))]
public class CheetahSalesDocumentsDomainEventsModule : CrmModule
{
}
