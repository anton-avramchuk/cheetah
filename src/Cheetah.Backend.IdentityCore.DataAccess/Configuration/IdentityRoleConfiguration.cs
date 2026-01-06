using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Backend.IdentityCore.Shared.Constants;
using Cheetah.Core.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Backend.IdentityCore.DataAccess.Configuration;

public abstract class IdentityRoleConfiguration : AggregateRootConfiguration<IdentityRole,Guid,IdentityRoleConfigurationOptions>
{
    protected override IdentityRoleConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        base.Configure(builder);

        builder.Property(r => r.Name).IsRequired().HasMaxLength(Options.NameMaxLength);
        builder.Property(r => r.NormalizedName).IsRequired()
            .HasMaxLength(Options.NormalizedNameMaxLength);

        builder.HasMany(r => r.Claims).WithOne()
            .HasForeignKey(rc => rc.RoleId).IsRequired();

        builder.HasIndex(r => r.NormalizedName);

        var claimsNavigation =
            builder.Metadata.FindNavigation(nameof(IdentityRole.Claims))!;


        claimsNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class IdentityRoleConfigurationOptions : AggregateRootConfigurationOptions<IdentityRole, Guid>
{
    public override string Schema => Constants.DefaultSchema;

    public int NameMaxLength { get; set; } = IdentityRoleConstants.MaxNameLength;

    public int NormalizedNameMaxLength { get; set; }= IdentityRoleConstants.MaxNormalizedNameLength;
}