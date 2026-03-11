using Cheetah.Contracts.Attributes;
using Cheetah.Core.Modularity;
using Crm.Customer.Contracts;

namespace Crm.Customer.ApiClient;

[GenerateApiClient("Customer")]
[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmCustomerContractsModule))]
public partial class CrmCustomerApiClientModule : CrmModule
{
}