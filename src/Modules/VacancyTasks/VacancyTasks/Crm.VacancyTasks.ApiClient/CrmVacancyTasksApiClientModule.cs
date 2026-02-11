using Cheetah.Core.Modularity;
using Crm.VacancyTasks.Contracts;

namespace Crm.VacancyTasks.ApiClient;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmVacancyTasksContractsModule))]
public partial class CrmVacancyTasksApiClientModule : CrmModule
{
}