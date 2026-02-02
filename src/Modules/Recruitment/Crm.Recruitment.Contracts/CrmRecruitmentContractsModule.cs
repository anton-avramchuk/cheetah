using Cheetah.Contracts;
using Cheetah.Core.Modularity;

namespace Crm.Recruitment.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule))]
public class CrmRecruitmentContractsModule : CrmModule
{
}