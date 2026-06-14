using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.Email.DomainEvents;
using Cheetah.Modules.Email.Shared;

namespace Cheetah.Modules.Email.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule))]
[DependsOn(typeof(CheetahEmailSharedModule), typeof(CheetahEmailDomainEventsModule))]
public partial class CheetahEmailDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
