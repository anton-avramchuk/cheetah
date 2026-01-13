using Cheetah.Backend.IdentityCore.Application;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.IdentityCore.Api;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(IdentityCoreApplicationModule))]
public class IdentityCoreApiModule : CrmModule
{
}