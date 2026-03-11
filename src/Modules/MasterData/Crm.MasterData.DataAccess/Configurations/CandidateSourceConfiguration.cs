using Cheetah.Core.EntityFramework.Configuration;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.MasterData.DataAccess.Configurations;

public class CandidateSourceConfigurationOptions : EntityConfigurationOptions<CandidateSource, Guid>
{
    public override string Schema => Constants.SchemaName;
}

public class CandidateSourceConfiguration : EntityConfiguration<CandidateSource, Guid, CandidateSourceConfigurationOptions>
{
    protected override CandidateSourceConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CandidateSource> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
