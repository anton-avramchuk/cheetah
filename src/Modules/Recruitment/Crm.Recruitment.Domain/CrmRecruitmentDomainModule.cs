using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;

namespace Crm.Recruitment.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule))]
[DependsOn(typeof(Cheetah.Core.Events.CrmEventsCoreModule))]
[DependsOn(typeof(CrmDataAccessModule))]
public partial class CrmRecruitmentDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}