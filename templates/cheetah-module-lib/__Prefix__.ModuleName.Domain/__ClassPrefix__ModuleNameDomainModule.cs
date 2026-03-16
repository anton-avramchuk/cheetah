using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;

namespace __Prefix__.ModuleName.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule))]
public partial class __ClassPrefix__ModuleNameDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
