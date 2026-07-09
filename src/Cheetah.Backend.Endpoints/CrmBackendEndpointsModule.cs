using Cheetah.AspNetCore.Contracts;
using Cheetah.Contracts;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.FeatureManagement;
using Cheetah.RateLimit;

namespace Cheetah.Backend.Endpoints;

// CrmFeatureManagementModule — для декларативного RequireFeature(...): движок лёгкий (без БД);
// гейт активен только когда приложение подключило источник определений (IFeatureDefinitionProvider).
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmContractsModule))]
[DependsOn(typeof(CrmAspNetCoreContractsModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmRateLimitModule))]
[DependsOn(typeof(CrmFeatureManagementModule))]
public class CrmBackendEndpointsModule : CrmModule
{
}