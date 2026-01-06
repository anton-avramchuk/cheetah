using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Events;

namespace Cheetah.Identity.Domain;

/// <summary>
/// Identity Domain Module
/// </summary>
[DependsOn(typeof(CrmDomainModule))]
[DependsOn(typeof(CrmIdentityEventsModule))]
[DependsOn(typeof(IdentityCoreDomainModule))]
public class CrmIdentityDomainModule : CrmModule
{
}
