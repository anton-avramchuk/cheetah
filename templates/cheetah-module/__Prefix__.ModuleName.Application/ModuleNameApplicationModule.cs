using Cheetah.Core.Modularity;
using __Prefix__.ModuleName.Contracts;
using __Prefix__.ModuleName.DataAccess;
using __Prefix__.ModuleName.Domain;
using __Prefix__.ModuleName.DomainEvents;

namespace __Prefix__.ModuleName.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(ModuleNameDomainModule),
    typeof(ModuleNameDataAccessModule),
    typeof(ModuleNameContractsModule),
    typeof(ModuleNameDomainEventsModule)
)]
public partial class ModuleNameApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
