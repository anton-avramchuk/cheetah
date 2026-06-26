using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Booking.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Booking: пункты бокового меню. Представление расписания/слотов —
/// бесповоротно нестандартный UI — поставляется приложением; сборка закрепляет присутствие модуля в
/// навигации. Contributor меню регистрируется генератором по <c>[Export]</c>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorNavigationModule))]
public partial class CheetahBookingBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
