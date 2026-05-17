using Cheetah.AspNetCore.Contracts;
using Cheetah.Contracts;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.RateLimit;

namespace Cheetah.Backend.Endpoints;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmContractsModule))]
[DependsOn(typeof(CrmAspNetCoreContractsModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmRateLimitModule))]
public class CrmBackendEndpointsModule : CrmModule
{
}