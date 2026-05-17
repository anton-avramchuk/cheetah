using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Notifications.Email;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmNotificationsModule))]
public partial class CrmNotificationsEmailModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.Configure<SmtpOptions>(services.GetConfiguration().GetSection("Notifications:Smtp"));
        RegisterServices(services);
    }
}
