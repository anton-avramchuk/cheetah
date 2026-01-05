using Cheetah.Core.Modularity;
using Cheetah.Features.Application;

namespace Cheetah.Features.Client;

/// <summary>
/// Features Client Module - Backend client for Features module
/// </summary>
[DependsOn(typeof(CrmFeaturesApplicationModule))]
public partial class CrmFeaturesClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
