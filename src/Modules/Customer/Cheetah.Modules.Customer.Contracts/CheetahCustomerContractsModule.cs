using Cheetah.Core.Modularity;
using Cheetah.Modules.Customer.Shared;

namespace Cheetah.Modules.Customer.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CheetahCustomerSharedModule))]
public class CheetahCustomerContractsModule : CrmModule
{
}
