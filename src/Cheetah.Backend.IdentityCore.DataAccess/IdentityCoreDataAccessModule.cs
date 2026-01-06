using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Backend.IdentityCore.Shared;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.Modularity;
using Cheetah.Core.Security;

namespace Cheetah.Backend.IdentityCore.DataAccess;

[DependsOn(typeof(IdentityCoreDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmCoreSecurityModule))]
[DependsOn(typeof(CrmIdentityCoreSharedModule))]
public class IdentityCoreDataAccessModule : CrmModule
{
}