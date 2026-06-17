using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahSalesDocumentsSharedModule))]
public class CheetahSalesDocumentsContractsModule : CrmModule
{
}
