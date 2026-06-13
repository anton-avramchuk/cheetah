using System.Reflection;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.Domain;
using Cheetah.Core.Mongo.Mapping;
using Cheetah.Core.Mongo.Specifications;
using Cheetah.Core.Mongo.UnitOfWork;
using Cheetah.Core.Specification;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Cheetah.Core.Mongo.Repositories;

/// <summary>
/// MongoDB-backed <see cref="IRepository{TEntity,TKey}"/>. Reads execute immediately against the
/// collection; writes are buffered on the scoped <see cref="IMongoUnitOfWork"/> and flushed
/// atomically (per connection) by <see cref="SaveChangesAsync"/>. All filtering flows through
/// specifications translated by <see cref="IMongoFilterBuilder"/>, honouring the project's
/// specification rule.
/// </summary>
public class MongoRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    private readonly IMongoCollectionResolver _collectionResolver;
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly IMongoFilterBuilder _filterBuilder;
    private readonly MongoEntityMapping _mapping;
    private readonly string? _connectionName;

    /// <summary>Initializes a new instance of the <see cref="MongoRepository{TEntity,TKey}"/> class.</summary>
    public MongoRepository(
        IMongoCollectionResolver collectionResolver,
        IMongoUnitOfWork unitOfWork,
        IMongoFilterBuilder filterBuilder,
        IMongoEntityMapRegistry mapRegistry)
    {
        _collectionResolver = collectionResolver;
        _unitOfWork = unitOfWork;
        _filterBuilder = filterBuilder;
        _mapping = mapRegistry.GetMapping<TEntity>();
        _connectionName = typeof(TEntity).GetCustomAttribute<ConnectionStringNameAttribute>()?.Name;
    }

    private FieldDefinition<TEntity, TKey> KeyField => new StringFieldDefinition<TEntity, TKey>(_mapping.KeyMemberName);

    /// <inheritdoc />
    public async ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var collection = await _collectionResolver.GetEntityCollectionAsync<TEntity>(cancellationToken);
        var filter = Builders<TEntity>.Filter.Eq(KeyField, id);
        return await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<TEntity?> GetBySpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
    {
        var collection = await _collectionResolver.GetEntityCollectionAsync<TEntity>(cancellationToken);
        return await collection.Find(_filterBuilder.Build(spec)).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<List<TEntity>> GetAllAsync(ISpecification<TEntity>? spec = null, CancellationToken cancellationToken = default)
    {
        var collection = await _collectionResolver.GetEntityCollectionAsync<TEntity>(cancellationToken);
        return await collection.Find(_filterBuilder.Build(spec)).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<bool> ExistsAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
    {
        var collection = await _collectionResolver.GetEntityCollectionAsync<TEntity>(cancellationToken);
        return await collection.Find(_filterBuilder.Build(spec)).Limit(1).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Add(TEntity entity) => _unitOfWork.Enqueue(new PendingWrite
    {
        ConnectionName = _connectionName,
        ApplyAsync = async (db, session, ct) =>
        {
            await Collection(db).InsertOneAsync(session, entity, cancellationToken: ct);
            return 1;
        },
    });

    /// <inheritdoc />
    public void Update(TEntity entity) => _unitOfWork.Enqueue(new PendingWrite
    {
        ConnectionName = _connectionName,
        ApplyAsync = async (db, session, ct) =>
        {
            var filter = Builders<TEntity>.Filter.Eq(KeyField, entity.Id);
            var result = await Collection(db).ReplaceOneAsync(session, filter, entity, cancellationToken: ct);
            return result.IsModifiedCountAvailable ? result.ModifiedCount : result.MatchedCount;
        },
    });

    /// <inheritdoc />
    public void Delete(TEntity entity) => _unitOfWork.Enqueue(new PendingWrite
    {
        ConnectionName = _connectionName,
        ApplyAsync = async (db, session, ct) =>
        {
            var filter = Builders<TEntity>.Filter.Eq(KeyField, entity.Id);
            var result = await Collection(db).DeleteOneAsync(session, filter, cancellationToken: ct);
            return result.DeletedCount;
        },
    });

    /// <inheritdoc />
    public ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _unitOfWork.SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    public IQueryable<TEntity> AsQueryable()
    {
        // The only async step (connection-string resolution) completes synchronously for the
        // default resolver; the driver's LINQ provider is genuinely supported here, unlike Dapper.
        var collection = _collectionResolver.GetEntityCollectionAsync<TEntity>()
            .AsTask().GetAwaiter().GetResult();
        return collection.AsQueryable();
    }

    /// <inheritdoc />
    public IQueryable<TEntity> AsNoTrackingQueryable() => AsQueryable();

    private IMongoCollection<TEntity> Collection(IMongoDatabase database)
        => database.GetCollection<TEntity>(_mapping.CollectionName);
}

/// <summary>
/// Convenience <see cref="Guid"/>-keyed MongoDB repository.
/// </summary>
public class MongoRepository<TEntity> : MongoRepository<TEntity, Guid>, IRepository<TEntity>
    where TEntity : Entity<Guid>
{
    /// <summary>Initializes a new instance of the <see cref="MongoRepository{TEntity}"/> class.</summary>
    public MongoRepository(
        IMongoCollectionResolver collectionResolver,
        IMongoUnitOfWork unitOfWork,
        IMongoFilterBuilder filterBuilder,
        IMongoEntityMapRegistry mapRegistry)
        : base(collectionResolver, unitOfWork, filterBuilder, mapRegistry)
    {
    }
}
