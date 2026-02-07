using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Crm.Recruitment.Contracts;
using Crm.Recruitment.DataAccess;
using Crm.Recruitment.Domain;
using Crm.Recruitment.DomainEvents;

namespace Crm.Recruitment.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmGridModule),
    typeof(CrmRecruitmentDomainModule),
    typeof(CrmRecruitmentDataAccessModule),
    typeof(CrmRecruitmentContractsModule),
    typeof(CrmRecruitmentDomainEventsModule)
)]
public partial class CrmRecruitmentApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}