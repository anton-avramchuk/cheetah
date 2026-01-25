using Cheetah.Backend.Endpoints;
using Cheetah.Backend.IdentityCore.Application;
using Cheetah.Backend.IdentityCore.DataAccess;
using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.IdentityCore.Api;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(IdentityCoreApplicationModule))]
[DependsOn(typeof(IdentityCoreDomainModule))]
[DependsOn(typeof(IdentityCoreDataAccessModule))]
[DependsOn(typeof(CrmBackendEndpointsModule))]
public class IdentityCoreApiModule : CrmModule
{
}