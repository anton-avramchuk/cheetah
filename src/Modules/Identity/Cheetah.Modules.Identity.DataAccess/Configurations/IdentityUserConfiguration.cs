using Cheetah.Core.EntityFramework.Configuration;
using Cheetah.Core.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Identity.DataAccess.Configurations;

public abstract class IdentityUserConfigurationOptions<TIdentityUser, TIdentityRole>
    : AggregateRootConfigurationOptions<TIdentityUser, Guid>
    where TIdentityUser : IdentityUser<TIdentityRole>
    where TIdentityRole : IdentityRole
{
    public override string TableName => "Users";
    public override string Schema => "identity";
}

public abstract class IdentityUserConfiguration<TIdentityUser, TIdentityRole, TConfiguration>
    : AggregateRootConfiguration<TIdentityUser, Guid, TConfiguration>
    where TIdentityUser : IdentityUser<TIdentityRole>
    where TIdentityRole : IdentityRole
    where TConfiguration : IdentityUserConfigurationOptions<TIdentityUser, TIdentityRole>
{
    public override void Configure(EntityTypeBuilder<TIdentityUser> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.NormalizedUserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.SecurityStamp)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(1024);

        builder.HasIndex(x => x.NormalizedUserName).IsUnique();
        builder.HasIndex(x => x.NormalizedEmail);

        builder.Navigation(x => x.Roles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Claims)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Claims)
            .WithOne()
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
