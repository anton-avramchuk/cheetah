using Cheetah.Core.EntityFramework.Configuration;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Candidates.DataAccess.Configurations;

public class CandidateStageConfigurationOptions : EntityConfigurationOptions<CandidateStage, Guid>
{
    public override string Schema => "candidates";
}

public class CandidateStageConfiguration : EntityConfiguration<CandidateStage, Guid, CandidateStageConfigurationOptions>
{
    protected override CandidateStageConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CandidateStage> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Order).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(7);
        builder.Property(x => x.IsDefault).IsRequired();

        builder.Navigation(x => x.CandidateApplications)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
