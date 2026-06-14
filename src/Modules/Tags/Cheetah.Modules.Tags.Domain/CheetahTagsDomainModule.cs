using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.Tags.DomainEvents;
using Cheetah.Modules.Tags.Shared;

namespace Cheetah.Modules.Tags.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule))]
[DependsOn(typeof(CheetahTagsSharedModule), typeof(CheetahTagsDomainEventsModule))]
public partial class CheetahTagsDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
