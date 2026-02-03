using Cheetah.Admin.Modules.Clients.Api.Client;
using Cheetah.Admin.Modules.Clients.Frontend.Mapping;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;
using Cheetah.Mapping.Core;
using Cheetah.Mapping.Mapster;
using Cheetah.Mapping.Mapster.DependencyInjection;

namespace Cheetah.Admin.Modules.Clients.Frontend;

[DependsOn(typeof(Cheetah.Blazor.Components.CrmBlazorComponentsModule))]
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAdminClientsApiClientModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
[DependsOn(typeof(CrmMappingCoreModule))]
[DependsOn(typeof(CrmMapsterModule))]
public partial class CrmAdminClientsFrontendModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddMapping<ClientsMapping>();
    }
}