using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Shared;

namespace Cheetah.Modules.FeatureManagement.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule),
    typeof(CrmFeatureManagementModule), typeof(CheetahFeatureManagementSharedModule))]
public class CheetahFeatureManagementContractsModule : CrmModule
{
}
