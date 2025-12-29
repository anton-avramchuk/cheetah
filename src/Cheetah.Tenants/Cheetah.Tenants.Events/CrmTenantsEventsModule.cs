using Cheetah.Core.Modularity;

namespace Cheetah.Tenants.Events;

/// <summary>
/// Tenants Events Module - Pure contracts with NO dependencies
/// Other modules can depend on this to subscribe to tenant events
/// </summary>
public partial class CrmTenantsEventsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Events project has no services to register
        // It only contains event contracts (records)
    }
}
