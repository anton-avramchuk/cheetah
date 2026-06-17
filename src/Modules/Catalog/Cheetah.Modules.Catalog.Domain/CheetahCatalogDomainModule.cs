using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.Catalog.DomainEvents;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Domain;

[DependsOn(typeof(CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule),
    typeof(CrmDataAccessModule))]
[DependsOn(typeof(CheetahCatalogSharedModule), typeof(CheetahCatalogDomainEventsModule))]
public partial class CheetahCatalogDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
