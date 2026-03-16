using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.DataAccess;

namespace Crm.Identity.DataAccess;

[DependsOn(typeof(CheetahIdentityDataAccessModule))]
public partial class CrmIdentityDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
