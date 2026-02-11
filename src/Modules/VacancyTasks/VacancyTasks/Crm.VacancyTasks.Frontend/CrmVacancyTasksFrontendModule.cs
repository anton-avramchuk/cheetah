using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;
using Crm.VacancyTasks.ApiClient;
using Crm.VacancyTasks.Contracts;

namespace Crm.VacancyTasks.Frontend;

[DependsOn(typeof(Cheetah.Blazor.Components.CrmBlazorComponentsModule))]
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmVacancyTasksApiClientModule))]
[DependsOn(typeof(CrmVacancyTasksContractsModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
public partial class CrmVacancyTasksFrontendModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}