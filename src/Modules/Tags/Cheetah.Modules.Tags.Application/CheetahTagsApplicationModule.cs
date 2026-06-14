using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.DomainEvents;
using Cheetah.Modules.Tags.Application.EventHandlers;
using Cheetah.Modules.Tags.Contracts;
using Cheetah.Modules.Tags.Domain;
using Cheetah.Modules.Tags.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Tags.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CheetahTagsDomainModule),
    typeof(CheetahTagsContractsModule),
    typeof(CheetahTagsDomainEventsModule),
    typeof(CheetahIdentityDomainEventsModule))]
public partial class CheetahTagsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();

        // Поддержание локальной реплики пользователей по доменным событиям Identity
        eventBus.Subscribe<UserCreatedEvent, UserCreatedEventHandler>();
        eventBus.Subscribe<UserNameChangedEvent, UserNameChangedEventHandler>();
        eventBus.Subscribe<UserDeletedEvent, UserDeletedEventHandler>();
    }
}
