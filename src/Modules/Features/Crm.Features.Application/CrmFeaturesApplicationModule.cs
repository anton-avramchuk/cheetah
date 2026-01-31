using Cheetah.Core.Modularity;
using Crm.Features.Contracts;
using Crm.Features.DataAccess;
using Crm.Features.Domain;
using Crm.Features.DomainEvents;

namespace Crm.Features.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmFeaturesDomainModule),
    typeof(CrmFeaturesDataAccessModule),
    typeof(CrmFeaturesContractsModule),
    typeof(CrmFeaturesDomainEventsModule)
)]
public partial class CrmFeaturesApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}