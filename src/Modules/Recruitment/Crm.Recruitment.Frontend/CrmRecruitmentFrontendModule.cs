using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;
using Crm.Recruitment.ApiClient;
using Crm.Recruitment.Contracts;

namespace Crm.Recruitment.Frontend;

[DependsOn(typeof(Cheetah.Blazor.Components.CrmBlazorComponentsModule))]
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmRecruitmentApiClientModule))]
[DependsOn(typeof(CrmRecruitmentContractsModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
public partial class CrmRecruitmentFrontendModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
