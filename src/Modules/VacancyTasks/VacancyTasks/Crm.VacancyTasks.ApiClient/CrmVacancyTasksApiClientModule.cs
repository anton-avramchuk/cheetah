using Cheetah.Contracts.Attributes;
using Cheetah.Core.Modularity;
using Crm.VacancyTasks.Contracts;

namespace Crm.VacancyTasks.ApiClient;

[GenerateApiClient("VacancyTasks")]
[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmVacancyTasksContractsModule))]
public partial class CrmVacancyTasksApiClientModule : CrmModule
{
}