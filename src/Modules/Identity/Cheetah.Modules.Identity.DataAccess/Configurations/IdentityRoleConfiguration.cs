using Cheetah.Core.EntityFramework.Configuration;
using Cheetah.Core.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Identity.DataAccess.Configurations;

public abstract class IdentityRoleConfigurationOptions<TIdentityRole>
    : AggregateRootConfigurationOptions<TIdentityRole, Guid>
    where TIdentityRole : IdentityRole
{
    public override string TableName => "Roles";
    public override string Schema => "identity";
}

public abstract class IdentityRoleConfiguration<TIdentityRole, TConfiguration>
    : AggregateRootConfiguration<TIdentityRole, Guid, TConfiguration>
    where TIdentityRole : IdentityRole
    where TConfiguration : IdentityRoleConfigurationOptions<TIdentityRole>
{
    public override void Configure(EntityTypeBuilder<TIdentityRole> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.NormalizedName).IsUnique();

        builder.Navigation(x => x.Claims)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Claims)
            .WithOne()
            .HasForeignKey(rc => rc.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
