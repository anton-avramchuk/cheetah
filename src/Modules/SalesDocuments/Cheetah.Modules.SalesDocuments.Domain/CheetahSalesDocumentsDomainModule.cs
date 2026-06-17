using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.SalesDocuments.DomainEvents;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Domain;

[DependsOn(typeof(CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule),
    typeof(CrmDataAccessModule), typeof(CrmStateMachineModule))]
[DependsOn(typeof(CheetahSalesDocumentsSharedModule), typeof(CheetahSalesDocumentsDomainEventsModule))]
public partial class CheetahSalesDocumentsDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
