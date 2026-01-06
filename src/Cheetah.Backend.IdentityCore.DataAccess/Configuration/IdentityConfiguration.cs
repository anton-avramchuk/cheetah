using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Backend.IdentityCore.DataAccess.Configuration;

public abstract class IdentityConfiguration<TIdentityEntity> : IEntityTypeConfiguration<TIdentityEntity>
    where TIdentityEntity : class
{
    protected virtual string Schema { get; } = "identity";

    protected virtual string TableName { get; } = typeof(TIdentityEntity).Name;

    public abstract void Configure(EntityTypeBuilder<TIdentityEntity> builder);
}