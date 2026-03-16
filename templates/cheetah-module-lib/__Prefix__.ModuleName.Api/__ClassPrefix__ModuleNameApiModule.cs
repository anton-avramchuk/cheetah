using Cheetah.Backend.CQRS;
using Cheetah.Backend.Endpoints;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using __Prefix__.ModuleName.Application;

namespace __Prefix__.ModuleName.Api;

[DependsOn(
    typeof(CrmMapsterModule),
    typeof(CrmBackendCQRSModule),
    typeof(CrmBackendEndpointsModule),
    typeof(__ClassPrefix__ModuleNameApplicationModule)
)]
public partial class __ClassPrefix__ModuleNameApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
