using Cheetah.Admin.Modules.Clients.Frontend;
using Cheetah.Blazor;
using Cheetah.Blazor.Layout;
using Cheetah.Blazor.Layout.Extensions;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Auth;
using Cheetah.Frontend.Navigation;
using Cheetah.Mapping.Mapster;
using Crm.Identity.Frontend;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Admin.Client;

[DependsOn(typeof(CoreModule))]
[Bootstrapper]
[DependsOn(typeof(CrmBlazorModule))]
[DependsOn(typeof(CrmBlazorLayoutModule))]
[DependsOn(typeof(CrmAdminClientsFrontendModule))]
[DependsOn(typeof(CrmFrontendAuthModule))]
[DependsOn(typeof(CrmIdentityFrontendModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
[DependsOn(typeof(CrmMapsterModule))]
public partial class AdminClientBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.ConfigureCrmLayout(config =>
        {
            config.ApplicationName = "Cheetah Admin";
            config.HomeUrl = "/";
            config.SidebarCollapsedByDefault = false;
        });

        // Явно подключаем JWT-хендлер ко всем HTTP-клиентам этого приложения.
        // Допустимо, так как Admin Client обращается исключительно к CRM-бэкенду.
        context.Services.ConfigureHttpClientDefaults(b =>
            b.AddHttpMessageHandler<JwtAuthorizationMessageHandler>());
    }
}
