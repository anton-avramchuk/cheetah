using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.DataAccess;

[DependsOn(typeof(CrmDomainModule))]
public class CrmDataAccessModule : CrmModule
{
}