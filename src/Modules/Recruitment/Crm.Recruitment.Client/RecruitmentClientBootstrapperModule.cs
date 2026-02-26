using Cheetah.Blazor;
using Cheetah.Blazor.Layout;
using Cheetah.Blazor.Layout.Extensions;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Auth;
using Cheetah.Frontend.Navigation;
using Cheetah.Mapping.Mapster;
using Crm.Candidates.ApiClient;
using Crm.Candidates.Frontend;
using Crm.Identity.ApiClient;
using Crm.Identity.Frontend;
using Crm.Recruitment.ApiClient;
using Crm.Recruitment.Frontend;
using Crm.VacancyTasks.ApiClient;
using Crm.VacancyTasks.Frontend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Crm.Recruitment.Client;

[Bootstrapper]
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorModule))]
[DependsOn(typeof(CrmBlazorLayoutModule))]
[DependsOn(typeof(CrmFrontendAuthModule))]
[DependsOn(typeof(CrmIdentityFrontendModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
[DependsOn(typeof(CrmMapsterModule))]
[DependsOn(typeof(CrmRecruitmentFrontendModule))]
[DependsOn(typeof(CrmCandidatesFrontendModule))]
[DependsOn(typeof(CrmVacancyTasksFrontendModule))]
public partial class RecruitmentClientBootstrapperModule : CrmModule
{
    private const string ApiSection = "Api";
    private const string RecruitmentPath = "/api/recruitment/";
    private const string IdentityPath = "/api/identity/";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.ConfigureHttpClientDefaults(b =>
            b.AddHttpMessageHandler<JwtAuthorizationMessageHandler>()
             .AddHttpMessageHandler<UnauthorizedRedirectHandler>());

        context.Services.AddOptions<CrmRecruitmentFrontendOptions>().BindConfiguration(ApiSection);


        AddRecruitmentOptions(context.Services);


        context.Services.ConfigureCrmLayout(config =>
        {
            config.ApplicationName = "Recruitment";
            config.HomeUrl = "/";
            config.SidebarCollapsedByDefault = false;
        });
    }

    private static void AddRecruitmentOptions(IServiceCollection services)
    {
        services.AddOptions<CrmRecruitmentApiClientOptions>()
            .Configure<IOptions<CrmRecruitmentFrontendOptions>>((apiOpts, frontendOpts) =>
            {
                if (!string.IsNullOrEmpty(frontendOpts.Value.ApiUrl))
                    apiOpts.BaseUrl = frontendOpts.Value.ApiUrl.TrimEnd('/') + RecruitmentPath;
            });

        services.AddOptions<CrmCandidatesApiClientOptions>()
            .Configure<IOptions<CrmRecruitmentFrontendOptions>>((apiOpts, frontendOpts) =>
            {
                if (!string.IsNullOrEmpty(frontendOpts.Value.ApiUrl))
                    apiOpts.BaseUrl = frontendOpts.Value.ApiUrl.TrimEnd('/') + RecruitmentPath;
            });

        services.AddOptions<CrmVacancyTasksApiClientOptions>()
            .Configure<IOptions<CrmRecruitmentFrontendOptions>>((apiOpts, frontendOpts) =>
            {
                if (!string.IsNullOrEmpty(frontendOpts.Value.ApiUrl))
                    apiOpts.BaseUrl = frontendOpts.Value.ApiUrl.TrimEnd('/') + RecruitmentPath;
            });

        services.AddOptions<CrmIdentityApiClientOptions>()
            .Configure<IOptions<CrmRecruitmentFrontendOptions>>((apiOpts, frontendOpts) =>
            {
                if (!string.IsNullOrEmpty(frontendOpts.Value.ApiUrl))
                    apiOpts.BaseUrl = frontendOpts.Value.ApiUrl.TrimEnd('/') + IdentityPath;
            });
    }
}