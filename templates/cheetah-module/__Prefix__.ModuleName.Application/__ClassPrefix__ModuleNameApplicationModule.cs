using Cheetah.Core.Modularity;
using __Prefix__.ModuleName.Contracts;
using __Prefix__.ModuleName.DataAccess;
using __Prefix__.ModuleName.Domain;
using __Prefix__.ModuleName.DomainEvents;

namespace __Prefix__.ModuleName.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(__ClassPrefix__ModuleNameDomainModule),
    typeof(__ClassPrefix__ModuleNameDataAccessModule),
    typeof(__ClassPrefix__ModuleNameContractsModule),
    typeof(__ClassPrefix__ModuleNameDomainEventsModule)
)]
public partial class __ClassPrefix__ModuleNameApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
