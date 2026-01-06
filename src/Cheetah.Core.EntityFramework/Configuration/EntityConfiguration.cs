using Cheetah.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Core.EntityFramework.Configuration;

public abstract class EntityConfiguration<TEntity, TConfiguration> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IEntity
    where TConfiguration : EntityConfigurationOptions<TEntity>
{
    protected abstract TConfiguration Options { get; }


    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ToTable(Options.TableName, Options.Schema);
    }
}

public abstract class EntityConfiguration<TEntity, TKey, TConfiguration> : EntityConfiguration<TEntity, TConfiguration>
    where TEntity : class, IEntity<TKey>
    where TConfiguration : EntityConfigurationOptions<TEntity, TKey>
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);
        builder.HasKey(x => x.Id);
    }
}