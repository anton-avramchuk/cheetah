using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Client;

/// <summary>
/// HTTP-клиент к сервису Workflow (server-to-server). Модули-контрибуторы при старте регистрируют свои
/// триггеры и действия в каталоге Workflow (registry/sync), как Permissions.Catalog/Tags/FeatureManagement.
/// </summary>
public interface IWorkflowCatalogClient
{
    ValueTask SyncAsync(
        IReadOnlyList<TriggerDescriptor> triggers,
        IReadOnlyList<ActionDescriptor> actions,
        CancellationToken ct = default);
}
