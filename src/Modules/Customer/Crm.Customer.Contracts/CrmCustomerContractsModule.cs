using Cheetah.Contracts;
using Cheetah.Core.Modularity;

namespace Crm.Customer.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule))]
public partial class CrmCustomerContractsModule : CrmModule
{
}