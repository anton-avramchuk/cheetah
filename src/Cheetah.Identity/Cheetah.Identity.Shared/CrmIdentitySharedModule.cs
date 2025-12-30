using Cheetah.Core.Modularity;

namespace Cheetah.Identity.Shared;

/// <summary>
/// Identity Shared module - DTOs, ViewModels, Requests, Responses
/// </summary>
public partial class CrmIdentitySharedModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
