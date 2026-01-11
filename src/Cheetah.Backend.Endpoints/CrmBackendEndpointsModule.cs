using Cheetah.AspNetCore.Contracts;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.Endpoints;

[DependsOn(typeof(CrmAspNetCoreContractsModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
public class CrmBackendEndpointsModule : CrmModule
{
}