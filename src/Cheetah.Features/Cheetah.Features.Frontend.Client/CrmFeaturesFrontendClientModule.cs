using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Features.Contracts;
using Cheetah.Features.Shared;
using Cheetah.Frontend.CQRS;

namespace Cheetah.Features.Frontend.Client;

/// <summary>
/// Features Frontend Client Module - Frontend client for Features module
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmFrontendCQRSModule))]
[DependsOn(typeof(CrmFeaturesSharedModule))]
[DependsOn(typeof(CrmFeaturesContractsModule))]
public partial class CrmFeaturesFrontendClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
