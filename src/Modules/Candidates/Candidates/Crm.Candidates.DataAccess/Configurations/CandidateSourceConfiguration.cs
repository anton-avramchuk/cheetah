using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Core.EntityFramework.Configuration;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Candidates.DataAccess.Configurations;

public class CandidateSourceConfigurationOptions : EntityConfigurationOptions<CandidateSource, Guid>
{
    public override string Schema => "candidates";
}

public class CandidateSourceConfiguration : EntityConfiguration<CandidateSource, Guid, CandidateSourceConfigurationOptions>
{
    protected override CandidateSourceConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CandidateSource> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Order).IsRequired();
        builder.Property(x => x.Color)
            .HasMaxLength(9)
            .HasConversion(c => c != null ? c.Value : null, s => s != null ? Color.Create(s) : null);
    }
}
