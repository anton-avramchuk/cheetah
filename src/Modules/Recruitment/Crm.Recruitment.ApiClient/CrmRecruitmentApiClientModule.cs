using Cheetah.Core.Modularity;
using Crm.Recruitment.Contracts;

namespace Crm.Recruitment.ApiClient;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmRecruitmentContractsModule))]
public class CrmRecruitmentApiClientModule : CrmModule
{
}