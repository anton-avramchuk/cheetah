using Cheetah.AspNetCore;
using Cheetah.Backend.Grpc;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Application;
using Cheetah.Modules.FeatureManagement.Contracts;

namespace Cheetah.Modules.FeatureManagement.Grpc;

/// <summary>
/// Opt-in gRPC-транспорт FeatureManagement: <see cref="FeatureCatalogGrpcService"/> (evaluate +
/// снимок definitions) поверх тех же CQRS/порта, что и REST. Подключается приложением рядом с
/// <c>.Default</c> (или своей реализацией шаблона); хосту нужен HTTP/2
/// (<c>Kestrel:EndpointDefaults:Protocols = Http1AndHttp2</c>).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmBackendGrpcModule),
    typeof(CrmFeatureManagementModule),
    typeof(CheetahFeatureManagementApplicationModule),
    typeof(CheetahFeatureManagementContractsModule))]
public partial class CheetahFeatureManagementGrpcModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
