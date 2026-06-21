namespace Cheetah.Modules.Workflow.Shared;

/// <summary>Общие константы модуля Workflow.</summary>
public static class WorkflowConstants
{
    public const string ConnectionStringName = "Workflow";
    public const string Schema = "workflow";
}

/// <summary>Имена встроенных действий (совпадают с IWorkflowAction.Name и ActionDescriptor.Name).</summary>
public static class BuiltInActions
{
    public const string CreateActivity = "CreateActivity";
    public const string ChangeDealStage = "ChangeDealStage";
    public const string SendNotification = "SendNotification";
    public const string CallWebhook = "CallWebhook";
    public const string AssignOwner = "AssignOwner";
}

/// <summary>Конвенции ключей Workflow.</summary>
public static class WorkflowKeys
{
    /// <summary>Синтетическое имя события cron-триггера правила.</summary>
    public static string ScheduleEventName(Guid ruleId) => $"schedule:{ruleId}";
}
