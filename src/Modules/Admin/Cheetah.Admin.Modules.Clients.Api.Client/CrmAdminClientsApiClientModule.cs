using Cheetah.Admin.Modules.Clients.Contracts;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.Admin.Modules.Clients.Api.Client;

[DependsOn(typeof(Cheetah.Core.CoreModule))]
[DependsOn(typeof(CrmAdminClientsContractsModule))]
public partial class CrmAdminClientsApiClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddOptions<CrmAdminClientsApiClientOptions>()
            .BindConfiguration("AdminClients");

        context.Services.AddHttpClient<IAdminClientsService, AdminClientsService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<CrmAdminClientsApiClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
    }
}
