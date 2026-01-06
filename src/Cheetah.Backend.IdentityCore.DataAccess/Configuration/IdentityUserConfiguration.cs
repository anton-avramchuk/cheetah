using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Backend.IdentityCore.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Backend.IdentityCore.DataAccess.Configuration;

public abstract class IdentityUserConfiguration<TIdentityUser, TIdentityRole> : IdentityConfiguration<TIdentityUser>
    where TIdentityUser : IdentityUser<TIdentityRole>
    where TIdentityRole : IdentityRole
{
    public override void Configure(EntityTypeBuilder<TIdentityUser> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserName).IsRequired().HasMaxLength(IdentityUserConstants.MaxUserNameLength);
        builder.Property(u => u.NormalizedUserName).IsRequired()
            .HasMaxLength(IdentityUserConstants.MaxNormalizedUserNameLength);
        builder.Property(u => u.NormalizedEmail).IsRequired()
            .HasMaxLength(IdentityUserConstants.MaxNormalizedEmailLength);
        builder.Property(u => u.PasswordHash).HasMaxLength(IdentityUserConstants.MaxPasswordHashLength);
        builder.Property(u => u.SecurityStamp).IsRequired().HasMaxLength(IdentityUserConstants.MaxSecurityStampLength);

        builder.Property(u => u.LockoutEnabled).HasDefaultValue(false);

        builder.Property(u => u.AccessFailedCount);

        var claimsNavigation =
            builder.Metadata.FindNavigation(nameof(IdentityUser<>.Claims))!;


        claimsNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        var rolesNavigation =
            builder.Metadata.FindNavigation(nameof(IdentityUser<>.Roles))!;


        rolesNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);


        builder.HasMany(u => u.Claims).WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
        builder.HasMany(u => u.Roles).WithOne().HasForeignKey(ur => ur.UserId).IsRequired();

        builder.HasIndex(u => u.NormalizedUserName).IsUnique();
        builder.HasIndex(u => u.NormalizedEmail).IsUnique();
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}