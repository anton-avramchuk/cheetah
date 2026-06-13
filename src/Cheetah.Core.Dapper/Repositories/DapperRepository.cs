using System.Reflection;
using Cheetah.Core.Dapper.Mapping;
using Cheetah.Core.Dapper.Specifications;
using Cheetah.Core.Dapper.Sql;
using Cheetah.Core.Dapper.UnitOfWork;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.Domain;
using Cheetah.Core.Specification;
using Dapper;

namespace Cheetah.Core.Dapper.Repositories;

/// <summary>
/// Dapper-backed <see cref="IRepository{TEntity,TKey}"/>. Reads execute immediately over the
/// scoped <see cref="IDapperUnitOfWork"/> connection; writes are buffered and flushed atomically
/// by <see cref="SaveChangesAsync"/>. All filtering flows through specifications translated by
/// <see cref="ExpressionToSqlParser"/>, honouring the project's specification rule.
/// </summary>
/// <remarks>
/// <see cref="AsQueryable"/>/<see cref="AsNoTrackingQueryable"/> are not supported — Dapper has no
/// <see cref="IQueryable{T}"/>. Use <see cref="Querying.IDapperQueryExecutor"/> for ad-hoc SQL.
/// </remarks>
public class DapperRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    private readonly IDapperUnitOfWork _unitOfWork;
    private readonly ISpecificationParser<SqlWhere> _specificationParser;
    private readonly EntityMapping _mapping;
    private readonly EntitySql _sql;
    private readonly string? _connectionName;

    /// <summary>Initializes a new instance of the <see cref="DapperRepository{TEntity,TKey}"/> class.</summary>
    public DapperRepository(
        IDapperUnitOfWork unitOfWork,
        IEntityMapRegistry mapRegistry,
        ISqlBuilder sqlBuilder,
        ISpecificationParser<SqlWhere> specificationParser)
    {
        _unitOfWork = unitOfWork;
        _specificationParser = specificationParser;
        _mapping = mapRegistry.GetMapping<TEntity>();
        _sql = sqlBuilder.GetSql(_mapping);
        _connectionName = ResolveConnectionName();
    }

    /// <inheritdoc />
    public async ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add(_sql.KeyParameterName, id);
        return await QueryFirstAsync(_sql.SelectById, parameters, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<TEntity?> GetBySpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
    {
        var where = _specificationParser.Parse(spec);
        return await QueryFirstAsync(Compose(_sql.SelectBase, where), ToParameters(where), cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<List<TEntity>> GetAllAsync(ISpecification<TEntity>? spec = null, CancellationToken cancellationToken = default)
    {
        var where = spec is null ? SqlWhere.Empty : _specificationParser.Parse(spec);
        var connection = await _unitOfWork.GetConnectionAsync(_connectionName, cancellationToken);
        var command = new CommandDefinition(Compose(_sql.SelectBase, where), ToParameters(where),
            _unitOfWork.GetTransaction(_connectionName), cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<TEntity>(command);
        return rows.ToList();
    }

    /// <inheritdoc />
    public async ValueTask<bool> ExistsAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
    {
        var where = _specificationParser.Parse(spec);
        var connection = await _unitOfWork.GetConnectionAsync(_connectionName, cancellationToken);
        var command = new CommandDefinition(Compose(_sql.CountBase, where), ToParameters(where),
            _unitOfWork.GetTransaction(_connectionName), cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<long>(command) > 0;
    }

    /// <inheritdoc />
    public void Add(TEntity entity) => _unitOfWork.Enqueue(new PendingOperation
    {
        ConnectionName = _connectionName,
        Sql = _sql.Insert,
        Parameters = entity,
        GeneratedKeyProperty = _sql.InsertReturnsKey ? _mapping.Key.Property : null,
    });

    /// <inheritdoc />
    public void Update(TEntity entity) => _unitOfWork.Enqueue(new PendingOperation
    {
        ConnectionName = _connectionName,
        Sql = _sql.Update,
        Parameters = entity,
    });

    /// <inheritdoc />
    public void Delete(TEntity entity) => _unitOfWork.Enqueue(new PendingOperation
    {
        ConnectionName = _connectionName,
        Sql = _sql.DeleteById,
        Parameters = entity,
    });

    /// <inheritdoc />
    public ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _unitOfWork.SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    public IQueryable<TEntity> AsQueryable()
        => throw new NotSupportedException(
            "Dapper repositories do not expose IQueryable. Use specifications or IDapperQueryExecutor.");

    /// <inheritdoc />
    public IQueryable<TEntity> AsNoTrackingQueryable() => AsQueryable();

    private async ValueTask<TEntity?> QueryFirstAsync(string sql, object parameters, CancellationToken cancellationToken)
    {
        var connection = await _unitOfWork.GetConnectionAsync(_connectionName, cancellationToken);
        var command = new CommandDefinition(sql, parameters, _unitOfWork.GetTransaction(_connectionName),
            cancellationToken: cancellationToken);
        return await connection.QueryFirstOrDefaultAsync<TEntity>(command);
    }

    private static string Compose(string baseSql, SqlWhere where)
        => where.IsEmpty ? baseSql : $"{baseSql} WHERE {where.Sql}";

    private static DynamicParameters ToParameters(SqlWhere where)
    {
        var parameters = new DynamicParameters();
        foreach (var (name, value) in where.Parameters)
            parameters.Add(name, value);
        return parameters;
    }

    private static string? ResolveConnectionName()
    {
        var attribute = typeof(TEntity).GetCustomAttribute<ConnectionStringNameAttribute>();
        return attribute?.Name;
    }
}

/// <summary>
/// Convenience <see cref="Guid"/>-keyed Dapper repository.
/// </summary>
public class DapperRepository<TEntity> : DapperRepository<TEntity, Guid>, IRepository<TEntity>
    where TEntity : Entity<Guid>
{
    /// <summary>Initializes a new instance of the <see cref="DapperRepository{TEntity}"/> class.</summary>
    public DapperRepository(
        IDapperUnitOfWork unitOfWork,
        IEntityMapRegistry mapRegistry,
        ISqlBuilder sqlBuilder,
        ISpecificationParser<SqlWhere> specificationParser)
        : base(unitOfWork, mapRegistry, sqlBuilder, specificationParser)
    {
    }
}
