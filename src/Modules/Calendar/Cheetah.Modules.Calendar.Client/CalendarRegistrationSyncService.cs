using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Client;

/// <summary>
/// При старте сервиса отправляет объявленные привязываемые типы сущностей в Calendar.Api через
/// <see cref="ICalendarClient"/>. Идемпотентно (upsert на стороне Calendar).
/// </summary>
public sealed class CalendarRegistrationSyncService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CalendarableTypeRegistrationOptions _registration;
    private readonly CalendarClientOptions _options;
    private readonly ILogger<CalendarRegistrationSyncService> _logger;

    public CalendarRegistrationSyncService(
        IServiceProvider serviceProvider,
        IOptions<CalendarableTypeRegistrationOptions> registration,
        IOptions<CalendarClientOptions> options,
        ILogger<CalendarRegistrationSyncService> logger)
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
            _logger.LogDebug("No calendarable entity types to register; skip Calendar registry sync");
            return;
        }

        var items = _registration.Items
            .Select(i => i with { OwnerService = _options.OwnerService })
            .ToArray();

        using var scope = _serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<ICalendarClient>();

        try
        {
            await client.SyncRegistryAsync(new CalendarRegistrySyncRequest(_options.OwnerService, items), cancellationToken);
            _logger.LogInformation("Registered {Count} calendarable entity types in Calendar for {Owner}",
                items.Length, _options.OwnerService);
        }
        catch (Exception ex) when (_options.ContinueOnFailure)
        {
            _logger.LogError(ex, "Failed to register calendarable entity types in Calendar for {Owner}", _options.OwnerService);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
