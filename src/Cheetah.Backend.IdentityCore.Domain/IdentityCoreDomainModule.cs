using Cheetah.Backend.IdentityCore.Events;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.IdentityCore.Domain;

[DependsOn(typeof(CrmDomainModule))]
[DependsOn(typeof(CrmIdentityCoreEventsModule))]
public partial class IdentityCoreDomainModule : CrmModule
{
}