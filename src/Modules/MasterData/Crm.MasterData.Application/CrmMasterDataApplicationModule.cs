using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Crm.MasterData.Contracts;
using Crm.MasterData.DataAccess;
using Crm.MasterData.Domain;
using Crm.MasterData.DomainEvents;

namespace Crm.MasterData.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule),
    typeof(CrmMasterDataDataAccessModule),
    typeof(CrmMasterDataDomainModule),
    typeof(CrmMasterDataContractsModule),
    typeof(CrmMasterDataDomainEventsModule)
)]
public partial class CrmMasterDataApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}