using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Notifications;

/// <summary>
/// Core-модуль уведомлений. Регистрирует INotificationDispatcher.
/// Конкретные каналы (email, sms, ...) подключаются отдельными модулями:
/// Cheetah.Notifications.Email, Cheetah.Notifications.Sms.
/// </summary>
[DependsOn(typeof(CoreModule))]
public partial class CrmNotificationsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
