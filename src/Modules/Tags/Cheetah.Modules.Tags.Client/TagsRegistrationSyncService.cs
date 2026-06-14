using Cheetah.Modules.Tags.Contracts.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Tags.Client;

/// <summary>
/// При старте сервиса отправляет объявленные применимые типы сущностей в Tags.Api
/// через <see cref="ITagsClient"/>. Идемпотентно (upsert на стороне Tags).
/// </summary>
public sealed class TagsRegistrationSyncService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TaggableTypeRegistrationOptions _registration;
    private readonly TagsClientOptions _options;
    private readonly ILogger<TagsRegistrationSyncService> _logger;

    public TagsRegistrationSyncService(
        IServiceProvider serviceProvider,
        IOptions<TaggableTypeRegistrationOptions> registration,
        IOptions<TagsClientOptions> options,
        ILogger<TagsRegistrationSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _registration = registration.Value;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_registration.Items.Count == 0)
        {
            _logger.LogDebug("No taggable entity types to register; skip Tags registry sync");
            return;
        }

        // OwnerService — единый для сервиса, проставляется здесь (в Add его не знали).
        var items = _registration.Items
            .Select(i => i with { OwnerService = _options.OwnerService })
            .ToArray();

        using var scope = _serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<ITagsClient>();

        try
        {
            await client.SyncRegistryAsync(new RegistrySyncRequest(_options.OwnerService, items), cancellationToken);
            _logger.LogInformation("Registered {Count} taggable entity types in Tags for {Owner}",
                items.Length, _options.OwnerService);
        }
        catch (Exception ex) when (_options.ContinueOnFailure)
        {
            _logger.LogError(ex, "Failed to register taggable entity types in Tags for {Owner}", _options.OwnerService);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
