using Cheetah.BackgroundTasks;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Cheetah.DistributedLock;
using Cheetah.Modules.Calendar.Application.Options;
using Cheetah.Modules.Calendar.Contracts;
using Cheetah.Modules.Calendar.Domain;
using Cheetah.Modules.Calendar.DomainEvents;
using Cheetah.Modules.Notification.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Calendar.Application;

/// <summary>
/// Прикладной слой Calendar: CQRS событий/календарей/участников/напоминаний, материализация
/// срабатываний и фоновые задачи рассылки. Calendar — продюсер уведомлений: публикует
/// <c>NotificationRequested</c> (зависит от Notification только на уровне Events).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmBackgroundTasksModule),
    typeof(CrmDistributedLockModule),
    typeof(CheetahCalendarDomainModule),
    typeof(CheetahCalendarContractsModule),
    typeof(CheetahCalendarDomainEventsModule),
    typeof(CheetahNotificationDomainEventsModule))]
public partial class CheetahCalendarApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.AddOptions<CalendarReminderOptions>()
            .Bind(services.GetConfiguration().GetSection("Calendar:Reminders"));

        RegisterServices(services);
    }
}
