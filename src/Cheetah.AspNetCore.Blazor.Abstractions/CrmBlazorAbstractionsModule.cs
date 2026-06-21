using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.AspNetCore.Blazor.Abstractions;

[DependsOn(typeof(CoreModule))]
public partial class CrmBlazorAbstractionsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
