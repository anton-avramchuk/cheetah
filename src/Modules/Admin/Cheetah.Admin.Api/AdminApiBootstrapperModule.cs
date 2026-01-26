using Cheetah.Admin.Modules.Clients.Api;
using Cheetah.AspNetCore;
using Cheetah.Backend.CQRS;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Scalar;

namespace Cheetah.Admin.Api;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmAspNetCoreModule), typeof(ScalarModule))]
[DependsOn(typeof(CrmAdminClientsApiModule))]
[DependsOn(typeof(CrmMapsterModule),typeof(CrmBackendCQRSModule))]
[Bootstrapper]
public partial class AdminApiBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}