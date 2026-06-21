using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.CustomFields.Client;

/// <summary>
/// При старте регистрирует расширяемые типы сервиса в каталоге (registry/sync). <c>ContinueOnFailure</c>:
/// недоступность каталога НЕ валит хост (типы догоняются при следующем старте / ретрае).
/// </summary>
public sealed class CustomFieldsRegistrationSyncService : IHostedService
{
    private readonly IEnumerable<CustomFieldsRegistrationContribution> _contributions;
    private readonly ICustomFieldsClient _client;
    private readonly ILogger<CustomFieldsRegistrationSyncService> _logger;

    public CustomFieldsRegistrationSyncService(
        IEnumerable<CustomFieldsRegistrationContribution> contributions,
        ICustomFieldsClient client,
        ILogger<CustomFieldsRegistrationSyncService> logger)
    {
        _contributions = contributions;
        _client = client;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var descriptors = _contributions.SelectMany(c => c.Descriptors).ToArray();
        if (descriptors.Length == 0) return;

        try
        {
            await _client.SyncTypesAsync(descriptors, cancellationToken);
            _logger.LogInformation("Registered {Count} custom-field entity types in catalog.", descriptors.Length);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Custom-fields registry sync failed; types will retry on next start.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
