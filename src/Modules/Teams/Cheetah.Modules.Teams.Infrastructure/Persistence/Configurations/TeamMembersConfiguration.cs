using Cheetah.Core.EntityFramework.Configuration;
using Cheetah.Modules.Teams.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Teams.Infrastructure.Persistence.Configurations;

public abstract class TeamMembersConfigurationOptions
    : EntityConfigurationOptions<TeamMember, Guid>
{
    public override string TableName => "TeamMembers";
    public override string Schema => "teams";
}

public abstract class TeamMembersConfiguration<TConfiguration>
    : EntityConfiguration<TeamMember, Guid, TConfiguration>
    where TConfiguration : TeamMembersConfigurationOptions
{
    public override void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .HasMaxLength(256)
            .IsRequired();
    }
}
