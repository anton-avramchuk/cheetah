using Cheetah.Core.Modularity;
using Crm.Features.Domain;

namespace Crm.Features.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule),typeof(CrmFeaturesDomainModule))]
public partial class CrmFeaturesDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}