using Cheetah.Backend.IdentityCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Backend.IdentityCore.DataAccess.Configuration;

public abstract class
    IdentityUserRoleConfiguration<TIdentityRole, TIdentityUser> : IdentityConfiguration<IdentityUserRole<TIdentityRole>>
    where TIdentityRole : IdentityRole
    where TIdentityUser : IdentityUser<TIdentityRole>
{
    protected override string TableName => "UserRoles";

    public override void Configure(EntityTypeBuilder<IdentityUserRole<TIdentityRole>> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.HasOne(x => x.Role).WithMany().HasForeignKey(ur => ur.RoleId).IsRequired();
        builder.HasOne<TIdentityUser>().WithMany(u => u.Roles).HasForeignKey(ur => ur.UserId).IsRequired();

        builder.HasIndex(ur => new { ur.RoleId, ur.UserId });
    }
}