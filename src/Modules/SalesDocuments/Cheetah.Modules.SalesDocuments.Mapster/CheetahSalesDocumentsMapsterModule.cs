using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.SalesDocuments.Application;
using Cheetah.Modules.SalesDocuments.Contracts;

namespace Cheetah.Modules.SalesDocuments.Mapster;

[DependsOn(typeof(CoreModule),
    typeof(CrmMapsterModule),
    typeof(CheetahSalesDocumentsApplicationModule),
    typeof(CheetahSalesDocumentsContractsModule))]
public partial class CheetahSalesDocumentsMapsterModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}