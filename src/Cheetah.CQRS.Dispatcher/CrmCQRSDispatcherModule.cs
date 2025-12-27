using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;

namespace Cheetah.CQRS.Dispatcher;
[DependsOn(
    typeof(CrmCQRSCoreModule)
    )]
public class CrmCQRSDispatcherModule : CrmModule
{
}