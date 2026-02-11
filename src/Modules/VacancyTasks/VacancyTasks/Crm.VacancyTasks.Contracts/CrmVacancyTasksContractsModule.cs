using Cheetah.Contracts;
using Cheetah.Core.Modularity;

namespace Crm.VacancyTasks.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule))]
public class CrmVacancyTasksContractsModule : CrmModule
{
}