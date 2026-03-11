using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;

namespace Crm.MasterData.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule))]
public partial class CrmMasterDataDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}