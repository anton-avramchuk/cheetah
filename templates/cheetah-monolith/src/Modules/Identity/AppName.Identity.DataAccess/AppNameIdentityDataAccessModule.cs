using AppName.Identity.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.DataAccess;

namespace AppName.Identity.DataAccess;

[DependsOn(typeof(CrmIdentityCoreDataAccessModule), typeof(AppNameIdentityDomainModule))]
public partial class AppNameIdentityDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
