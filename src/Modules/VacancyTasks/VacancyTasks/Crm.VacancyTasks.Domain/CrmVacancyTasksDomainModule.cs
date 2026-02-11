using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;

namespace Crm.VacancyTasks.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule))]
public partial class CrmVacancyTasksDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}