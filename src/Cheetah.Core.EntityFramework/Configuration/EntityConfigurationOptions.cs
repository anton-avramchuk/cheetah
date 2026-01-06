using Cheetah.Core.Domain;

namespace Cheetah.Core.EntityFramework.Configuration;

public abstract class EntityConfigurationOptions<TEntity> where TEntity : class, IEntity
{
    public virtual string TableName => typeof(TEntity).Name;

    public abstract string Schema { get; }
}

public abstract class EntityConfigurationOptions<TEntity, TKey> : EntityConfigurationOptions<TEntity>
    where TEntity : class, IEntity<TKey>
{
}

public abstract class AggregateRootConfigurationOptions<TEntity, TKey> : EntityConfigurationOptions<TEntity, TKey>
    where TEntity : AggregateRoot<TKey>
{
}