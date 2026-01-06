using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Backend.IdentityCore.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Backend.IdentityCore.DataAccess.Configuration;

public abstract class IdentityRoleConfiguration<TIdentityRole> : IdentityConfiguration<TIdentityRole>
    where TIdentityRole : IdentityRole
{
    public override void Configure(EntityTypeBuilder<TIdentityRole> builder)
    {
        builder.ToTable(TableName, Schema);


        builder.Property(r => r.Name).IsRequired().HasMaxLength(IdentityRoleConstants.MaxNameLength);
        builder.Property(r => r.NormalizedName).IsRequired()
            .HasMaxLength(IdentityRoleConstants.MaxNormalizedNameLength);

        builder.HasMany(r => r.Claims).WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();

        builder.HasIndex(r => r.NormalizedName);

        var claimsNavigation =
            builder.Metadata.FindNavigation(nameof(IdentityRole.Claims))!;


        claimsNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}