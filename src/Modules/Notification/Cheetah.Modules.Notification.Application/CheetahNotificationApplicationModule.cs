using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Email.DomainEvents;
using Cheetah.Modules.Identity.DomainEvents;
using Cheetah.Modules.Notification.Application.EventHandlers;
using Cheetah.Modules.Notification.Application.EventHandlers.Identity;
using Cheetah.Modules.Notification.Contracts;
using Cheetah.Modules.Notification.Domain;
using Cheetah.Modules.Notification.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Notification.Application;

/// <summary>
/// Оркестрация уведомлений. Подписывается на:
/// <list type="bullet">
/// <item>NotificationRequested — намерение от продюсеров → раскладка по каналам;</item>
/// <item>EmailDelivered / EmailFailed — обратные статусы канала Email → обновление Dispatch;</item>
/// <item>UserCreated / UserDeleted (Identity) — поддержание локальной реплики контактов.</item>
/// </list>
/// </summary>
[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CheetahNotificationDomainModule),
    typeof(CheetahNotificationContractsModule),
    typeof(CheetahNotificationDomainEventsModule),
    typeof(CheetahEmailDomainEventsModule),
    typeof(CheetahIdentityDomainEventsModule))]
public partial class CheetahNotificationApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();

        // Вход: намерение уведомить
        eventBus.Subscribe<NotificationRequested, NotificationRequestedHandler>();

        // Обратные статусы канала Email
        eventBus.Subscribe<EmailDelivered, EmailDeliveredHandler>();
        eventBus.Subscribe<EmailFailed, EmailFailedHandler>();

        // Реплика контактов из Identity
        eventBus.Subscribe<UserCreatedEvent, UserCreatedContactHandler>();
        eventBus.Subscribe<UserDeletedEvent, UserDeletedContactHandler>();
    }
}
