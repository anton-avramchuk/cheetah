using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;

namespace Crm.Candidates.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule))]
public partial class CrmCandidatesDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}