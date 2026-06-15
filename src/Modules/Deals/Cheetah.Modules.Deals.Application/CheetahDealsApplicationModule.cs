using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Deals.Contracts;
using Cheetah.Modules.Deals.Domain;
using Cheetah.Modules.Deals.DomainEvents;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Application;

/// <summary>
/// Прикладной слой Deals: CQRS воронок/сделок/доски + конечный автомат статуса сделки.
/// Интеграционные события публикуются через шину (Outbox) в той же транзакции, что и SaveChangesAsync.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmStateMachineModule),
    typeof(CheetahDealsDomainModule),
    typeof(CheetahDealsContractsModule),
    typeof(CheetahDealsDomainEventsModule))]
public partial class CheetahDealsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        // Конечный автомат статуса сделки: Open → Won|Lost; reopen Won|Lost → Open.
        services.AddStateMachine<DealStatus>(sm => sm
            .From(DealStatus.Open).To(DealStatus.Won, DealStatus.Lost)
            .From(DealStatus.Won).To(DealStatus.Open)
            .From(DealStatus.Lost).To(DealStatus.Open));

        RegisterServices(services);
    }
}
