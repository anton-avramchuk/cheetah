using Cheetah.Core.Modularity;
using Cheetah.Features.Shared;

namespace Cheetah.Features.Contracts;

[DependsOn(typeof(CrmFeaturesSharedModule))]
public class CrmFeaturesContractsModule : CrmModule
{
}