using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Domain;
using Cheetah.Modules.Leads.DomainEvents;

namespace Cheetah.Modules.Leads.Application;

/// <summary>
/// Прикладной слой шаблонного модуля Leads: generic CQRS лидов + lookup-запросы справочников.
/// Статус/источник — данные (справочники), переходы валидируются доменно, поэтому enum-автомат не
/// используется. Закрытые generic-handler'ы и реализацию <c>ILeadConversionOrchestrator</c> подключает
/// наследник; lookup-хендлеры регистрируются генератором ([Export]).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CheetahLeadsDomainModule),
    typeof(CheetahLeadsContractsModule),
    typeof(CheetahLeadsDomainEventsModule))]
public partial class CheetahLeadsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
