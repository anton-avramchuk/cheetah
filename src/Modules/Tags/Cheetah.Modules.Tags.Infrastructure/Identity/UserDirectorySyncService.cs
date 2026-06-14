using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Tags.Domain.Abstractions;
using Cheetah.Modules.Tags.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.Tags.Infrastructure.Identity;

/// <summary>
/// При старте модуля наполняет локальную реплику пользователей: тянет полный список из Identity
/// (через <see cref="IIdentityUserDirectory"/>) и идемпотентно апсёртит в БД Tags. Дальше
/// актуальность поддерживается доменными событиями Identity.
///
/// Ошибка синка логируется, но не валит хост — реплика наполнится из событий.
/// </summary>
public sealed class UserDirectorySyncService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserDirectorySyncService> _logger;

    public UserDirectorySyncService(IServiceProvider serviceProvider, ILogger<UserDirectorySyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var directory = scope.ServiceProvider.GetRequiredService<IIdentityUserDirectory>();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<User, Guid>>();

            var entries = await directory.GetAllAsync(cancellationToken);
            var existing = (await repository.GetAllAsync(null, cancellationToken)).ToDictionary(u => u.Id);

            var changed = 0;
            foreach (var entry in entries)
            {
                if (existing.TryGetValue(entry.Id, out var user))
                {
                    if (user.Apply(entry)) // детект изменений по хэшу
                    {
                        repository.Update(user);
                        changed++;
                    }
                }
                else
                {
                    repository.Add(User.Create(entry));
                    changed++;
                }
            }

            if (changed > 0)
                await repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User directory synced from Identity: {Total} fetched, {Changed} upserted",
                entries.Count, changed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initial user directory sync from Identity failed; replica will rely on events");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
