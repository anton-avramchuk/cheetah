using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Saga;

/// <summary>
/// Core-модуль саг. Регистрирует SagaOrchestrator. Сами саги добавляются через
/// services.AddSaga&lt;MySaga&gt;() в модуле, который их владеет.
///
/// Storage (ISagaRepository) подключается отдельным модулем —
/// Cheetah.Saga.EntityFrameworkCore для EF реализации.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
public partial class CrmSagaModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
