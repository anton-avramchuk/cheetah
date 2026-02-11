using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Crm.Candidates.DomainEvents;

namespace Crm.Candidates.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule), typeof(CrmCandidatesDomainEventsModule))]
public partial class CrmCandidatesDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}