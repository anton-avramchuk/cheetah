using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Mongo.Mapping;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cheetah.Saga.Mongo;

/// <summary>
/// MongoDB <see cref="ISagaRepository"/> with optimistic concurrency. MongoDB has no change tracker,
/// so this scoped repository tracks instances returned by <see cref="FindAsync"/> and added via
/// <see cref="AddAsync"/>, and persists them in <see cref="SaveChangesAsync"/>: new instances are
/// inserted, existing ones are replaced with a <c>Version</c> guard. A replace that matches no
/// document means a concurrent update won, which raises <see cref="SagaConcurrencyException"/>.
/// </summary>
[Export(LifetimeType.Scoped, typeof(ISagaRepository))]
public class MongoSagaRepository : ISagaRepository
{
    private readonly IMongoCollectionResolver _collectionResolver;
    private readonly MongoSagaStoreOptions _options;
    private readonly Dictionary<Guid, Tracked> _tracked = new();

    /// <summary>Initializes a new instance of the <see cref="MongoSagaRepository"/> class.</summary>
    public MongoSagaRepository(
        IMongoCollectionResolver collectionResolver,
        IOptions<MongoSagaStoreOptions> options)
    {
        _collectionResolver = collectionResolver;
        _options = options.Value;
    }

    private ValueTask<IMongoCollection<SagaInstance>> CollectionAsync(CancellationToken ct)
        => _collectionResolver.GetCollectionAsync<SagaInstance>(_options.SagaCollection, _options.ConnectionName, ct);

    /// <inheritdoc />
    public async ValueTask<SagaInstance?> FindAsync(string sagaType, string correlationKey, CancellationToken cancellationToken = default)
    {
        var collection = await CollectionAsync(cancellationToken);
        var instance = await collection
            .Find(x => x.SagaType == sagaType && x.CorrelationKey == correlationKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (instance is not null)
            _tracked[instance.Id] = new Tracked(instance, instance.Version, IsNew: false);

        return instance;
    }

    /// <inheritdoc />
    public ValueTask AddAsync(SagaInstance instance, CancellationToken cancellationToken = default)
    {
        _tracked[instance.Id] = new Tracked(instance, instance.Version, IsNew: true);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_tracked.Count == 0)
            return;

        var collection = await CollectionAsync(cancellationToken);
        using var session = await collection.Database.Client.StartSessionAsync(cancellationToken: cancellationToken);

        session.StartTransaction();
        try
        {
            foreach (var tracked in _tracked.Values)
            {
                if (tracked.IsNew)
                {
                    await collection.InsertOneAsync(session, tracked.Instance, cancellationToken: cancellationToken);
                    continue;
                }

                tracked.Instance.Version += 1;
                tracked.Instance.UpdatedAt = DateTimeOffset.UtcNow;

                var filter = Builders<SagaInstance>.Filter.And(
                    Builders<SagaInstance>.Filter.Eq(x => x.Id, tracked.Instance.Id),
                    Builders<SagaInstance>.Filter.Eq(x => x.Version, tracked.OriginalVersion));

                var result = await collection.ReplaceOneAsync(session, filter, tracked.Instance, cancellationToken: cancellationToken);
                if (result.MatchedCount == 0)
                {
                    await session.AbortTransactionAsync(CancellationToken.None);
                    throw new SagaConcurrencyException(tracked.Instance.SagaType, tracked.Instance.CorrelationKey);
                }
            }

            await session.CommitTransactionAsync(cancellationToken);
        }
        catch (SagaConcurrencyException)
        {
            throw;
        }
        catch
        {
            await session.AbortTransactionAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            _tracked.Clear();
        }
    }

    private readonly record struct Tracked(SagaInstance Instance, int OriginalVersion, bool IsNew);
}
