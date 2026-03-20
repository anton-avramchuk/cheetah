using AppName.Contracts;
using AppName.Domain;
using AppName.Events;
using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;

namespace AppName.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmGridModule),
    typeof(AppNameDomainModule),
    typeof(AppNameContractsModule),
    typeof(AppNameEventsModule)
)]
public partial class AppNameApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
