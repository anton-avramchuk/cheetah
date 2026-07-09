using Cheetah.Audit;
using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.DomainEvents;
using Cheetah.Modules.FeatureManagement.Shared;

namespace Cheetah.Modules.FeatureManagement.Domain;

// CrmAuditModule — только ради атрибутов [Auditable] на сущностях; interceptor/sink живут в Infrastructure.
[DependsOn(typeof(CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule),
    typeof(CrmDataAccessModule), typeof(CrmFeatureManagementModule), typeof(CrmAuditModule))]
[DependsOn(typeof(CheetahFeatureManagementSharedModule), typeof(CheetahFeatureManagementDomainEventsModule))]
public partial class CheetahFeatureManagementDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
