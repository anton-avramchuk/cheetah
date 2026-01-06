using Cheetah.Core.Domain;

namespace Cheetah.Core.EntityFramework.Configuration;

public abstract class AggregateRootConfigurationOptions<TEntity, TKey> : EntityConfigurationOptions<TEntity, TKey>
    where TEntity : AggregateRoot<TKey>
{
}