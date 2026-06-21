using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.CustomFields.DomainEvents;
using Cheetah.Modules.CustomFields.Shared;

namespace Cheetah.Modules.CustomFields.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule),
    typeof(CrmDataAccessModule))]
[DependsOn(typeof(CheetahCustomFieldsSharedModule), typeof(CheetahCustomFieldsDomainEventsModule))]
public partial class CheetahCustomFieldsDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
