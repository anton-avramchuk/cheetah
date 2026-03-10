using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;

namespace Cheetah.Core.Domain;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CrmSpecificationModule))]
public class CrmDomainModule : CrmModule
{
}