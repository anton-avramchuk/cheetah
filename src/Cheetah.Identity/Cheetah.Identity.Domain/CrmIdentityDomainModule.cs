using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Identity.Events;

namespace Cheetah.Identity.Domain;

/// <summary>
/// Identity Domain Module
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDomainModule))]
[DependsOn(typeof(CrmIdentityEventsModule))]
[DependsOn(typeof(IdentityCoreDomainModule))]
[DependsOn(typeof(CrmSpecificationModule))]
public class CrmIdentityDomainModule : CrmModule
{
}
