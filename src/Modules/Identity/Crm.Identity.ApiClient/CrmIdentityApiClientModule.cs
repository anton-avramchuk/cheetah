using Cheetah.Contracts.Attributes;
using Cheetah.Core.Modularity;
using Crm.Identity.Contracts;

namespace Crm.Identity.ApiClient;

[GenerateApiClient("Identity")]
[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmIdentityContractsModule))]
public partial class CrmIdentityApiClientModule : CrmModule
{
}