using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Workflow.Client;

/// <summary>
/// Клиентский модуль Workflow для сервиса-контрибутора/цели: регистрирует приёмник действий
/// (<see cref="ExecuteActionRequestedHandler"/>) и подписывает его на шину. HTTP-клиент каталога и
/// декларацию триггеров/действий подключают через <c>AddWorkflowClient(...).RegisterTriggers/RegisterActions</c>.
/// </summary>
[DependsOn(typeof(CoreModule), typeof(CrmEventsCoreModule), typeof(CrmWorkflowModule),
    typeof(CheetahWorkflowContractsModule))]
public partial class CrmWorkflowClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<ExecuteActionRequestedIntegrationEvent, ExecuteActionRequestedHandler>();
    }
}
