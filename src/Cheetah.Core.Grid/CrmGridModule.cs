using Cheetah.Contracts;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;

namespace Cheetah.Core.Grid;

/// <summary>
/// Module providing grid query services for filtering, sorting, and pagination
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmContractsModule))]
[DependsOn(typeof(CrmMappingCoreModule))]
public partial class CrmGridModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
