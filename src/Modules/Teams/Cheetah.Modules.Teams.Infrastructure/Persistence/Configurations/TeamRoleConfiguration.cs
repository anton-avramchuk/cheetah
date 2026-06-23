using Cheetah.Core.EntityFramework.Configuration;
using Cheetah.Modules.Teams.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Teams.Infrastructure.Persistence.Configurations;

public abstract class TeamRoleConfigurationOptions
    : EntityConfigurationOptions<TeamRoleBase, Guid>
{
    public override string TableName => "TeamRoles";
    public override string Schema => "teams";
}

public abstract class TeamRoleConfiguration<TConfiguration>
    : EntityConfiguration<TeamRoleBase, Guid, TConfiguration>
    where TConfiguration : TeamRoleConfigurationOptions
{
    public override void Configure(EntityTypeBuilder<TeamRoleBase> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.HasIndex(x=>x.Name)
            .IsUnique();
    }
}
