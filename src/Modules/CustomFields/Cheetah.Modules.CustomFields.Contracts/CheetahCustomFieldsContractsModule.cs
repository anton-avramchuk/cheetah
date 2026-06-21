using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.CustomFields.Shared;
using Cheetah.Validation;

namespace Cheetah.Modules.CustomFields.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule),
    typeof(CrmValidationModule), typeof(CheetahCustomFieldsSharedModule))]
public class CheetahCustomFieldsContractsModule : CrmModule
{
}
