using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.Domain;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
public class CrmDomainModule : CrmModule
{
}