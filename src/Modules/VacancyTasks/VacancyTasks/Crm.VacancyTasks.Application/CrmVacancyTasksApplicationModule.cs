using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Crm.VacancyTasks.Contracts;
using Crm.VacancyTasks.DataAccess;
using Crm.VacancyTasks.Domain;
using Crm.VacancyTasks.DomainEvents;

namespace Crm.VacancyTasks.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule),
    typeof(CrmVacancyTasksDomainModule),
    typeof(CrmVacancyTasksDataAccessModule),
    typeof(CrmVacancyTasksContractsModule),
    typeof(CrmVacancyTasksDomainEventsModule)
)]
public partial class CrmVacancyTasksApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}