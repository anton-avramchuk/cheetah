using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Identity.Events;

/// <summary>
/// Identity Events Module - Pure contracts with NO dependencies
/// Other modules can depend on this to subscribe to identity events
/// </summary>
[DependsOn(typeof(CrmEventsCoreModule))]
public partial class CrmIdentityEventsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Events project has no services to register
        // It only contains event contracts (records)
    }
}
