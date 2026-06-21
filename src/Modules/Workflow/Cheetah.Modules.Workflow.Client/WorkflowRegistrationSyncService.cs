using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.Workflow.Client;

/// <summary>
/// При старте: агрегирует все вклады (<see cref="WorkflowRegistrationContribution"/>) и регистрирует
/// триггеры/действия в каталоге Workflow (registry/sync). <c>ContinueOnFailure</c> — недоступность
/// Workflow НЕ валит хост (правила, ссылающиеся на эти триггеры/действия, просто бездействуют, пока
/// каталог не поднимется).
/// </summary>
public sealed class WorkflowRegistrationSyncService : IHostedService
{
    private readonly IEnumerable<WorkflowRegistrationContribution> _contributions;
    private readonly IWorkflowCatalogClient _client;
    private readonly ILogger<WorkflowRegistrationSyncService> _logger;

    public WorkflowRegistrationSyncService(
        IEnumerable<WorkflowRegistrationContribution> contributions,
        IWorkflowCatalogClient client,
        ILogger<WorkflowRegistrationSyncService> logger)
    {
        _contributions = contributions;
        _client = client;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var triggers = _contributions.SelectMany(c => c.Triggers).ToArray();
        var actions = _contributions.SelectMany(c => c.Actions).ToArray();
        if (triggers.Length == 0 && actions.Length == 0)
            return;

        try
        {
            await _client.SyncAsync(triggers, actions, cancellationToken);
            _logger.LogInformation("Registered {Triggers} triggers and {Actions} actions in Workflow catalog.",
                triggers.Length, actions.Length);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Workflow registry sync failed; triggers/actions stay unregistered until catalog is up.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
