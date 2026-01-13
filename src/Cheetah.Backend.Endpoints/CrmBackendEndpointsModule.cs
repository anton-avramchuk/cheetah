using Cheetah.AspNetCore.Contracts;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.Endpoints;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAspNetCoreContractsModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
public class CrmBackendEndpointsModule : CrmModule
{
}