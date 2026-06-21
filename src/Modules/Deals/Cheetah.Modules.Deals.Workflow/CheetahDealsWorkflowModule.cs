using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Deals.Application;
using Cheetah.Workflow;

namespace Cheetah.Modules.Deals.Workflow;

/// <summary>
/// Адаптер Deals → Workflow: регистрирует in-proc действия модуля Deals (<c>ChangeDealStage</c>,
/// <c>WinDeal</c>, <c>AssignDealOwner</c>) как <c>IWorkflowAction</c>. Зависит ТОЛЬКО на абстракцию
/// <see cref="CrmWorkflowModule"/> и собственный прикладной слой Deals — не на бизнес-модуль Workflow.
/// Образец для остальных модулей-контрибуторов.
/// <para>
/// Действия подхватываются <c>InProcActionExecutor</c> через DI автоматически. Декларацию триггеров/действий
/// в каталог Workflow (для админки) хост подключает через <see cref="DealsWorkflowDescriptors"/> — см. README.
/// </para>
/// </summary>
[DependsOn(typeof(CoreModule), typeof(CrmCQRSCoreModule), typeof(CrmWorkflowModule), typeof(CheetahDealsApplicationModule))]
public partial class CheetahDealsWorkflowModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services); // [Export]: ChangeDealStageAction, WinDealAction, AssignDealOwnerAction
    }
}
