using Cheetah.AspNetCore.Blazor.Dialogs;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.AspNetCore.Blazor.Controls;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorDialogsModule))]
public class CrmBlazorControlsModule : CrmModule
{
}
