using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;

namespace Crm.Features.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule))]
public partial class CrmFeaturesDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}