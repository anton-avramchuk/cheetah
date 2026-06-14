using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.Notification.DomainEvents;
using Cheetah.Modules.Notification.Shared;

namespace Cheetah.Modules.Notification.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule))]
[DependsOn(typeof(CheetahNotificationSharedModule), typeof(CheetahNotificationDomainEventsModule))]
public partial class CheetahNotificationDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
