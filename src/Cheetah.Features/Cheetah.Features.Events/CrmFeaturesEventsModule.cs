using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Features.Events;

/// <summary>
/// Features Events Module - Pure event contracts
/// Other modules can depend on this to subscribe to feature events
/// </summary>
[DependsOn(typeof(CrmEventsCoreModule))]
public partial class CrmFeaturesEventsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Events project has no services to register
        // It only contains event contracts (records)
    }
}
