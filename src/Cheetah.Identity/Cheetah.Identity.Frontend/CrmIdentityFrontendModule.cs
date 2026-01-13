using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Frontend.Client;

namespace Cheetah.Identity.Frontend;

[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmIdentityFrontendClientModule))]
public class CrmIdentityFrontendModule:CrmModule
{
    
}