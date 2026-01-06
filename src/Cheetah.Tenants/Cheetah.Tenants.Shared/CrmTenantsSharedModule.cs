using Cheetah.Core.Modularity;

namespace Cheetah.Tenants.Shared;

/// <summary>
/// Tenants Shared module - DTOs, ViewModels, Requests, Responses
/// </summary>
public partial class CrmTenantsSharedModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
