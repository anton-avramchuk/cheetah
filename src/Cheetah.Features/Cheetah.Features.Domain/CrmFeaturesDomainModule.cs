using Cheetah.Core;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Features.Events;

namespace Cheetah.Features.Domain;

/// <summary>
/// Features Domain Module - Contains domain entities and business logic
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDomainModule))]
[DependsOn(typeof(CrmFeaturesEventsModule))]
public partial class CrmFeaturesDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
