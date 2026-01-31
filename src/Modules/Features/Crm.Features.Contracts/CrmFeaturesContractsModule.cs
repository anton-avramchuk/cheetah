using Cheetah.Contracts;
using Cheetah.Core.Modularity;

namespace Crm.Features.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule),typeof(CrmContractsModule))]

public class CrmFeaturesContractsModule : CrmModule
{
}