using Cheetah.Core.CQRS;
using Cheetah.Core.Modules;

namespace Cheetah.CQRS.Dispatcher;
[DependsOn(
    typeof(CrmCQRSCoreModule)
    )]
public class CrmCQRSDispatcherModule : CrmModule
{
}