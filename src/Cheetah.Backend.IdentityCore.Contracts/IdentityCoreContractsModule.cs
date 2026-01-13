using Cheetah.Backend.IdentityCore.Shared;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.IdentityCore.Contracts;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(IdentityCoreSharedModule))]
public class IdentityCoreContractsModule : CrmModule
{
}