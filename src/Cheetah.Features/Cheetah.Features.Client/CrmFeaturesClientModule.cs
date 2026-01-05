using Cheetah.Core.Modularity;
using Cheetah.Features.Application;
using Cheetah.Features.Shared;

namespace Cheetah.Features.Client;

/// <summary>
/// Features Client Module - Backend client for Features module
/// </summary>
[DependsOn(typeof(CrmFeaturesApplicationModule))]
[DependsOn(typeof(CrmFeaturesSharedModule))]
public partial class CrmFeaturesClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
