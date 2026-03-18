using Cheetah.Core.EntityFramework.Configuration;
using Cheetah.Core.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Identity.DataAccess.Configurations;

public abstract class IdentityUserRoleConfigurationOptions<TIdentityRole>
    : EntityConfigurationOptions<IdentityUserRole<TIdentityRole>>
    where TIdentityRole : IdentityRole
{
    public override string TableName => "UserRoles";
    public override string Schema => "identity";
}

public abstract class IdentityUserRoleConfiguration<TIdentityRole, TConfiguration>
    : EntityConfiguration<IdentityUserRole<TIdentityRole>, TConfiguration>
    where TIdentityRole : IdentityRole
    where TConfiguration : IdentityUserRoleConfigurationOptions<TIdentityRole>
{
    public override void Configure(EntityTypeBuilder<IdentityUserRole<TIdentityRole>> builder)
    {
        base.Configure(builder);

        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
