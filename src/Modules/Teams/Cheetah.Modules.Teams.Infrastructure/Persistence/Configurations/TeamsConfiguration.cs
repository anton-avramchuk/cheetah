using Cheetah.Core.EntityFramework.Configuration;
using Cheetah.Modules.Teams.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Teams.Infrastructure.Persistence.Configurations;

public abstract class TeamsConfigurationOptions
    : EntityConfigurationOptions<TeamBase, Guid>
{
    public override string TableName => "Teams";
    public override string Schema => "teams";
}

public abstract class TeamsConfiguration<TConfiguration>
    : EntityConfiguration<TeamBase, Guid, TConfiguration>
    where TConfiguration : TeamsConfigurationOptions
{
    public override void Configure(EntityTypeBuilder<TeamBase> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.HasIndex(x=>x.Name)
            .IsUnique();
    }
}
