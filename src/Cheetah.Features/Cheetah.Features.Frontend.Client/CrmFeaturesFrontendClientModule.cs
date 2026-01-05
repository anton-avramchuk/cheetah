using Cheetah.Core.Modularity;
using Cheetah.Features.Shared;
using Cheetah.Frontend.CQRS;

namespace Cheetah.Features.Frontend.Client;

/// <summary>
/// Features Frontend Client Module - Frontend client for Features module
/// </summary>
[DependsOn(typeof(CrmFrontendCQRSModule))]
public partial class CrmFeaturesFrontendClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
