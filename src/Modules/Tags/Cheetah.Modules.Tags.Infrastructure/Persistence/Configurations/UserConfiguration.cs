using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Tags.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "tags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever(); // id приходит из Identity
        builder.Property(x => x.UserName).HasMaxLength(TagsConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.SyncHash).HasMaxLength(64).IsRequired(); // SHA-256 hex
    }
}
