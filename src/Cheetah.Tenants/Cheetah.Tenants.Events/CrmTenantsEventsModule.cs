using Cheetah.Core.Modularity;
using Cheetah.Core.Tenants;

namespace Cheetah.Tenants.Events;

/// <summary>
/// Tenants Events Module - Extends Core Tenants events
/// Other modules can depend on this to subscribe to tenant events
/// </summary>
[DependsOn(typeof(CrmTenantsCoreModule))]
public partial class CrmTenantsEventsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Events project has no services to register
        // It only contains event contracts (records)
    }
}
