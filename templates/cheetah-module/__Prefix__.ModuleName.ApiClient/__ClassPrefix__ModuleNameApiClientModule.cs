using Cheetah.Contracts.Attributes;
using Cheetah.Core.Modularity;
using __Prefix__.ModuleName.Contracts;

namespace __Prefix__.ModuleName.ApiClient;

[GenerateApiClient("ModuleName")]
[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(__ClassPrefix__ModuleNameContractsModule))]
public partial class __ClassPrefix__ModuleNameApiClientModule : CrmModule
{
}
