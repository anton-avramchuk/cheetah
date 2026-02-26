using Cheetah.Blazor.Layout;
using Cheetah.Blazor.Layout.Extensions;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Auth.Components;
using Cheetah.Frontend.Auth.Navigation;
using Cheetah.Frontend.Navigation;
using Cheetah.Frontend.Navigation.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Frontend.Auth;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorLayoutModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
public partial class CrmFrontendAuthModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddAuthorizationCore();
        context.Services.AddHeaderComponent<UserInfoComponent>(order: 100);
        context.Services.AddSingleton<IMenuContributor, UserMenuContributor>();
    }
}
