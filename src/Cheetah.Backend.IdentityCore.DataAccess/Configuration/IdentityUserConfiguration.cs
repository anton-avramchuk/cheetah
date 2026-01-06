using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Backend.IdentityCore.Shared.Constants;
using Cheetah.Core.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Backend.IdentityCore.DataAccess.Configuration;

public abstract class IdentityUserConfiguration<TIdentityUser, TIdentityRole, TConfiguration> :
    AggregateRootConfiguration<TIdentityUser, Guid, TConfiguration>
    where TIdentityUser : IdentityUser<TIdentityRole>
    where TIdentityRole : IdentityRole
    where TConfiguration : IdentityUserConfigurationOptions<TIdentityUser, TIdentityRole>, new()
{
    protected override TConfiguration Options { get; } = new();

    public override void Configure(EntityTypeBuilder<TIdentityUser> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.UserName).IsRequired()
            .HasMaxLength(Options.UserNameMaxLength);
        builder.Property(u => u.NormalizedUserName).IsRequired()
            .HasMaxLength(Options.NormalizedUserNameLength);

        builder.Property(u => u.Email).IsRequired()
            .HasMaxLength(Options.EmailMaxLength);

        builder.Property(u => u.NormalizedEmail).IsRequired()
            .HasMaxLength(Options.NormalizedEmailMaxLength);
        builder.Property(u => u.PasswordHash).HasMaxLength(Options.PasswordHashMaxLength);
        builder.Property(u => u.SecurityStamp).IsRequired().HasMaxLength(Options.SecurityStampMaxLength);

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

public class IdentityUserConfigurationOptions<TIdentityUser, TIdentityRole> : AggregateRootConfigurationOptions<
    TIdentityUser,
    Guid>
    where TIdentityUser : IdentityUser<TIdentityRole>
    where TIdentityRole : IdentityRole
{
    public override string Schema => Constants.DefaultSchema;

    public int UserNameMaxLength { get; set; } = IdentityUserConstants.MaxUserNameLength;

    public int NormalizedUserNameLength { get; set; } = IdentityUserConstants.MaxNormalizedUserNameLength;

    public int NormalizedEmailMaxLength { get; set; } = IdentityUserConstants.MaxNormalizedEmailLength;

    public int EmailMaxLength { get; set; } = IdentityUserConstants.MaxEmailLength;

    public int PasswordHashMaxLength { get; set; } = IdentityUserConstants.MaxPasswordHashLength;

    public int SecurityStampMaxLength { get; set; } = IdentityUserConstants.MaxSecurityStampLength;
}