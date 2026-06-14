using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Notification.Shared;

namespace Cheetah.Modules.Notification.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahNotificationSharedModule))]
public class CheetahNotificationContractsModule : CrmModule
{
}
