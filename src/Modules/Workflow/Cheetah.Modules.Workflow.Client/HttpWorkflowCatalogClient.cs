using System.Net.Http.Json;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Client;

/// <summary>HTTP-реализация клиента каталога Workflow.</summary>
public sealed class HttpWorkflowCatalogClient : IWorkflowCatalogClient
{
    private readonly HttpClient _http;

    public HttpWorkflowCatalogClient(HttpClient http) => _http = http;

    public async ValueTask SyncAsync(
        IReadOnlyList<TriggerDescriptor> triggers,
        IReadOnlyList<ActionDescriptor> actions,
        CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(
            "/api/automation/registry/sync",
            new WorkflowRegistrySyncBody(triggers, actions),
            ct);
        response.EnsureSuccessStatusCode();
    }
}

/// <summary>Тело запроса registry/sync.</summary>
public sealed record WorkflowRegistrySyncBody(
    IReadOnlyList<TriggerDescriptor> Triggers,
    IReadOnlyList<ActionDescriptor> Actions);
