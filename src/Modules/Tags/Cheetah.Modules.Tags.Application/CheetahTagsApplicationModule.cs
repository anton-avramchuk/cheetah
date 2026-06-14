using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Tags.Contracts;
using Cheetah.Modules.Tags.Domain;
using Cheetah.Modules.Tags.DomainEvents;

namespace Cheetah.Modules.Tags.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CheetahTagsDomainModule),
    typeof(CheetahTagsContractsModule),
    typeof(CheetahTagsDomainEventsModule))]
public partial class CheetahTagsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
