using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Catalog.Application;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain;

namespace Cheetah.Modules.Catalog.Mapster;

[DependsOn(typeof(CoreModule), typeof(CrmMapsterModule), typeof(CheetahCatalogContractsModule),
    typeof(CheetahCatalogApplicationModule), typeof(CheetahCatalogDomainModule))]
public partial class CheetahCatalogMapsterModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}