using System.Security.Cryptography;
using System.Text;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.BackgroundTasks;

/// <summary>
/// Base class for periodic background tasks that synchronize remote data into a local entity store.
/// Implements two-level change detection: in-memory batch hash (fast path) + per-entity ContentHash (DB).
/// </summary>
/// <typeparam name="TService">Remote service interface to fetch data from.</typeparam>
/// <typeparam name="TViewModel">DTO returned by the remote service.</typeparam>
/// <typeparam name="TEntity">Local domain entity to sync into.</typeparam>
/// <typeparam name="TId">Entity primary key type.</typeparam>
public abstract class SyncBackgroundTask<TService, TViewModel, TEntity, TId>(
    IServiceScopeFactory scopeFactory,
    ILogger logger)
    : PeriodicBackgroundTask
    where TService : class
    where TEntity : Entity<TId>
    where TId : notnull
{
    private volatile string? _lastBatchHash;

    /// <summary>Fetches all items from the remote service.</summary>
    protected abstract ValueTask<IReadOnlyList<TViewModel>> FetchAsync(
        TService service, CancellationToken ct);

    /// <summary>Extracts the identifier from a remote item.</summary>
    protected abstract TId GetId(TViewModel vm);

    /// <summary>Computes the content hash of a remote item. Use the generated <c>vm.ComputeHash()</c>.</summary>
    protected abstract string ComputeItemHash(TViewModel vm);

    /// <summary>Returns the stored content hash of a local entity.</summary>
    protected abstract string GetEntityContentHash(TEntity entity);

    /// <summary>Creates a new local entity from a remote item.</summary>
    protected abstract TEntity Create(TViewModel vm);

    /// <summary>Updates an existing local entity from a remote item.</summary>
    protected abstract void Update(TEntity entity, TViewModel vm);

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var taskName = GetType().Name;

        using var scope = scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();

        var items = await FetchAsync(service, cancellationToken);

        if (items.Count == 0)
        {
            logger.LogDebug("{Task}: no items returned from remote, skipping sync", taskName);
            return;
        }

        var batchHash = BuildBatchHash(items);
        if (batchHash == _lastBatchHash)
        {
            logger.LogDebug("{Task}: no changes detected, skipping sync", taskName);
            return;
        }

        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TEntity, TId>>();

        var existingById = (await repository.GetAllAsync(cancellationToken: cancellationToken))
            .ToDictionary(x => x.Id);

        var added = 0;
        var updated = 0;

        foreach (var item in items)
        {
            var id = GetId(item);
            var hash = ComputeItemHash(item);

            if (existingById.TryGetValue(id, out var entity))
            {
                if (GetEntityContentHash(entity) == hash)
                    continue;

                Update(entity, item);
                repository.Update(entity);
                updated++;
            }
            else
            {
                repository.Add(Create(item));
                added++;
            }
        }

        if (added > 0 || updated > 0)
            await repository.SaveChangesAsync(cancellationToken);

        _lastBatchHash = batchHash;

        if (added > 0 || updated > 0)
            logger.LogInformation(
                "{Task}: synced — added: {Added}, updated: {Updated}",
                taskName, added, updated);
    }

    private string BuildBatchHash(IReadOnlyList<TViewModel> items)
    {
        var content = string.Join(";",
            items.Select(x => $"{GetId(x)}:{ComputeItemHash(x)}")
                 .OrderBy(x => x));
        return Hash(content);
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
