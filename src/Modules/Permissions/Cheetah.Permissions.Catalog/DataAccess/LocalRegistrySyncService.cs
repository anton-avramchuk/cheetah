using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Permissions.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.Permissions.Catalog.DataAccess;

/// <summary>
/// При старте: сканирует загруженные сборки на [Permission]-атрибуты (через PermissionRegistry),
/// синхронизирует их с каталогом в БД. Запускается только в монолитном профиле — там, где
/// Catalog подключён вместе со всеми модулями, объявляющими permissions.
///
/// В микросервисах каждый сервис при старте отправляет свои permissions через HTTP-клиент
/// (Cheetah.Permissions.Catalog.Client), а не через этот hosted-сервис.
/// </summary>
public sealed class LocalRegistrySyncService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PermissionRegistry _registry;
    private readonly ILogger<LocalRegistrySyncService> _logger;

    public LocalRegistrySyncService(
        IServiceProvider serviceProvider,
        PermissionRegistry registry,
        ILogger<LocalRegistrySyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _registry = registry;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _registry.ScanAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PermissionsDbContext>();

        try
        {
            await db.Database.CanConnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Permissions catalog DB unavailable; sync skipped");
            return;
        }

        var repository = scope.ServiceProvider.GetRequiredService<IRepository<PermissionDefinition, string>>();

        var keys = _registry.All.Select(d => d.Key).ToArray();
        var existing = (await repository.GetAllAsync(new PermissionsByKeysSpecification(keys), cancellationToken))
            .ToDictionary(p => p.Id);

        foreach (var d in _registry.All)
        {
            if (existing.TryGetValue(d.Key, out var pd)) pd.Update(d.Description, d.Module);
            else repository.Add(PermissionDefinition.Create(d.Key, d.Description, d.Module));
        }

        await repository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Permissions catalog sync completed: {Count} entries", _registry.All.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
