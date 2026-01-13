using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Backend.IdentityCore.Shared;
using Cheetah.Core;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.Modularity;
using Cheetah.Core.Security;

namespace Cheetah.Backend.IdentityCore.DataAccess;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(IdentityCoreDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmCoreSecurityModule))]
[DependsOn(typeof(IdentityCoreSharedModule))]
public class IdentityCoreDataAccessModule : CrmModule
{
}