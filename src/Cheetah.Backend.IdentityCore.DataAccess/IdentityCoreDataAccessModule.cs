using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.IdentityCore.DataAccess;

[DependsOn(typeof(IdentityCoreDomainModule))]
public class IdentityCoreDataAccessModule : CrmModule
{
}