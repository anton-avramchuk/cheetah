using Cheetah.Core.EntityFramework.Configuration;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.MasterData.DataAccess.Configurations;

public class SkillCategoryConfigurationOptions : EntityConfigurationOptions<SkillCategory, Guid>
{
    public override string Schema => Constants.SchemaName;
}

public class SkillCategoryConfiguration : EntityConfiguration<SkillCategory, Guid, SkillCategoryConfigurationOptions>
{
    protected override SkillCategoryConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<SkillCategory> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
