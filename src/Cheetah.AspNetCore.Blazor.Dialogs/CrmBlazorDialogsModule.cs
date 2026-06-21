using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.AspNetCore.Blazor.Dialogs;

[DependsOn(typeof(CoreModule))]
public partial class CrmBlazorDialogsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
