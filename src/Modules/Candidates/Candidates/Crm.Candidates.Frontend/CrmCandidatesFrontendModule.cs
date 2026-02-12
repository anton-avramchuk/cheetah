using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;
using Crm.Candidates.ApiClient;
using Crm.Candidates.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Crm.Candidates.Frontend;

[DependsOn(typeof(Cheetah.Blazor.Components.CrmBlazorComponentsModule))]
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmCandidatesApiClientModule))]
[DependsOn(typeof(CrmCandidatesContractsModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
public partial class CrmCandidatesFrontendModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddOptions<CrmCandidatesApiClientOptions>()
            .Configure<IOptions<CrmCandidatesFrontendOptions>>((apiOpts, frontendOpts) =>
            {
                if (!string.IsNullOrEmpty(frontendOpts.Value.ApiUrl))
                    apiOpts.BaseUrl = frontendOpts.Value.ApiUrl.TrimEnd('/') + "/api";
            });
    }
}