using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.Modularity;
using Cheetah.Core.Security;

namespace Cheetah.Backend.IdentityCore.DataAccess;

[DependsOn(typeof(IdentityCoreDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmCoreSecurityModule))]
public class IdentityCoreDataAccessModule : CrmModule
{
}