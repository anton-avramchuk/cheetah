using Cheetah.Core.EntityFramework.Configuration;
using Crm.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Identity.DataAccess.Configurations;

public class UserIdentityConfigurationOptions : AggregateRootConfigurationOptions<UserIdentity, Guid>
{
    public override string Schema => "identity";
}

public class
    UserIdentityConfiguration : AggregateRootConfiguration<UserIdentity, Guid, UserIdentityConfigurationOptions>
{
    protected override UserIdentityConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<UserIdentity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}