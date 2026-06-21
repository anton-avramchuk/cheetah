using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Expressions.JsonLogic;

namespace Cheetah.Workflow;

/// <summary>
/// Абстракция Workflow: контракты расширения (<see cref="IWorkflowAction"/>, <see cref="IWorkflowTrigger"/>),
/// конверт события (<see cref="WorkflowEventEnvelope"/>), порт исполнения действий
/// (<see cref="IWorkflowActionExecutor"/>) и дескрипторы реестра. Никакой БД и движка — движок
/// исполняется централизованно в бизнес-модуле <c>Cheetah.Modules.Workflow.*</c>.
/// <para>
/// Подключается в каждом модуле-контрибуторе, который поставляет действия/триггеры или зависит от
/// конверта. Пользовательские <see cref="IWorkflowAction"/>/<see cref="IWorkflowTrigger"/> регистрируются
/// через <c>[Export(..., typeof(IWorkflowAction))]</c>.
/// </para>
/// </summary>
[DependsOn(typeof(CoreModule), typeof(CrmEventsCoreModule), typeof(CrmExpressionsJsonLogicModule))]
public partial class CrmWorkflowModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
