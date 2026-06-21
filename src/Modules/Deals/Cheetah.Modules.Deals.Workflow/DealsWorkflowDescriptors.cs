using Cheetah.Workflow;

namespace Cheetah.Modules.Deals.Workflow;

/// <summary>
/// Дескрипторы триггеров и действий Deals для каталога Workflow (нужны админке, чтобы строить правила).
/// Хост передаёт их в каталог через <c>AddWorkflowClient(...).RegisterTriggers(...)/RegisterActions(...)</c>
/// — см. README адаптера. Сами действия исполняются in-proc (см. Actions/*), регистрируются автоматически
/// через <c>[Export(typeof(IWorkflowAction))]</c>.
/// </summary>
public static class DealsWorkflowDescriptors
{
    public const string Owner = "Deals";

    public static IReadOnlyList<TriggerDescriptor> Triggers { get; } =
    [
        new("DealCreatedIntegrationEvent", Owner, "Сделка создана",
            ["DealId", "CustomerId", "PipelineId", "StageId", "Amount", "Currency", "OwnerId"]),
        new("DealStageChangedIntegrationEvent", Owner, "Стадия сделки изменена",
            ["DealId", "FromStageId", "ToStageId", "ChangedBy"]),
        new("DealWonIntegrationEvent", Owner, "Сделка выиграна",
            ["DealId", "CustomerId", "Amount", "Currency"]),
        new("DealLostIntegrationEvent", Owner, "Сделка проиграна",
            ["DealId", "Reason"]),
        new("DealOwnerChangedIntegrationEvent", Owner, "Ответственный по сделке изменён",
            ["DealId", "OldOwnerId", "NewOwnerId"]),
    ];

    public static IReadOnlyList<ActionDescriptor> Actions { get; } =
    [
        new(DealsActions.ChangeStage, Owner, "Переместить сделку на стадию", Params(
            new("dealId", "guid", true), new("toStageId", "guid", true), new("changedBy", "guid", false)),
            ActionTransport.InProc),
        new(DealsActions.Win, Owner, "Пометить сделку выигранной", Params(
            new("dealId", "guid", true), new("changedBy", "guid", false)),
            ActionTransport.InProc),
        new(DealsActions.AssignOwner, Owner, "Сменить ответственного по сделке", Params(
            new("dealId", "guid", true), new("newOwnerId", "guid", true)),
            ActionTransport.InProc),
    ];

    private static IReadOnlyList<ActionParameterDescriptor> Params(params ActionParameterDescriptor[] p) => p;
}
