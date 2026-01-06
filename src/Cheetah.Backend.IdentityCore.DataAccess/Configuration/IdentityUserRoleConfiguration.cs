using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Backend.IdentityCore.DataAccess.Configuration;

public abstract class IdentityUserRoleConfiguration<TIdentityRole, TIdentityUser> : 
    EntityConfiguration<IdentityUserRole<TIdentityRole>,
    IdentityUserRoleConfigurationOptions<TIdentityRole>>
    where TIdentityRole : IdentityRole
    where TIdentityUser : IdentityUser<TIdentityRole>
{
    public override void Configure(EntityTypeBuilder<IdentityUserRole<TIdentityRole>> builder)
    {
        base.Configure(builder);
        
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.HasOne(x => x.Role).WithMany().HasForeignKey(ur => ur.RoleId).IsRequired();
        builder.HasOne<TIdentityUser>().WithMany(u => u.Roles).HasForeignKey(ur => ur.UserId).IsRequired();

        builder.HasIndex(ur => new { ur.RoleId, ur.UserId });
    }
}

public class IdentityUserRoleConfigurationOptions<TIdentityRole> : 
    EntityConfigurationOptions<IdentityUserRole<TIdentityRole>>
    where TIdentityRole : IdentityRole
{
    public override string Schema => Constants.DefaultSchema;
}