using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Features.Shared;

namespace Cheetah.Features.Contracts;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmFeaturesSharedModule))]
public class CrmFeaturesContractsModule : CrmModule
{
}