using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;

namespace Crm.Customer.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule))]
public partial class CrmCustomerDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}