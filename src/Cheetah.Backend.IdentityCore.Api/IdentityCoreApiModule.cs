using Cheetah.Backend.IdentityCore.Application;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.IdentityCore.Api;

[DependsOn(typeof(IdentityCoreApplicationModule))]
public class IdentityCoreApiModule : CrmModule
{
}