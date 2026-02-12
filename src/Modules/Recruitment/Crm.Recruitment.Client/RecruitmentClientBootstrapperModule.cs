using Cheetah.Blazor;
using Cheetah.Blazor.Layout;
using Cheetah.Blazor.Layout.Extensions;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;
using Cheetah.Mapping.Mapster;
using Crm.Candidates.Frontend;
using Crm.Recruitment.Frontend;
using Crm.VacancyTasks.Frontend;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Recruitment.Client;

[Bootstrapper]
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorModule))]
[DependsOn(typeof(CrmBlazorLayoutModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
[DependsOn(typeof(CrmMapsterModule))]
[DependsOn(typeof(CrmRecruitmentFrontendModule))]
[DependsOn(typeof(CrmCandidatesFrontendModule))]
[DependsOn(typeof(CrmVacancyTasksFrontendModule))]
public partial class RecruitmentClientBootstrapperModule : CrmModule
{
    private const string ApiSection = "Api";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddOptions<CrmRecruitmentFrontendOptions>().BindConfiguration(ApiSection);
        context.Services.AddOptions<CrmCandidatesFrontendOptions>().BindConfiguration(ApiSection);
        context.Services.AddOptions<CrmVacancyTasksFrontendOptions>().BindConfiguration(ApiSection);

        context.Services.ConfigureCrmLayout(config =>
        {
            config.ApplicationName = "Recruitment";
            config.HomeUrl = "/";
            config.SidebarCollapsedByDefault = false;
        });
    }
}
