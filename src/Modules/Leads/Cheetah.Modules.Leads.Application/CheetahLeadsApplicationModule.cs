using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Domain;
using Cheetah.Modules.Leads.DomainEvents;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Application;

/// <summary>
/// Прикладной слой шаблонного модуля Leads: generic CQRS + конечный автомат жизненного цикла лида.
/// Конфигурация автомата (<see cref="LeadStatus"/>) не зависит от конкретного типа и живёт здесь;
/// закрытые generic-handler'ы и реализацию <c>ILeadConversionOrchestrator</c> подключает наследник.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmStateMachineModule),
    typeof(CheetahLeadsDomainModule),
    typeof(CheetahLeadsContractsModule),
    typeof(CheetahLeadsDomainEventsModule))]
public partial class CheetahLeadsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        // Конечный автомат жизненного цикла лида.
        services.AddStateMachine<LeadStatus>(sm => sm
            .From(LeadStatus.New).To(LeadStatus.Working, LeadStatus.Qualified, LeadStatus.Disqualified)
            .From(LeadStatus.Working).To(LeadStatus.Qualified, LeadStatus.Disqualified)
            .From(LeadStatus.Qualified).To(LeadStatus.Converted, LeadStatus.Disqualified));

        RegisterServices(services);
    }
}
