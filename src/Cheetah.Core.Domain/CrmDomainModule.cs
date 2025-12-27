using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.Domain;

[DependsOn(typeof(CrmEventsCoreModule))]
public class CrmDomainModule : CrmModule
{
}