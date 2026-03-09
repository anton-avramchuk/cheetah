using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Crm.Candidates.Contracts;
using Crm.Candidates.DataAccess;
using Crm.Candidates.Domain;
using Crm.Candidates.DomainEvents;

namespace Crm.Candidates.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule),
    typeof(CrmCandidatesDomainModule),
    typeof(CrmCandidatesDataAccessModule),
    typeof(CrmCandidatesContractsModule),
    typeof(CrmCandidatesDomainEventsModule)
)]
public partial class CrmCandidatesApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}