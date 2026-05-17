using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Notifications.Sms;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmNotificationsModule))]
public partial class CrmNotificationsSmsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.Configure<SmsOptions>(services.GetConfiguration().GetSection("Notifications:Sms"));
        services.AddHttpClient(HttpSmsSender.HttpClientName);
        RegisterServices(services);
    }
}
