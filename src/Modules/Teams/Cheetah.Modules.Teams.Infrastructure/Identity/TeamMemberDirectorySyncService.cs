using Cheetah.Modules.Teams.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Teams.Infrastructure.Identity;

/// <summary>
/// Фоновый сервис: периодически синкает реплику участников из Identity, вызывая
/// <see cref="ITeamMemberDirectorySynchronizer"/>. Хэш-сравнение в оркестраторе гарантирует, что в БД
/// пишутся только изменившиеся записи. Ошибка одной итерации логируется и не валит хост — следующий
/// тик попробует снова.
/// </summary>
public sealed class TeamMemberDirectorySyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TeamMemberSyncOptions _options;
    private readonly ILogger<TeamMemberDirectorySyncService> _logger;

    public TeamMemberDirectorySyncService(
        IServiceScopeFactory scopeFactory,
        IOptions<TeamMemberSyncOptions> options,
        ILogger<TeamMemberDirectorySyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Team member directory sync is disabled");
            return;
        }

        if (_options.RunOnStartup)
            await SyncOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(_options.Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await SyncOnceAsync(stoppingToken);
    }

    private async Task SyncOnceAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var synchronizer = scope.ServiceProvider.GetRequiredService<ITeamMemberDirectorySynchronizer>();

            var changed = await synchronizer.SyncAsync(ct);

            if (changed > 0)
                _logger.LogInformation("Team member directory synced from Identity: {Changed} upserted", changed);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // штатная остановка хоста — не логируем как ошибку
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Team member directory sync from Identity failed; will retry next tick");
        }
    }
}
