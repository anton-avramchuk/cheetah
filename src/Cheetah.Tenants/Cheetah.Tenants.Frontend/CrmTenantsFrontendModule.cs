using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.CQRS;
using Cheetah.Frontend.Events;

namespace Cheetah.Tenants.Frontend;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmFrontendCQRSModule))]
[DependsOn(typeof(CrmFrontendEventsModule))]
public partial class CrmTenantsFrontendModule : CrmModule
{
}
