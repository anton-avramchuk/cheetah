using Cheetah.Contracts.Attributes;
using Cheetah.Core.Modularity;
using Crm.Recruitment.Contracts;

namespace Crm.Recruitment.ApiClient;

[GenerateApiClient("Recruitment")]
[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmRecruitmentContractsModule))]
public partial class CrmRecruitmentApiClientModule : CrmModule
{
}