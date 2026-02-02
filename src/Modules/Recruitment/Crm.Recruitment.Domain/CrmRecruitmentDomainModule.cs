using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;

namespace Crm.Recruitment.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule))]
public partial class CrmRecruitmentDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}