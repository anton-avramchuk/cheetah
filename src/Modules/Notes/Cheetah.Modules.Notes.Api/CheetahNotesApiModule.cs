using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.Backend.Endpoints;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Notes.Application;
using Cheetah.Modules.Notes.Contracts;

namespace Cheetah.Modules.Notes.Api;

/// <summary>
/// Api-модуль заметок. Сам маршрутов не объявляет: все эндпоинты заметки — абстрактные шаблоны
/// (<see cref="Endpoints.CreateNoteEndpoint{TRequest,TCommand}"/> и др.), которые закрывает
/// наследник/хост; генератор <c>Cheetah.Generators.Endpoints</c> регистрирует их уже в модуле
/// наследника. Модуль нужен как носитель зависимостей Api-слоя — наследник объявляет его
/// в своём <c>[DependsOn]</c>. Готовый закрытый набор — <c>Cheetah.Modules.Notes.Default</c>.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmAspNetCoreContractsModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CheetahNotesApplicationModule),
    typeof(CheetahNotesContractsModule))]
public partial class CheetahNotesApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
