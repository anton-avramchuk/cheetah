using Cheetah.Core.EntityFramework.Configuration;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.MasterData.DataAccess.Configurations;

public class SkillConfigurationOptions : EntityConfigurationOptions<Skill, Guid>
{
    public override string Schema => Constants.SchemaName;
}

public class SkillConfiguration : EntityConfiguration<Skill, Guid, SkillConfigurationOptions>
{
    protected override SkillConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<Skill> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
