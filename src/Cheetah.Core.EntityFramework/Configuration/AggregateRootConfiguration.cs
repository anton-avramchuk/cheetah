using Cheetah.Core.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Core.EntityFramework.Configuration;

public abstract class AggregateRootConfiguration<TEntity, TKey, TConfiguration> :
    EntityConfiguration<TEntity, TKey, TConfiguration>
    where TEntity : AggregateRoot<TKey>
    where TConfiguration : AggregateRootConfigurationOptions<TEntity, TKey>
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Ignore(x => x.DomainEvents);
    }
}